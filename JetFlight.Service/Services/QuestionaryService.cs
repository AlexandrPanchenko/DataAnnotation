using JetFlight.ApplicationDataAccess;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Extensions;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Helpers;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.Coupons;
using JetFlight.Shared.Models.Export;
using JetFlight.Shared.Models.Questionary;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models.Users;
using JetFlight.Shared.UserContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using JetFlight.Shared.Models.Message;

namespace JetFlight.Service.Services
{
    public interface IQuestionaryService
    {
        Task<AdminQuestionaryDTO> GetAsync(int id);
        Task<PagedListDTO<AdminQuestionaryDTO>> GetAllAsync(
            PagingDTO pagingDTO,
            byte? branchId,
            DateOnly? date = null,
            QuestionaryStatus? status = null);
        Task DeleteAsync(int id);
        Task UpdateAsync(UpdateQuestionaryDTO model);
        Task<AdminQuestionaryDTO> CreateAsync(CreateQuestionaryDTO model);

        Task<List<CustomerQuestionaryDTO>> GetCustomerQuestionariesAsync();

        Task<CustomerQuestionaryDTO> GetCustomerQuestionaryByIdAsync(int id);

        Task<PagedListDTO<CustomerQuestionaryAnswerDTO>> GetAnswersAsync(
            PagingDTO pagingDTO,
            int questionaryId,
            byte? branchId = null,
            ClientPlatform? clientPlatform = null,
            DateOnly? date = null);

        Task<QuestionaryAnswerResponse> AnswerAsync(QuestionaryAnswerDTO model, ClientPlatform platform, byte branchId);

        Task<ExportFile> ExportAsync(
            int id,
            ExportFileFormat format,
            byte? branchId = null,
            ClientPlatform? clientPlatform = null,
            DateOnly? date = null);

        Task ArchiveAsync(int id);

        Task PublishAsync(int id);

        Task DeactivateAsync(int id);
    }

    public partial class QuestionaryService : IQuestionaryService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IntegrationDataContext _integrationDataContext;
        private readonly IUserContext _userContext;
        private readonly IAvatarService _avatarService;
        private readonly IJobSchedulerService _jobSchedulerService;
        private readonly ApplicationDataContext _applicationDataContext;
        private readonly INotificationService _notificationService;
        private readonly IHtmlGenerationService _htmlGenerationService;
        private readonly ILogger<QuestionaryService> _logger;

        [GeneratedRegex(RegexConstants.Email)]
        private static partial Regex EmailRegex();

        public QuestionaryService(
            IServiceProvider serviceProvider,
            IntegrationDataContext integrationDataContext,
            IUserContext userContext,
            IAvatarService avatarService,
            IJobSchedulerService jobSchedulerService,
            ApplicationDataContext applicationDataContext,
            INotificationService notificationService,
            IHtmlGenerationService htmlGenerationService,
            ILogger<QuestionaryService> logger)
        {
            _serviceProvider = serviceProvider;
            _integrationDataContext = integrationDataContext;
            _userContext = userContext;
            _avatarService = avatarService;
            _jobSchedulerService = jobSchedulerService;
            _applicationDataContext = applicationDataContext;
            _notificationService = notificationService;
            _htmlGenerationService = htmlGenerationService;
            _logger = logger;
        }

        public async Task<AdminQuestionaryDTO> CreateAsync(CreateQuestionaryDTO model)
        {
            var entity = await SetEntityAsync(model);

            entity.CreatedAt = DateTime.UtcNow.SetKindUtc();
            entity.Status = QuestionaryStatus.Inactive;

            await _integrationDataContext.Questionaries.AddAsync(entity);
            await _integrationDataContext.SaveChangesAsync();

            return ToAdminDTO(entity, 0);
        }

        public async Task PublishAsync(int id)
        {
            var entity = await _integrationDataContext.Questionaries
                .Include(x => x.Coupon)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                throw new ArgumentException("Опитувальник не знайден");
            }

            if (entity.Status != QuestionaryStatus.Inactive)
            {
                throw new ArgumentException("Тільки неактивний опитувальник можна запаблішити");
            }

            if (entity.ExpirationDate <= DateTime.UtcNow)
            {
                throw new ArgumentException("Змініть дату закінчення на майбутній час, щоб запаблішити опитувальник");
            }

            if (entity.Coupon != null && entity.Coupon.Status != CouponStatus.Active)
            {
                throw new ArgumentException("Купон, який є нагородою більше не доступний");
            }

            if (entity.Coupon != null && entity.Coupon.StartDate > DateTime.UtcNow)
            {
                throw new ArgumentException("Не можна опублікувати опитувальник, бо заасайнений купон починає діяти в майбутньому");
            }

            entity.UpdatedAt = DateTime.UtcNow.SetKindUtc();
            entity.Status = QuestionaryStatus.Activated;

            await _integrationDataContext.SaveChangesAsync();

