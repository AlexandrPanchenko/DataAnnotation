using JetFlight.Shared.Models.SendGrid;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models.Subscription;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using System.Text;
using System.Text.Json;

namespace JetFlight.Service.Services;

public interface ISubscriptionService
{
    Task SubscribeAsync(SubscriptionRequest contact, Branches branchId, CancellationToken cancellationToken);
    Task<ExistingContact?> GetContactAsync(Branches branchId, string phoneNumber, string? email, CancellationToken cancellationToken);
    Task UnsubscribeAsync(Guid sendGridContactId, CancellationToken cancellationToken);
}

public class SubscriptionService(ISendGridClient sendGridClient, IOptions<EmailSettings> emailSettings, ILogger<SubscriptionService> logger) : ISubscriptionService
{
    private readonly ISendGridClient _sendGridClient = sendGridClient;
    private readonly EmailSettings _emailSettings = emailSettings.Value;
    private readonly ILogger<SubscriptionService> _logger = logger;

    private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<ExistingContact?> GetContactAsync(Branches branchId, string phoneNumber, string? email, CancellationToken cancellationToken)
    {
        var listName = _emailSettings.SubscriptionList[branchId];
        var listId = (await GetMarketingListId(listName, cancellationToken))
            ?? await CreateMarketingListAsync(listName, cancellationToken);

        var queryBuilder = new StringBuilder();
        queryBuilder.Append($"phone_number_id = '{phoneNumber}'");
        if (email != null)
        {
            queryBuilder.Append($" AND email = '{email}'");
        }
        else
        {
            queryBuilder.Append($" AND email IS NULL");
        }

        queryBuilder.Append($" AND CONTAINS(list_ids, '{listId}')");

        var query = queryBuilder.ToString();
        var request = new ContactSearchRequest
        {
            Query = query,
        };

        var requestJson = JsonSerializer.Serialize(request);

        var response = await _sendGridClient.RequestAsync(BaseClient.Method.POST, requestJson, urlPath: "/marketing/contacts/search", cancellationToken: cancellationToken);
        await ThrowIfErrorAsync(response, "Помилка при зчитуванні контактів.");

        var item = await DeserializeResponseAsync<GetContactListResponse>(response, cancellationToken);
        return item.Result.FirstOrDefault();
    }

    public async Task UnsubscribeAsync(Guid sendGridContactId, CancellationToken cancellationToken)
    {
        var query = "ids=" + sendGridContactId.ToString();
        var response = await _sendGridClient.RequestAsync(BaseClient.Method.DELETE, urlPath: "/v3/marketing/contacts", queryParams: query, cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Body.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("SendGrid unsubscribe failed for contact {ContactId}. StatusCode: {StatusCode}. Body: {Body}",
                sendGridContactId, response.StatusCode, errorContent);

            var statusCode = (int)response.StatusCode;
            if (statusCode >= 400 && statusCode < 500)
            {
                return;
            }

            throw new Exception($"Помилка при відписці контакту в SendGrid. StatusCode: {response.StatusCode}. Body: {errorContent}");
        }
    }

    public async Task SubscribeAsync(SubscriptionRequest contact, Branches branchId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(contact.Email) && string.IsNullOrEmpty(contact.PhoneNumber))
        {
            throw new ArgumentException("Хоча б 1 ідентифікатор необхідний");
        }

        var listName = _emailSettings.SubscriptionList[branchId];

        var listId = (await GetMarketingListId(listName, cancellationToken))
            ?? await CreateMarketingListAsync(listName, cancellationToken);

        await UpsertContact(listId, contact, cancellationToken);
    }

    private async Task<Guid?> GetMarketingListId(string listName, CancellationToken cancellationToken)
    {
        var response = await _sendGridClient.RequestAsync(BaseClient.Method.GET, urlPath: "/marketing/lists", cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Body.ReadAsStringAsync();
            throw new Exception($"Помилка при отриманні списків контактів. API error: {errorContent}");
        }

        var existingLists = await DeserializeResponseAsync<GetMarketingListResponse>(response, cancellationToken);

        var listId = existingLists.Result.FirstOrDefault(x => x.Name == listName)?.Id;
        return listId;
    }

    private async Task<Guid> CreateMarketingListAsync(string name, CancellationToken cancellationToken)
    {
        var request = new CreateMarketingListRequest
        {
            Name = name,
        };

        var requestJson = JsonSerializer.Serialize(request);

        var createResponse = await _sendGridClient.RequestAsync(BaseClient.Method.POST, requestJson, urlPath: "/marketing/lists", cancellationToken: cancellationToken);
        await ThrowIfErrorAsync(createResponse, "Помилка при створенні списку контактів.");

        var item = await DeserializeResponseAsync<MarketingList>(createResponse, cancellationToken);
        return item.Id;
    }

    private async Task UpsertContact(Guid listId, SubscriptionRequest contact, CancellationToken cancellationToken)
    {
        var request = new CreateContactRequest
        {
            Contacts = new List<Contact>
            {
                new Contact
                {
                    Email = contact.Email,
                    PhoneNumber = contact.PhoneNumber,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    ListIds = new List<Guid> { listId },
                }
            },
            ListIds = new List<Guid> { listId },
        };
        var requestJson = JsonSerializer.Serialize(request, jsonSerializerOptions);

        var response = await _sendGridClient.RequestAsync(BaseClient.Method.PUT, requestJson, urlPath: "/marketing/contacts", cancellationToken: cancellationToken);
        await ThrowIfErrorAsync(response, "Помилка при створенні контакту.");
    }

    private async Task ThrowIfErrorAsync(Response response, string message)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorString = await response.Body.ReadAsStringAsync();
            _logger.LogError($"{message} Code: {0}. Response: {1}", response.StatusCode, errorString);
            throw new HttpRequestException(message);
        }
    }

    private async Task<T> DeserializeResponseAsync<T>(Response response, CancellationToken cancellationToken)
    {
        var responseJson = await response.Body.ReadAsStringAsync(cancellationToken);
        var model = JsonSerializer.Deserialize<T>(responseJson, jsonSerializerOptions);
        return model!;
    }
}
