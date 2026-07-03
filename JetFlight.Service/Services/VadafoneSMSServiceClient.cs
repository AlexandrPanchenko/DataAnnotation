using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using JetFlight.Shared;
using JetFlight.Shared.Models.Shared;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace JetFlight.Service.Services;
public interface ISMSServiceClient
{
    Task<string> GetTokenAsync();
    Task SendSMSAsync(string token, string senderId, List<string> receivers, string message);
}

public class VodafoneSMSServiceClient : ISMSServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly SmsSettings _smsSettings;
    private readonly IDistributedCache _cache;
    private readonly ILogger _logger;

    public VodafoneSMSServiceClient(HttpClient httpClient,
        IOptions<SmsSettings> smsSettings,
        IDistributedCache cache,
        ILogger logger)
    {
        _httpClient = httpClient;
        _smsSettings = smsSettings.Value;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync()
    {
        var cachedToken = await _cache.GetStringAsync("vodafone_token");
        if (!string.IsNullOrEmpty(cachedToken))
        {
            _logger.Information("[VodafoneSMSServiceClient.GetTokenAsync] Using cached token");
            return cachedToken;
        }

        _logger.Information("[VodafoneSMSServiceClient.GetTokenAsync] Requesting new token from Vodafone API");
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_smsSettings.BaseUrl}/uaa/oauth/token");

        using (var client = new HttpClient())
        {
            string baseUrl = $"{_smsSettings.BaseUrl}/uaa/oauth/token";
            var clientCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("webapp:webapp"));
            var username = Environment.GetEnvironmentVariable("SMS_AUTH_EMAIL");
            var password = Environment.GetEnvironmentVariable("SMS_AUTH_PASSWORD");
            
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.Error("[VodafoneSMSServiceClient.GetTokenAsync] SMS_AUTH_EMAIL environment variable is not set");
                throw new InvalidOperationException("SMS_AUTH_EMAIL environment variable is not set");
            }
            
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.Error("[VodafoneSMSServiceClient.GetTokenAsync] SMS_AUTH_PASSWORD environment variable is not set");
                throw new InvalidOperationException("SMS_AUTH_PASSWORD environment variable is not set");
            }
            
            string queryParams = $"?grant_type=password&username={username}&password={password}";
            string fullUrl = $"{baseUrl}{queryParams}";

            var content = new StringContent(""); // Use an empty string or your actual content
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", clientCredentials);

            var response = await client.PostAsync(fullUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("[VodafoneSMSServiceClient.GetTokenAsync] Failed to get token. StatusCode: {StatusCode}, Response: {Response}", 
                    response.StatusCode, responseString);
                response.EnsureSuccessStatusCode();
            }
            
            var tokenResponse = JObject.Parse(responseString);
            var token = tokenResponse["access_token"].ToString();
            var expiresIn = tokenResponse["expires_in"].ToObject<int>();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiresIn - 60)
            };
            await _cache.SetStringAsync("vodafone_token", token, cacheOptions);

            _logger.Information("[VodafoneSMSServiceClient.GetTokenAsync] Successfully obtained token, expires in {ExpiresIn} seconds", expiresIn);
            return token;
        }
    }

    public async Task SendSMSAsync(string token, string senderId, List<string> receivers, string message)
    {
        _logger.Information("[VodafoneSMSServiceClient.SendSMSAsync] Preparing SMS request - SenderId: {SenderId}, Receivers: {Receivers}, MessageLength: {MessageLength}", 
            senderId, string.Join(", ", receivers), message?.Length ?? 0);

        if (string.IsNullOrWhiteSpace(senderId))
        {
            _logger.Error("[VodafoneSMSServiceClient.SendSMSAsync] SenderId is null or empty");
            throw new ArgumentException("SenderId is required");
        }

        if (receivers == null || receivers.Count == 0)
        {
            _logger.Error("[VodafoneSMSServiceClient.SendSMSAsync] Receivers list is null or empty");
            throw new ArgumentException("Receivers are required");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_smsSettings.BaseUrl}/communication-event/api/communicationManagement/v3/communicationMessage/send");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        var smsContent = new
        {
            receiver = receivers,
            cascades = new[]
            {
            new
            {
                transport = "SMS",
                senderId = senderId,
                validityPeriod = "1",
                messageObject = new
                {
                    type = "SMS",
                    smsMessage = new
                    {
                        content = message
                    }
                }
            }
        }
        };
        var json = JsonConvert.SerializeObject(smsContent);
        _logger.Information("[VodafoneSMSServiceClient.SendSMSAsync] Request JSON: {RequestJson}", json);
        
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        request.Content = content;
        
        _logger.Information("[VodafoneSMSServiceClient.SendSMSAsync] Sending request to Vodafone API - URL: {Url}, SenderId: {SenderId}", 
            request.RequestUri, senderId);
        
        var response = await _httpClient.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        
        _logger.Information("[VodafoneSMSServiceClient.SendSMSAsync] Vodafone API response - StatusCode: {StatusCode}, SenderId: {SenderId}, Response: {Response}", 
            response.StatusCode, senderId, responseString);

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error("[VodafoneSMSServiceClient.SendSMSAsync] Vodafone API returned error - StatusCode: {StatusCode}, SenderId: {SenderId}, Response: {Response}", 
                response.StatusCode, senderId, responseString);
            
            // Try to parse error details from response
            try
            {
                var errorResponse = JObject.Parse(responseString);
                var errorMessage = errorResponse["message"]?.ToString() ?? errorResponse["error"]?.ToString() ?? responseString;
                _logger.Error("[VodafoneSMSServiceClient.SendSMSAsync] Parsed error message: {ErrorMessage}", errorMessage);
            }
            catch
            {
                // If parsing fails, just log the raw response
            }
            
            response.EnsureSuccessStatusCode();
        }
        
        var responseModel = await response.Content.ReadFromJsonAsync<SmsResponseModel>();
        if (string.IsNullOrEmpty(responseModel?.MessageId))
        {
            _logger.Error("[VodafoneSMSServiceClient.SendSMSAsync] Response model is null or MessageId is empty - SenderId: {SenderId}, Response: {Response}", 
                senderId, responseString);
            throw new HttpRequestException($"Помилка при надсилання повідомлення. Response: {responseString}");
        }
        
        _logger.Information("[VodafoneSMSServiceClient.SendSMSAsync] SMS successfully sent - MessageId: {MessageId}, SenderId: {SenderId}", 
            responseModel.MessageId, senderId);
    }
}