            if (entity.ExpirationDate <= DateTime.UtcNow.AddHours(JobConstants.LoyaltyExpirationJobIntervalHours))
            {
                await _jobSchedulerService.SetQuestionaryExpirationJobAsync(entity.Id, entity.ExpirationDate, true);
            }
        }

        public async Task ArchiveAsync(int id)
        {
            var entity = await _integrationDataContext.Questionaries
                .FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                throw new ArgumentException("Опитувальник не знайден");
            }

            if (entity.IsLocked)
            {
                throw new ArgumentException("Цей опитувальник неможливо архівувати");
            }

            if (entity.Status == QuestionaryStatus.Archived)
            {
                throw new ArgumentException("Опитувальник вже заархівований");
            }

            entity.UpdatedAt = DateTime.UtcNow.SetKindUtc();
            entity.Status = QuestionaryStatus.Archived;

            await _integrationDataContext.SaveChangesAsync();

            await _jobSchedulerService.RemoveQuestionaryExpirationJobAsync(entity.Id);
        }

        public async Task UpdateAsync(UpdateQuestionaryDTO model)
        {
            var entity = await _integrationDataContext.Questionaries
                .Include(x => x.Fields)
                .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (entity == null)
            {
                throw new ArgumentException("Опитувальник не знайден");
            }

            if (entity.Status != QuestionaryStatus.Inactive)
            {
                throw new ArgumentException("Тільки неактивний опитувальник можно редагувати");
            }

            if (await _integrationDataContext.QuestionaryAnswers.AnyAsync(x => x.QuestionaryId == model.Id))
            {
                throw new ArgumentException("Опитувальник з відповідями редагувати не можна");
            }

            if (entity.IsLocked)
            {
                throw new ArgumentException("Цей опитувальник неможливо редагувати");
            }

            await SetEntityAsync(model, entity);
            entity.UpdatedAt = DateTime.UtcNow.SetKindUtc();

            await _integrationDataContext.SaveChangesAsync();
        }

        private async Task<Questionary> SetEntityAsync(CreateQuestionaryDTO model, Questionary? entity = null)
        {
            entity ??= new Questionary
            {
                Fields = new List<QuestionaryField>(),
            };

            if (!model.Fields.Any(x => x.IsRequired))
            {
                throw new ArgumentException("Додайте хоча б 1 обов'язкове питання");
            }

            entity.BranchId = model.BranchId;

            entity.ExpirationDate = model.ExpirationDate.SetKindUtc();

            entity.BonusReward = model.Reward is QuestionaryBonusRewardDTO a ? a.Amount : null;
            if (model.Reward is QuestionaryCouponRewardDTO c)
            {
                var coupon = await _integrationDataContext.Coupons.FirstOrDefaultAsync(
                    x =>
                        x.Id == c.CouponId
                        && x.Status == CouponStatus.Active
                        && x.Class == CouponClass.Personal
                        && x.ExpirationDate >= entity.ExpirationDate
                        && !x.IsCardCoupon);

                if (coupon == null)
                {
                    throw new ArgumentException("Обраний ваучер не знайден або не валідний");
                }

                entity.CouponId = coupon.Id;
            }

            entity.ActiveDaysAfterComplete = model.ActiveDaysAfterComplete;
            entity.Image = model.Image;
            entity.Alt = model.Alt;
            entity.Name = model.Name;
            entity.Fields.Clear();

            var set = new HashSet<string>();
            var disallowedNames = new string[]
            {
                "CustomerId",
                "CreatedAt",
                "CustomerName",
                "Branch",
                "ClientPlatfrom"
            };

            for (int i = 0; i < model.Fields.Count; i++)
            {
                if (disallowedNames.Contains(model.Fields[i].Name))
                {
                    throw new ArgumentException($"{model.Fields[i].Name} заборонено використовувати");
                }

                if (!set.Add(model.Fields[i].Name))
                {
                    throw new ArgumentException($"{model.Fields[i].Name} використовується більше 1 разу");
                }

                var result = new QuestionaryField
                {
                    Position = i + 1,
                    IsRequired = model.Fields[i].IsRequired,
                    Name = model.Fields[i].Name,
                    Type = model.Fields[i].Type,
                    Options = new List<QuestionarySelectOption>(),
                    Validation = model.Fields[i].Validation,
                };

                if (model.Fields[i].Type == QuestionaryItemType.Select || model.Fields[i].Type == QuestionaryItemType.Multiselect)
                {
                    if (!model.Fields[i].Options.Any())
                    {
                        throw new ArgumentException("Додайте хоча б 1 відповідь до селекту");
                    }

                    result.Options.AddRange(model.Fields[i].Options.Select(x => new QuestionarySelectOption
                    {
                        Key = x.Key,
                        Value = x.Value,
                    }));
                }

                entity.Fields.Add(result);
            }

            return entity;
        }

        private Task<List<Questionary>> GetCustomerEntitiesAsync()
        {
            var now = DateTime.UtcNow.SetKindUtc();
            var branchId = (byte) _userContext.BranchId.Value;
            const int fetchCount = 3;
            return _integrationDataContext.Questionaries
                .Include(x => x.Fields)
                    .ThenInclude(x => x.Options)
                .Include(x => x.Answers.Where(x => x.CustomerId == _userContext.CustomerId))
                .Include(x => x.Coupon)
                .Where(x =>
                    (!x.BranchId.HasValue || x.BranchId == (int)_userContext.BranchId)
                    && x.Status == QuestionaryStatus.Activated
                    && x.ExpirationDate > now
                    && (!x.BranchId.HasValue || x.BranchId == branchId)
                    && (
                        (x.IsLocked && !x.Answers.Any(a => a.CustomerId == _userContext.CustomerId))
                        || !x.Answers.Any(a => a.CustomerId == _userContext.CustomerId)
                        || x.Answers.Any(a => a.CreatedAt.AddDays(x.ActiveDaysAfterComplete).Date > now && a.CustomerId == _userContext.CustomerId))
                        )
                .OrderByDescending(x => x.IsLocked)
                    .ThenBy(x => x.Id)
                .Take(fetchCount)
                .ToListAsync();
        }

        public async Task<List<CustomerQuestionaryDTO>> GetCustomerQuestionariesAsync()
        {
            var entities = await GetCustomerEntitiesAsync();

            // Filter out personal questionnaire if it was already completed
            var customer = await _integrationDataContext.Customers
                .FirstOrDefaultAsync(x => x.Id == _userContext.CustomerId);
            
            var filteredEntities = entities.Where(e => 
                !(e.IsLocked && e.Name == PersonalDataQuestionaryConstants.Name && 
                  customer != null && customer.PersonalQuestionaryCompletedAt.HasValue)
            ).ToList();

            var questionaries = filteredEntities.Select(ToCustomerDTO).ToList();

            foreach (var questionary in questionaries)
            {
                if (questionary.IsLocked && questionary.Name == PersonalDataQuestionaryConstants.Name)
                {
                    var branchStores = await _applicationDataContext.Stores
                        .Where(x => x.BranchId == (byte)_userContext.BranchId!.Value && x.isActive)
                        .ToListAsync();
                    var storeCodes = branchStores.Select(x => x.Number).Select(x => x.ToString()).ToList();
                    var cityIds = branchStores.Select(x => x.CityId).Select(x => x.ToString()).ToList();

                    var cityField = questionary.Fields.First(x => x.Name == PersonalDataQuestionaryConstants.CityField);

                    var cityKeysToRemove = cityField.Options.Keys.Where(x => !cityIds.Contains(x)).ToList();
                    foreach (var cityKeyToRemove in cityKeysToRemove)
                    {
                        cityField.Options.Remove(cityKeyToRemove);
                    }

                    var storeField = questionary.Fields.First(x => x.Name == PersonalDataQuestionaryConstants.HomeAirportField);

                    var storeKeysToRemove = storeField.Options.Keys.Where(x => !storeCodes.Contains(x)).ToList();
                    foreach (var storeKeyToRemove in storeKeysToRemove)
                    {
                        storeField.Options.Remove(storeKeyToRemove);
                    }
                }
            }

            return questionaries;
        }

        public async Task<CustomerQuestionaryDTO> GetCustomerQuestionaryByIdAsync(int id)
        {
            var questionary = (await GetCustomerEntitiesAsync())
                .FirstOrDefault(x => x.Id == id);

            if (questionary == null)
            {
                throw new ArgumentNullException("Опитувальник не знайден");
            }

            var dto = await ToCustomerDTOAsync(questionary);
            return dto;
        }

        private CustomerQuestionaryDTO ToCustomerDTO(Questionary entity)
        {
            var image = entity.Image;

            if (entity.IsLocked
                && entity.Name == PersonalDataQuestionaryConstants.Name
                && _userContext.BranchId.HasValue
                && (Branches)_userContext.BranchId.Value == Branches.CatJet
                && !string.IsNullOrEmpty(image))
            {
                image = image.Replace(
                    PersonalDataQuestionaryConstants.QuestionaryImageName,
                    PersonalDataQuestionaryConstants.QuestionaryCatJetImageName,
                    StringComparison.OrdinalIgnoreCase);
            }

            return new CustomerQuestionaryDTO
            {
                Id = entity.Id,
                ActiveDaysAfterComplete = entity.ActiveDaysAfterComplete,
                ExpirationDate = entity.ExpirationDate,
                Image = image,
                Alt = entity.Alt,
                Name = entity.Name,
                Fields = entity.Fields.OrderBy(x => x.Position).Select(x => new QuestionaryFieldDTO
                {
                    Name = x.Name,
                    IsRequired = x.IsRequired,
                    Type = x.Type,
                    Options = x.Name == PersonalDataQuestionaryConstants.HomeAirportField
                        ? x.Options.OrderBy(o => StoreAddressSortHelper.GetStreetTypeOrder(o.Value)).ThenBy(o => StoreAddressSortHelper.GetStreetNamePart(o.Value), StringComparer.CurrentCultureIgnoreCase).ToDictionary(o => o.Key, o => o.Value)
                        : x.Options.ToDictionary(o => o.Key, o => o.Value),
                    DefaultValue = null,
                    Validation = x.Validation
                }).ToList(),
                IsAnswered = entity.Answers.Any(),
                BranchId = entity.BranchId,
                Reward = entity.CouponId.HasValue
                ? new QuestionaryCouponRewardDTO
                {
                    CouponId = entity.CouponId.Value,
                }
                : new QuestionaryBonusRewardDTO
                {
                    Amount = entity.BonusReward!.Value,
                },
                IsLocked = entity.IsLocked,
            };
        }

        private async Task<CustomerQuestionaryDTO> ToCustomerDTOAsync(Questionary entity)
        {
            var dto = ToCustomerDTO(entity);

            // Auto-fill city and store for personal questionnaire
            if (entity.IsLocked && entity.Name == PersonalDataQuestionaryConstants.Name)
            {
                var customer = await _integrationDataContext.Customers
                    .Include(x => x.CustomerSettings)
                    .FirstOrDefaultAsync(x => x.Id == _userContext.CustomerId);

                if (customer != null)
                {
                    int? storeIdToUse = null;
                    int? cityIdToUse = null;

                    // Priority 1: Use ActiveStoreId from CustomerSettings if available
                    var activeStoreSetting = customer.CustomerSettings?
                        .FirstOrDefault(x => x.BranchId == (byte)_userContext.BranchId!.Value && x.ActiveStoreId.HasValue);

                    if (activeStoreSetting?.ActiveStoreId.HasValue == true)
                    {
                        storeIdToUse = activeStoreSetting.ActiveStoreId.Value;
                        var store = await _applicationDataContext.Stores
                            .Include(x => x.City)
                            .FirstOrDefaultAsync(x => x.Id == storeIdToUse.Value);
                        if (store != null)
                        {
                            cityIdToUse = store.CityId;
                        }
                    }
                    // Priority 2: Use StoreNearHomeId if available
                    else if (customer.StoreNearHomeId.HasValue)
                    {
                        storeIdToUse = customer.StoreNearHomeId.Value;
                        var store = await _applicationDataContext.Stores
                            .Include(x => x.City)
                            .FirstOrDefaultAsync(x => x.Id == storeIdToUse.Value);
                        if (store != null)
                        {
                            cityIdToUse = store.CityId;
                        }
                    }

                    // Set default values for city and store fields
                    var cityField = dto.Fields.FirstOrDefault(x => x.Name == PersonalDataQuestionaryConstants.CityField);
                    var storeField = dto.Fields.FirstOrDefault(x => x.Name == PersonalDataQuestionaryConstants.HomeAirportField);

                    if (cityField != null && cityIdToUse.HasValue)
                    {
                        var cityKey = cityIdToUse.Value.ToString();
                        if (cityField.Options.ContainsKey(cityKey))
                        {
                            cityField.DefaultValue = cityKey;
                        }
                    }

                    if (storeField != null && storeIdToUse.HasValue)
                    {
                        var storeKey = storeIdToUse.Value.ToString();
                        if (storeField.Options.ContainsKey(storeKey))
                        {
                            storeField.DefaultValue = storeKey;
                        }
                    }
                }
            }

            return dto;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _integrationDataContext.Questionaries
                .Include(x => x.Fields)
                .ThenInclude(x => x.Options)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                throw new ArgumentException("Опитувальник не знайден");
            }

            if (entity.Status != QuestionaryStatus.Inactive)
            {
                throw new ArgumentException("Тільки неактивний опитувальник можно видалити");
            }

            if (await _integrationDataContext.QuestionaryAnswers.AnyAsync(x => x.QuestionaryId == id))
            {
                throw new ArgumentException("Опитувальник з відповідями видаляти не можна");
            }

            if (entity.IsLocked)
            {
                throw new ArgumentException("Цей опитувальник неможливо видалити");
            }

            foreach (var field in entity.Fields)
            {
                if (field.Options?.Any() == true)
                {
                    _integrationDataContext.Remove(field.Options);
                }
                
                _integrationDataContext.Remove(field);
            }

            _integrationDataContext.Questionaries.Remove(entity);

            await _integrationDataContext.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            var entity = await _integrationDataContext.Questionaries.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                throw new ArgumentException("Опитувальник не знайден");
            }

            if (entity.Status != QuestionaryStatus.Activated)
            {
                throw new ArgumentException("Тільки активний опитувальник можно деактивувати");
            }

            if (entity.IsLocked)
            {
                throw new ArgumentException("Цей опитувальник неможливо деактивувати");
            }

            entity.Status = QuestionaryStatus.Inactive;

            await _integrationDataContext.SaveChangesAsync();

            await _jobSchedulerService.RemoveQuestionaryExpirationJobAsync(entity.Id);
        }

        public async Task<ExportFile> ExportAsync(int id, ExportFileFormat format, byte? branchId = null, ClientPlatform? clientPlatform = null, DateOnly? date = null)
        {
            var exportService = _serviceProvider.GetRequiredKeyedService<IExportService>(format);

            var entity = await _integrationDataContext.Questionaries
                .Include(x => x.Fields)
                    .ThenInclude(x => x.Options)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                throw new ArgumentException("Опитувальник не знайден");
            }

            var query = _integrationDataContext.QuestionaryAnswers.Where(x => x.QuestionaryId == id)
                .Include(x => x.Customer)
                .Include(x => x.StringAnswers)
                .Include(x => x.IntAnswers)
                .Include(x => x.FloatAnswers)
                .Include(x => x.DateTimeAnswers)
                .Include(x => x.SingleSelectAnswers)
                    .ThenInclude(x => x.Answer)
                .Include(x => x.MultiSelectAnswers)
                    .ThenInclude(x => x.Answer)
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId.Value);
            }

            if (clientPlatform.HasValue)
            {
                query = query.Where(x => x.ClientPlatform == clientPlatform.Value);
            }

            if (date.HasValue)
            {
                var dateTime = date.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(x => x.CreatedAt.Date == dateTime);
            }

            var fieldNames = new List<string>
            {
                "CustomerId",
                "CreatedAt",
                "CustomerName",
                "Branch",
                "ClientPlatfrom"
            };

            fieldNames.AddRange(entity.Fields.OrderBy(x => x.Position).Select(x => x.Name));

            return await exportService.ExportAsync(query, fieldNames, x => ToDictionaryAnswers(entity, x), $"export-answers-{id}");
        }

        private Dictionary<string, object> ToDictionaryAnswers(Questionary questionary, QuestionaryAnswer answer)
        {
            var dictionary = new Dictionary<string, object>();

            dictionary["CustomerId"] = answer.CustomerId;
            dictionary["CustomerName"] = answer.Customer.FirstName;
            dictionary["CreatedAt"] = answer.CreatedAt;
            dictionary["Branch"] = ((Branches)answer.BranchId).ToString();
            dictionary["ClientPlatfrom"] = answer.ClientPlatform.ToString();

            foreach (var field in questionary.Fields)
            {
                object? objectValue = field.Type switch
                {
                    QuestionaryItemType.String => answer.StringAnswers.FirstOrDefault(x => x.QuestionaryFieldId == field.Id)?.Answer,
                    QuestionaryItemType.Integer => answer.IntAnswers.FirstOrDefault(x => x.QuestionaryFieldId == field.Id)?.Answer,
                    QuestionaryItemType.Float => answer.FloatAnswers.FirstOrDefault(x => x.QuestionaryFieldId == field.Id)?.Answer,
                    QuestionaryItemType.DateTime => answer.DateTimeAnswers.FirstOrDefault(x => x.QuestionaryFieldId == field.Id)?.Answer,
                    QuestionaryItemType.Select => answer.SingleSelectAnswers.FirstOrDefault(x => x.QuestionaryFieldId == field.Id)?.Answer.Value,
                    QuestionaryItemType.Multiselect => GetMultiselectExportAnswer(answer.MultiSelectAnswers.FirstOrDefault(x => x.QuestionaryFieldId == field.Id)?.Answer)
                };

                if (objectValue != null)
                {
                    dictionary[field.Name] = objectValue;
                }
            }

            return dictionary;
        }

        private string? GetMultiselectExportAnswer(List<QuestionarySelectOption>? options)
        {
            if (options == null)
            {
                return null;
            }

            return string.Join(";", options.Select(x => x.Value));
        }

        public async Task<PagedListDTO<AdminQuestionaryDTO>> GetAllAsync(PagingDTO pagingDTO, byte? branchId, DateOnly? date = null, QuestionaryStatus? status = null)
        {
            var query = _integrationDataContext.Questionaries
                .Include(x => x.Fields)
                    .ThenInclude(x => x.Options)
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => !x.BranchId.HasValue || x.BranchId == branchId);
            }

            if (date.HasValue)
            {
                var dateTime = date.Value.ToDateTime(TimeOnly.MinValue);
                var dateTimeFrom = dateTime.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                var dateTimeTo = dateTime.AddDays(1).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                query = query.Where(x => x.CreatedAt >= dateTimeFrom && x.CreatedAt < dateTimeTo);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status);
            }

            return await query.GetPagedListAsync(pagingDTO, x => ToAdminDTO(x, _integrationDataContext.QuestionaryAnswers.Count(a => a.QuestionaryId == x.Id)));
        }

        public async Task<PagedListDTO<CustomerQuestionaryAnswerDTO>> GetAnswersAsync(PagingDTO pagingDTO, int questionaryId, byte? branchId = null, ClientPlatform? clientPlatform = null, DateOnly? date = null)
        {
            if (!await _integrationDataContext.Questionaries
                .AnyAsync(x => x.Id == questionaryId))
            {
                throw new ArgumentException("Опитувальник не знайден");
            }

            var query = _integrationDataContext.QuestionaryAnswers.Where(x => x.QuestionaryId == questionaryId)
                .Include(x => x.Customer)
                    .ThenInclude(x => x.CustomerSettings)
                .Include(x => x.StringAnswers)
                    .ThenInclude(x => x.QuestionaryField)
                .Include(x => x.IntAnswers)
                    .ThenInclude(x => x.QuestionaryField)
                .Include(x => x.FloatAnswers)
                    .ThenInclude(x => x.QuestionaryField)
                .Include(x => x.DateTimeAnswers)
                    .ThenInclude(x => x.QuestionaryField)
                .Include(x => x.SingleSelectAnswers)
                    .ThenInclude(x => x.Answer)
                .Include(x => x.SingleSelectAnswers)
                    .ThenInclude(x => x.QuestionaryField)
                .Include(x => x.MultiSelectAnswers)
                    .ThenInclude(x => x.Answer)
                .Include(x => x.MultiSelectAnswers)
                    .ThenInclude(x => x.QuestionaryField)
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId.Value);
            }

            if (clientPlatform.HasValue)
            {
                query = query.Where(x => x.ClientPlatform == clientPlatform.Value);
            }

            if (date.HasValue)
            {
                var dateTime = date.Value.ToDateTime(TimeOnly.MinValue);
                var dateTimeFrom = dateTime.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                var dateTimeTo = dateTime.AddDays(1).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                query = query.Where(x => x.CreatedAt >= dateTimeFrom && x.CreatedAt < dateTimeTo);
            }

            return await query.GetPagedListAsync(pagingDTO, ToAnswerDTO);
        }

        private CustomerQuestionaryAnswerDTO ToAnswerDTO(QuestionaryAnswer entity)
            => new CustomerQuestionaryAnswerDTO
            {
                Id = entity.Id,
                QuestionaryId = entity.QuestionaryId,
                CreatedAt = entity.CreatedAt,
                BranchId = entity.BranchId,
                ClientPlatform = entity.ClientPlatform,
                CustomerId = entity.CustomerId,
                CustomerName = entity.Customer!.FirstName,
                CustomerAvatar = _avatarService.GetDto(entity.Customer.CustomerSettings.FirstOrDefault(x => x.BranchId == entity.BranchId)?.Avatar, (Branches)entity.BranchId).Path,
                Answers = entity.MultiSelectAnswers
                    .Select(x => (x.QuestionaryField.Name, new QuestionaryMultiSelectAnswerDTO
                    {
                        Items = x.Answer.Select(x => new QuestionarySelectAnswerDTO
                        {
                            Key = x.Key,
                            Value = x.Value,
                        }).ToList()
                    } as QuestionaryFieldAnswerDTO))
                    .Concat(entity.SingleSelectAnswers
                        .Select(x => (x.QuestionaryField.Name, new QuestionarySelectAnswerDTO
                        {
                            Key = x.Answer.Key,
                            Value = x.Answer.Value,
                        } as QuestionaryFieldAnswerDTO)))
                    .Concat(entity.StringAnswers.Select(x => (x.QuestionaryField.Name, new QuestionaryStringAnswerDTO
                    {
                        Value = x.Answer,
                    } as QuestionaryFieldAnswerDTO)))
                    .Concat(entity.IntAnswers.Select(x => (x.QuestionaryField.Name, new QuestionaryIntAnswerDTO
                    {
                        Value = x.Answer,
                    } as QuestionaryFieldAnswerDTO)))
                    .Concat(entity.FloatAnswers.Select(x => (x.QuestionaryField.Name, new QuestionaryFloatAnswerDTO
                    {
                        Value = x.Answer,
                    } as QuestionaryFieldAnswerDTO)))
                    .Concat(entity.DateTimeAnswers.Select(x => (x.QuestionaryField.Name, new QuestionaryDateTimeAnswerDTO
                    {
                        Value = x.Answer,
                    } as QuestionaryFieldAnswerDTO)))
                .ToDictionary(x => x.Name, x => x.Item2)
            };

        public async Task<AdminQuestionaryDTO> GetAsync(int id)
        {
            var entity = await _integrationDataContext.Questionaries
                .Include(x => x.Fields)
                    .ThenInclude(x => x.Options)
                .Select(x => new { Questionary = x, AnswerCount = x.Answers.Count })
                .FirstOrDefaultAsync(x => x.Questionary.Id == id);
            if (entity?.Questionary == null)
            {
                throw new ArgumentException("Опитувальник не знайден");
            }

            return ToAdminDTO(entity.Questionary, entity.AnswerCount);
        }

        private AdminQuestionaryDTO ToAdminDTO(Questionary entity, int answerCount)
            => new AdminQuestionaryDTO
            {
                Id = entity.Id,
                ActiveDaysAfterComplete = entity.ActiveDaysAfterComplete,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                ExpirationDate = entity.ExpirationDate,
                Image = entity.Image,
                Alt = entity.Alt,
                Name = entity.Name,
                Status = entity.Status,
                Fields = entity.Fields.OrderBy(x => x.Position).Select(x => new QuestionaryFieldDTO
                {
                    Name = x.Name,
                    IsRequired = x.IsRequired,
                    Type = x.Type,
                    Options = x.Name == PersonalDataQuestionaryConstants.HomeAirportField
                        ? x.Options.OrderBy(o => StoreAddressSortHelper.GetStreetTypeOrder(o.Value)).ThenBy(o => StoreAddressSortHelper.GetStreetNamePart(o.Value), StringComparer.CurrentCultureIgnoreCase).ToDictionary(o => o.Key, o => o.Value)
                        : x.Options.ToDictionary(o => o.Key, o => o.Value)
                }).ToList(),
                BranchId = entity.BranchId,
                AnswerCount = answerCount,
                Reward = entity.CouponId.HasValue 
                ? new QuestionaryCouponRewardDTO
                {
                    CouponId = entity.CouponId.Value,
                }
                : new QuestionaryBonusRewardDTO
                {
                    Amount = entity.BonusReward!.Value,
                },
                IsLocked = entity.IsLocked,
            };

        public async Task<QuestionaryAnswerResponse> AnswerAsync(QuestionaryAnswerDTO model, ClientPlatform platform, byte branchId)
        {
            var questionary = (await GetCustomerEntitiesAsync()).FirstOrDefault(x => x.Id == model.QuestionaryId);

            if (questionary == null)
            {
                throw new ArgumentException("Опитувальник не знайден");
            }

            // Special check for personal questionnaire: prevent completing it multiple times across different branches
            if (questionary.IsLocked && questionary.Name == PersonalDataQuestionaryConstants.Name)
            {
                var customer = await _integrationDataContext.Customers
                    .FirstAsync(x => x.Id == _userContext.CustomerId);
                
                if (customer.PersonalQuestionaryCompletedAt.HasValue)
                {
                    throw new ArgumentException("Особистий опитувальник вже пройдений. Ви можете заповнити його лише один раз.");
                }
            }

            if (questionary.Answers.Any())
            {
                throw new ArgumentException("Опитувальник вже пройден");
            }

            var answerEntity = new QuestionaryAnswer
            {
                QuestionaryId = model.QuestionaryId,
                IntAnswers = new List<QuestionaryAnswerField<int>>(),
                MultiSelectAnswers = new List<QuestionaryAnswerField<List<QuestionarySelectOption>>>(),
                SingleSelectAnswers = new List<QuestionaryAnswerField<QuestionarySelectOption>>(),
                StringAnswers = new List<QuestionaryAnswerField<string>>(),
                DateTimeAnswers = new List<QuestionaryAnswerField<DateTime>>(),
                FloatAnswers = new List<QuestionaryAnswerField<float>>(),
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                CustomerId = _userContext.CustomerId!.Value,
                BranchId = branchId,
                ClientPlatform = platform,
            };

            foreach (var field in questionary.Fields)
            {
                if (model.Answers.TryGetValue(field.Name, out var answer))
                {
                    CollectFieldAnswer(answerEntity, field, answer);
                }
                else
                {
                    if (field.IsRequired)
                    {
                        throw new ArgumentException($"{field.Name} обов'язкове для заповнення");
                    }
                }
            }

            await _integrationDataContext.QuestionaryAnswers.AddRangeAsync(answerEntity);

            bool emailVerificationSent = false;
            string? email = null;

            if (questionary.IsLocked && questionary.Name == PersonalDataQuestionaryConstants.Name)
            {
                (emailVerificationSent, email) = await ApplyPersonalQuestionaryAnswersAsync(model, branchId);
            }

            await ApplyQuestionaryAnswerRewardAsync(questionary);

            await _integrationDataContext.SaveChangesAsync();

            return new QuestionaryAnswerResponse
            {
                RequiresEmailVerification = emailVerificationSent,
                Email = email,
                EmailVerificationMessage = emailVerificationSent 
                    ? "Щоб отримати бонус, перейдіть за посиланням у листі, який ми надіслали на вашу пошту" 
                    : null
            };
        }

        private static void CollectFieldAnswer(QuestionaryAnswer answerEntity, QuestionaryField field, QuestionaryFieldAnswerDTO answer)
        {
            switch (field.Type)
            {
                case QuestionaryItemType.String:
                    {
                        CollectStringAnswer(answerEntity, field, answer);
                        break;
                    }
                case QuestionaryItemType.Integer:
                    {
                        CollectIntegerAnswer(answerEntity, field, answer);
                        break;
                    }
                case QuestionaryItemType.Float:
                    {
                        CollectFloatAnswer(answerEntity, field, answer);
                        break;
                    }
                case QuestionaryItemType.DateTime:
                    {
                        CollectDateTimeAnswer(answerEntity, field, answer);
                        break;
                    }
                case QuestionaryItemType.Select:
                    {
                        CollectSelectAnswer(answerEntity, field, answer);
                        break;
                    }
                case QuestionaryItemType.Multiselect:
                    {
                        CollectMultiselectAnswer(answerEntity, field, answer);
                        break;
                    }
            }
        }

        private static void CollectStringAnswer(QuestionaryAnswer answerEntity, QuestionaryField field, QuestionaryFieldAnswerDTO answer)
        {
            if (answer is QuestionaryStringAnswerDTO a)
            {
                // Apply validation if specified
                if (!string.IsNullOrEmpty(field.Validation))
                {
                    var regex = new Regex(field.Validation, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
                    if (!regex.IsMatch(a.Value))
                    {
                        throw new ArgumentException($"{field.Name} не валідний");
                    }
                }

                answerEntity.StringAnswers.Add(new QuestionaryAnswerField<string>
                {
                    Answer = a.Value,
                    QuestionaryFieldId = field.Id,
                });
            }
            else
            {
                throw new ArgumentException($"{field.Name} має бути {nameof(QuestionaryStringAnswerDTO)}");
            }
        }

        private static void CollectIntegerAnswer(QuestionaryAnswer answerEntity, QuestionaryField field, QuestionaryFieldAnswerDTO answer)
        {
            if (answer is QuestionaryIntAnswerDTO a)
            {
                answerEntity.IntAnswers.Add(new QuestionaryAnswerField<int>
                {
                    Answer = a.Value,
                    QuestionaryFieldId = field.Id,
                });
            }
            else
            {
                throw new ArgumentException($"{field.Name} має бути {nameof(QuestionaryIntAnswerDTO)}");
            }
        }

        private static void CollectFloatAnswer(QuestionaryAnswer answerEntity, QuestionaryField field, QuestionaryFieldAnswerDTO answer)
        {
            if (answer is QuestionaryFloatAnswerDTO a)
            {
                answerEntity.FloatAnswers.Add(new QuestionaryAnswerField<float>
                {
                    Answer = a.Value,
                    QuestionaryFieldId = field.Id,
                });
            }
            else
            {
                throw new ArgumentException($"{field.Name} має бути {nameof(QuestionaryFloatAnswerDTO)}");
            }
        }

        private static void CollectDateTimeAnswer(QuestionaryAnswer answerEntity, QuestionaryField field, QuestionaryFieldAnswerDTO answer)
        {
            if (answer is QuestionaryDateTimeAnswerDTO a)
            {
                // Apply validation if specified (e.g., "18+" for minimum age)
                if (!string.IsNullOrEmpty(field.Validation) && field.Validation.EndsWith("+"))
                {
                    if (int.TryParse(field.Validation.Replace("+", ""), out var minAge))
                    {
                        var birthDate = a.Value;
                        var today = DateTime.UtcNow;
                        var age = today.Year - birthDate.Year;
                        if (birthDate.Date > today.AddYears(-age)) age--;

                        if (age < minAge)
                        {
                            throw new ArgumentException($"Вам має бути {minAge} років");
                        }
                    }
                }

                answerEntity.DateTimeAnswers.Add(new QuestionaryAnswerField<DateTime>
                {
                    Answer = a.Value.SetKindUtc(),
                    QuestionaryFieldId = field.Id,
                });
            }
            else
            {
                throw new ArgumentException($"{field.Name} має бути {nameof(QuestionaryDateTimeAnswerDTO)}");
            }
        }

        private static void CollectSelectAnswer(QuestionaryAnswer answerEntity, QuestionaryField field, QuestionaryFieldAnswerDTO answer)
        {
            if (answer is QuestionarySelectAnswerDTO a)
            {
                var selection = field.Options.FirstOrDefault(x => x.Key == a.Key);
                if (selection == null)
                {
                    throw new ArgumentException($"{field.Name} обрана опція невалідна");
                }

                answerEntity.SingleSelectAnswers.Add(new QuestionaryAnswerField<QuestionarySelectOption>
                {
                    Answer = selection,
                    QuestionaryFieldId = field.Id,
                });
            }
            else
            {
                throw new ArgumentException($"{field.Name} має бути {nameof(QuestionarySelectAnswerDTO)}");
            }
        }

        private static void CollectMultiselectAnswer(QuestionaryAnswer answerEntity, QuestionaryField field, QuestionaryFieldAnswerDTO answer)
        {
            if (answer is QuestionaryMultiSelectAnswerDTO a)
            {
                if (field.IsRequired && !a.Items.Any())
                {
                    throw new ArgumentException($"Оберіть хоч 1 опцію для {field.Name}");
                }

                var multiselectAnswers = new QuestionaryAnswerField<List<QuestionarySelectOption>>
                {
                    QuestionaryFieldId = field.Id,
                    Answer = new List<QuestionarySelectOption>(),
                };

                foreach (var item in a.Items)
                {
                    var selection = field.Options.FirstOrDefault(x => x.Key == item.Key);
                    if (selection == null)
                    {
                        throw new ArgumentException($"{field.Name} обрана опція невалідна");
                    }

                    multiselectAnswers.Answer.Add(selection);
                }

                answerEntity.MultiSelectAnswers.Add(multiselectAnswers);
            }
            else
            {
                throw new ArgumentException($"{field.Name} має бути {nameof(QuestionaryMultiSelectAnswerDTO)}");
            }
        }

        private async Task ApplyQuestionaryAnswerRewardAsync(Questionary questionary)
        {
            // Skip bonus rewards for personal questionnaire - they should only be awarded after email verification
            if (questionary.IsLocked && questionary.Name == PersonalDataQuestionaryConstants.Name)
            {
                return;
            }

            if (questionary.CouponId.HasValue)
            {
                await _integrationDataContext.CustomerCoupons.AddAsync(new CustomerCoupon
                {
                    CouponId = questionary.CouponId.Value,
                    CustomerId = _userContext.CustomerId!.Value,
                    Activated = true,
                });
            }
            else
            {
                var card = await _integrationDataContext.CustomerCards
                    .FirstAsync(x => x.CustomerId == _userContext.CustomerId
                        && x.BranchId == (int)_userContext.BranchId!.Value && x.Type == CardType.Virtual);

                var bonusTransaction = new CustomerBonusTransaction
                {
                    BranchId = (int)_userContext.BranchId!.Value,
                    Amount = questionary.BonusReward!.Value,
                    AmountRemaining = questionary.BonusReward!.Value,
                    Description = $"Нагорода за опитувальник {questionary.Name}",
                    CardCode = card.Code,
                };

                await _integrationDataContext.CustomerBonusTransactions.AddAsync(bonusTransaction);
            }
        }

        private async Task<(bool EmailVerificationSent, string? Email)> ApplyPersonalQuestionaryAnswersAsync(QuestionaryAnswerDTO model, byte branchId)
        {
            var customer = await _integrationDataContext.Customers
                    .Include(x => x.CustomerSettings)
                    .FirstAsync(x => x.Id == _userContext.CustomerId);

            customer.Birthday = ((QuestionaryDateTimeAnswerDTO)model.Answers[PersonalDataQuestionaryConstants.DateOfBirthField]).Value;
            customer.Sex = ((QuestionarySelectAnswerDTO)model.Answers[PersonalDataQuestionaryConstants.SexField]).Key == PersonalDataQuestionaryConstants.SexFieldManOption ? Sex.Male : Sex.Female;
            
            // WhereFindOut is now a Select field - Key = Value, so we can use either
            if (model.Answers.TryGetValue(PersonalDataQuestionaryConstants.WhereFindOutField, out var whereFindOutAnswerRaw) 
                && whereFindOutAnswerRaw is QuestionarySelectAnswerDTO whereFindOutAnswer)
            {
                // Використовуємо Value, якщо воно не порожнє, інакше Key
                // Перевіряємо обидва, щоб гарантувати, що значення буде встановлено
                var whereFindOutValue = !string.IsNullOrWhiteSpace(whereFindOutAnswer.Value) 
                    ? whereFindOutAnswer.Value 
                    : (!string.IsNullOrWhiteSpace(whereFindOutAnswer.Key) 
                        ? whereFindOutAnswer.Key 
                        : null);
                
                if (string.IsNullOrWhiteSpace(whereFindOutValue))
                {
                    throw new ArgumentException($"Відповідь для поля '{PersonalDataQuestionaryConstants.WhereFindOutField}' має порожнє значення");
                }
                
                customer.WhereFindOut = whereFindOutValue;
            }
            else
            {
                throw new ArgumentException($"Відповідь для поля '{PersonalDataQuestionaryConstants.WhereFindOutField}' не знайдена або невалідна");
            }

            // TypeOfActivity - Key = Value in questionnaire. Map Ukrainian labels to enum for analytics.
            if (model.Answers.TryGetValue(PersonalDataQuestionaryConstants.TypeOfActivityField, out var typeOfActivityAnswerRaw) 
                && typeOfActivityAnswerRaw is QuestionarySelectAnswerDTO typeOfActivityAnswer)
            {
                // Використовуємо Value, якщо воно не порожнє, інакше Key
                var typeOfActivityValue = !string.IsNullOrWhiteSpace(typeOfActivityAnswer.Value) 
                    ? typeOfActivityAnswer.Value 
                    : (!string.IsNullOrWhiteSpace(typeOfActivityAnswer.Key) 
                        ? typeOfActivityAnswer.Key 
                        : null);
                
                if (!string.IsNullOrWhiteSpace(typeOfActivityValue))
                {
                    // Мапимо всі 5 варіантів з анкети на enum
                    var mappedValue = typeOfActivityValue switch
                    {
                        "Студент"   => CustomerTypeOfActivity.Student,
                        "Працюю"    => CustomerTypeOfActivity.Working,
                        "Не працюю" => CustomerTypeOfActivity.Unemployed,
                        "Пенсіонер" => CustomerTypeOfActivity.Pensioner,
                        "Інше"      => CustomerTypeOfActivity.Other,
                        _           => (CustomerTypeOfActivity?)null
                    };
                    
                    customer.TypeOfActivity = mappedValue;
                    
                    // Explicitly mark as modified to ensure EF tracks the change
                    _integrationDataContext.Entry(customer).Property(x => x.TypeOfActivity).IsModified = true;
                }
            }
            else
            {
                throw new ArgumentException($"Відповідь для поля '{PersonalDataQuestionaryConstants.TypeOfActivityField}' не знайдена або невалідна");
            }

            // NumberOfChildren - now a Select field
            if (model.Answers.TryGetValue(PersonalDataQuestionaryConstants.NumberOfChildrenField, out var numberOfChildrenAnswer))
            {
                if (numberOfChildrenAnswer is QuestionarySelectAnswerDTO childrenAnswer)
                {
                    // Parse the key as integer, handle "10+" as 10
                    var key = childrenAnswer.Key;
                    if (key == "10+")
                    {
                        customer.NumberOfChildren = 10;
                    }
                    else if (int.TryParse(key, out var numberOfChildren))
                    {
                        customer.NumberOfChildren = numberOfChildren;
                    }
                }
            }

            var email = ((QuestionaryStringAnswerDTO)model.Answers[PersonalDataQuestionaryConstants.EmailField]).Value;
            // Email validation is handled via Validation field in CollectStringAnswer

            var emailChanged = customer.Email != email;
            customer.Email = email;
            customer.EmailVerified = false; // Reset verification if email changed

            var cityId = int.Parse(((QuestionarySelectAnswerDTO)model.Answers[PersonalDataQuestionaryConstants.CityField]).Key);
            var storeId = int.Parse(((QuestionarySelectAnswerDTO)model.Answers[PersonalDataQuestionaryConstants.HomeAirportField]).Key);

            var store = await _applicationDataContext.Stores
                .Include(x => x.City)
                .FirstOrDefaultAsync(x => x.Id == storeId && x.CityId == cityId);

            if (store == null)
            {
                throw new ArgumentException("Обраний магазин не відповідає місту");
            }

            customer.City = store.City.Name;
            customer.StoreNearHomeId = storeId;
            customer.PersonalQuestionaryCompletedAt = DateTime.UtcNow;

            // Explicitly mark TypeOfActivity and PersonalQuestionaryCompletedAt as modified
            // This ensures Entity Framework will save these changes even if they are nullable enum/DateTime
            var customerEntry = _integrationDataContext.Entry(customer);
            customerEntry.Property(x => x.TypeOfActivity).IsModified = true;
            customerEntry.Property(x => x.PersonalQuestionaryCompletedAt).IsModified = true;

            // Обробка опціональних полів, якщо вони є в анкеті
            if (model.Answers.TryGetValue("Країна", out var countryAnswer) && countryAnswer is QuestionaryStringAnswerDTO countryString)
            {
                if (!string.IsNullOrWhiteSpace(countryString.Value))
                {
                    customer.Country = countryString.Value;
                }
            }
            else if (model.Answers.TryGetValue("Country", out var countryAnswerEn) && countryAnswerEn is QuestionaryStringAnswerDTO countryStringEn)
            {
                if (!string.IsNullOrWhiteSpace(countryStringEn.Value))
                {
                    customer.Country = countryStringEn.Value;
                }
            }

            if (model.Answers.TryGetValue("Прізвище", out var lastNameAnswer) && lastNameAnswer is QuestionaryStringAnswerDTO lastNameString)
            {
                if (!string.IsNullOrWhiteSpace(lastNameString.Value))
                {
                    customer.LastName = lastNameString.Value;
                }
            }
            else if (model.Answers.TryGetValue("LastName", out var lastNameAnswerEn) && lastNameAnswerEn is QuestionaryStringAnswerDTO lastNameStringEn)
            {
                if (!string.IsNullOrWhiteSpace(lastNameStringEn.Value))
                {
                    customer.LastName = lastNameStringEn.Value;
                }
            }

            if (model.Answers.TryGetValue("Ім'я", out var firstNameAnswer) && firstNameAnswer is QuestionaryStringAnswerDTO firstNameString)
            {
                if (!string.IsNullOrWhiteSpace(firstNameString.Value))
                {
                    customer.FirstName = firstNameString.Value;
                }
            }
            else if (model.Answers.TryGetValue("FirstName", out var firstNameAnswerEn) && firstNameAnswerEn is QuestionaryStringAnswerDTO firstNameStringEn)
            {
                if (!string.IsNullOrWhiteSpace(firstNameStringEn.Value))
                {
                    customer.FirstName = firstNameStringEn.Value;
                }
            }

            // Generate and send email verification token
            bool emailVerificationSent = false;
            if (emailChanged || !customer.EmailVerified)
            {
                await SendEmailVerificationAsync(customer, email, branchId);
                emailVerificationSent = true;
            }

            return (emailVerificationSent, email);
        }

        private async Task SendEmailVerificationAsync(Customer customer, string email, byte branchId)
        {
            // Deactivate existing tokens
            var existingTokens = await _integrationDataContext.EmailVerificationTokens
                .Where(x => x.CustomerId == customer.Id && x.IsActive)
                .ToListAsync();

            foreach (var token in existingTokens)
            {
                token.IsActive = false;
            }

            // Generate new token
            var verificationToken = Guid.NewGuid().ToString();
            var tokenEntity = new EmailVerificationToken
            {
                CustomerId = customer.Id,
                Token = verificationToken,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
            };

            await _integrationDataContext.EmailVerificationTokens.AddAsync(tokenEntity);
            await _integrationDataContext.SaveChangesAsync();

            // Generate verification URL
            var baseUrl = Environment.GetEnvironmentVariable("API_URL") ?? "https://api.jetflight.com";
            // Remove trailing slash if present to avoid double slashes
            baseUrl = baseUrl.TrimEnd('/');
            var verificationUrl = $"{baseUrl}/v1/client/customer/verify-email?token={verificationToken}&branchId={branchId}";

            // Generate email body (branchId з запиту — сайт, з якого пройшов опитувальник)
            var emailBody = await _htmlGenerationService.GenerateEmailVerificationEmail(
                customer.FirstName ?? "Користувач",
                verificationUrl,
                branchId);
            if (string.IsNullOrWhiteSpace(emailBody))
            {
                emailBody = $@"<!DOCTYPE html><html><head><meta charset=""utf-8""/></head><body style=""font-family:sans-serif;padding:1rem""><p>Щоб отримати бонус, перейдіть за посиланням для підтвердження вашого email.</p><p><a href=""{verificationUrl}"" style=""color:#33a853;font-weight:bold"">Перейти за посиланням</a></p><p style=""color:#666;font-size:14px"">Посилання діє протягом 24 годин.</p></body></html>";
            }

            // Відправляємо від гілки, з якої пройшли опитувальник (branchId з заголовка запиту)
            var emailFrom = branchId switch
            {
                2 => EmailFrom.CatJet,
                1 => EmailFrom.BirdJet,
                _ => EmailFrom.BirdJet
            };
            _logger.LogInformation(
                "[QuestionaryService] Sending email verification - To: {To}, From: {From}, CustomerId: {CustomerId}",
                email, emailFrom, customer.Id);
            await _notificationService.SendEmailAsync(new EmailMessage
            {
                From = emailFrom,
                Subject = "Підтвердіть ваш email",
                Body = emailBody,
                To = new List<string> { email }
            });
        }
    }
}
