using JetFlight.ApplicationDataAccess;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.IntegrationDataAccess;
using JetFlight.Service.Extensions;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.Feedback;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.UserContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace JetFlight.Service.Services
{
    public interface IFeedbackService
    {
        Task CreateForBranchAsync(CreateBranchFeedbackDTO model, ClientPlatform platform, byte branchId);
        Task CreateForStoreAsync(CreateStoreFeedbackDTO model, ClientPlatform platform, byte branchId);
        Task<FeedbackDTO> GetAsync(int id);
        Task<PagedListDTO<FeedbackDTO>> GetAllAsync(PagingDTO paging, byte? branchId, FeedbackType? type = null, FeedbackStatus? status = null, ClientPlatform? platform = null, DateTime? createdTime = null);
        Task ProcessAsync(ProcessFeedbackRequest model);
        Task ChangeStatusAndAssigneeAsync(int feedbackId, FeedbackStatus? status = null, int? assigneeId = null);
    }

    public class FeedbackService : IFeedbackService
    {
        private readonly IUserContext _userContext;
        private readonly IDataUnitOfWork _unitOfWork;
        private readonly IntegrationDataContext _integrationDataContext;
        private readonly SmsSettings _smsSettings;
        private readonly IAvatarService _avatarService;
        private readonly INotificationService _notificationService;
        private readonly ICouponService _couponService;
        private readonly IFirebaseService _firebaseService;
        private readonly ApplicationDataContext _applicationUnitOfWork;
        private readonly IMediaService _mediaService;
        private readonly ILogger _logger;

        public FeedbackService(
            IUserContext userContext,
            IDataUnitOfWork unitOfWork,
            IntegrationDataContext integrationDataContext,
            ApplicationDataContext applicationUnitOfWork,
            IAvatarService avatarService,
            IFirebaseService firebaseService,
            INotificationService notificationService,
            ICouponService couponService,
            IOptions<SmsSettings> smsSettings,
            IMediaService mediaService,
            ILogger logger)
        {
            _userContext = userContext;
            _unitOfWork = unitOfWork;
            _integrationDataContext = integrationDataContext;
            _avatarService = avatarService;
            _notificationService = notificationService;
            _smsSettings = smsSettings.Value;
            _couponService = couponService;
            _firebaseService = firebaseService;
            _applicationUnitOfWork = applicationUnitOfWork;
            _mediaService = mediaService;
            _logger = logger;
        }

        public async Task CreateForStoreAsync(CreateStoreFeedbackDTO model, ClientPlatform platform, byte branchId)
        {
            var store = await _unitOfWork.Stores.Find(x => x.Id == model.storeId && x.BranchId == branchId && x.isActive).FirstOrDefaultAsync();
            if (store == null)
            {
                throw new ArgumentException("Магазин не існує в мережі");
            }

            var feedback = new Feedback
            {
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                CustomerId = _userContext.CustomerId!.Value,
                Message = model.message,
                Rating = model.rating,
                StoreId = model.storeId,
                BranchId = branchId,
                Status = FeedbackStatus.Pending,
                Platform = platform,
            };

            if (model.Files != null)
            {
                var attachments = await UploadAsync(model.Files);
                feedback.Attachments = attachments;
            }

            await _unitOfWork.Feedbacks.Add(feedback);
            await _unitOfWork.Save();
        }

        public async Task CreateForBranchAsync(CreateBranchFeedbackDTO model, ClientPlatform platform, byte branchId)
        {
            var feedback = new Feedback
            {
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                CustomerId = _userContext.CustomerId!.Value,
                Message = model.message,
                Rating = model.rating,
                BranchId = branchId,
                Status = FeedbackStatus.Pending,
                Platform = platform,
            };

            await _unitOfWork.Feedbacks.Add(feedback);
            await _unitOfWork.Save();
        }

        private async Task<List<FeedbackAttachment>> UploadAsync(ICollection<FeedbackFileRequestDTO> files)
        {
            var result = new List<FeedbackAttachment>();

            foreach (var dto in files)
            {
                if (dto == null)
                {
                    throw new ArgumentNullException(nameof(dto), "Файл не може бути null");
                }

                if (string.IsNullOrEmpty(dto.file.FileName))
                {
                    throw new ArgumentException("Назва файла обовязкова", nameof(dto.file.FileName));
                }

                if (string.IsNullOrEmpty(dto.file.ContentType))
                {
                    throw new ArgumentException("Контент не може бути пустим", nameof(dto.file.ContentType));
                }

                var filePath = await _mediaService.UploadAsync(dto.file);
                var uniqueName = Path.GetFileName(filePath.ToString());

                var media = new FeedbackAttachment
                {
                    MimeType = dto.file.ContentType,
                    CreatedAt = DateTime.UtcNow.SetKindUtc(),
                    FileName = dto.file.FileName,
                    FilePath = uniqueName,
                };

                result.Add(media);
            }

            return result;
        }

        public async Task<FeedbackDTO> GetAsync(int id)
        {
            var feedback = await _unitOfWork.Feedbacks.GetAll()
                .Include(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (feedback == null)
            {
                throw new ArgumentException("Заявку не знайдено");
            }

            return await EnrichFeedbackAsync(ToDTO(feedback));
        }

        public async Task<PagedListDTO<FeedbackDTO>> GetAllAsync(PagingDTO paging, byte? branchId, FeedbackType? type = null, FeedbackStatus? status = null, ClientPlatform? platform = null, DateTime? createdTime = null)
        {
            var query = _unitOfWork.Feedbacks
                .GetAll()
                .Include(x => x.Attachments)
                .AsQueryable();
            
            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status);
            }

            if (platform.HasValue)
            {
                query = query.Where(x => x.Platform == platform);
            }

            if (createdTime.HasValue)
            {
                query = query.Where(x => x.CreatedAt.Date == createdTime.Value.Date);
            };

            if (type.HasValue)
            {
                query = query.Where(x => x.StoreId.HasValue == (type == FeedbackType.Store));                    
            }

            var result = await query.GetPagedListAsync(paging, ToDTO);
            foreach (var feedback in result.Items)
            {
                await EnrichFeedbackAsync(feedback);
            }

            return result;
        }

        private async Task<FeedbackDTO> EnrichFeedbackAsync(FeedbackDTO dto)
        {
            var customer = await _integrationDataContext.Customers
                .Include(x => x.CustomerSettings.Where(x => x.BranchId == dto.BranchId))
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);

            if (customer != null)
            {
                dto.Customer = new FeedbackCustomerDTO
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    PhoneNumber = customer.PhoneNumber,
                    Avatar = _avatarService.GetDto(customer.CustomerSettings.First().Avatar, (Shared.Models.Store.Branches)dto.BranchId)
                };
            }

            foreach (var coupon in dto.AssignedCoupons)
            {
                var customerCoupon = await _integrationDataContext.CustomerCoupons
                    .Include(x => x.Coupon)
                    .FirstAsync(x => x.Id == coupon.CustomerCouponId);

                coupon.Status = customerCoupon.Coupon.Status;
                coupon.StartDate = customerCoupon.Coupon.StartDate;
                coupon.ExpirationDate = customerCoupon.Coupon.ExpirationDate;
                coupon.CouponId = customerCoupon.Coupon.Id;
                coupon.Description = customerCoupon.Coupon.Description;
                coupon.Name = customerCoupon.Coupon.Name;
                coupon.Image = customerCoupon.Coupon.Image;
                coupon.PrivateName = customerCoupon.Coupon.PrivateName;
                coupon.IsUsed = customerCoupon.UsedTimes == customerCoupon.Coupon.UseTimes;
            }

            return dto;
        }

        private FeedbackDTO ToDTO(Feedback feedback)
        {
            return new FeedbackDTO
            {
                Id = feedback.Id,
                Platform = feedback.Platform,
                AssigneeId = feedback.AssigneeId,
                BranchId = feedback.BranchId,
                CreatedAt = feedback.CreatedAt,
                CustomerId = feedback.CustomerId,
                Message = feedback.Message,
                Rating = feedback.Rating,
                ProcessingDate = feedback.ProcessingDate,
                Status = feedback.Status,
                ResolveMessage = feedback.ResolveMessage,
                ResolveSignature = feedback.ResolveSignature,
                StoreId = feedback.StoreId,
                UpdatedAt = feedback.UpdatedAt,
                Attachments = feedback.Attachments.Select(x => new FeedbackAttachmentDTO
                {
                    CreatedAt = x.CreatedAt,
                    Id = x.Id,
                    MimeType = x.MimeType,
                    FilePath = new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
                    {
                        Path = $"{StorageConstants.AppPath}/{x.FilePath}"
                    }.ToString(),
                    Name = x.FileName,
                }).ToList(),
                AssignedCoupons = feedback.CustomerCouponIds?.Select(x => new FeedbackCouponDTO
                {
                    CustomerCouponId = x,
                }).ToList() ?? new List<FeedbackCouponDTO>(),
            };
        }

        public async Task ProcessAsync(ProcessFeedbackRequest model)
        {
            var existingResult = await _unitOfWork.Feedbacks.GetAll().FirstOrDefaultAsync(x => x.Id == model.Id);

            if (existingResult == null)
            {
                throw new ArgumentException("Заявку не знайдено");
            }

            if (existingResult.Status == FeedbackStatus.Completed)
            {
                throw new ArgumentException("Заявку вже опрацьована");
            }

            var utcNow = DateTime.UtcNow.SetKindUtc();

            existingResult.ResolveMessage = model.ResolveMessage;
            existingResult.ResolveSignature = model.ResolveSignature;
            existingResult.Status = FeedbackStatus.Completed;
            existingResult.ProcessingDate = DateTime.UtcNow.SetKindUtc();

            var couponText = string.Empty;
            var couponSmsText = string.Empty;
            List<byte?> branchIds = new List<byte?>();
            if (model.CouponIdToAssign.HasValue)
            {
                var createdCustomerCouponId = await _couponService.AssignPersonalCouponToCustomerAsync(model.CouponIdToAssign.Value, existingResult.CustomerId);
                var assignedCoupon = await _couponService.GetCustomerCouponByAdminAsync(createdCustomerCouponId);
                if (assignedCoupon?.StoreIds != null && assignedCoupon.StoreIds.Any())
                {
                    branchIds = await _applicationUnitOfWork.Stores
                        .Where(x => assignedCoupon.StoreIds.Contains(x.Id))
                        .Select(x => x.BranchId)
                        .Where(x => x != null)
                        .Distinct()
                        .ToListAsync();
                }
                couponText = assignedCoupon != null ? $" Ваш ваучер {assignedCoupon.Name}, термін дії якого {assignedCoupon.StartDate.FromUtcToTimezone(TimeZoneConstants.UATimezone)}-{assignedCoupon.ExpirationDate.FromUtcToTimezone(TimeZoneConstants.UATimezone)}" : string.Empty;
                couponSmsText = assignedCoupon != null ? $" Ваучер {assignedCoupon.Name}" : string.Empty;

                existingResult.CustomerCouponIds ??= new List<int>();
                existingResult.CustomerCouponIds.Add(createdCustomerCouponId);
            }

            await _unitOfWork.Save();

            var customer = await _integrationDataContext.Customers
                .Include(x => x.CustomerSettings)
                .FirstAsync(x => x.Id == existingResult.CustomerId);


			var setting = customer.CustomerSettings.FirstOrDefault(x => x.BranchId == existingResult.BranchId);

            var notificationTitle = $"Ваш відгук #{existingResult.Id} оброблено";
            var notificationBody = string.IsNullOrWhiteSpace(couponText)
                ? existingResult.ResolveMessage
                : $"{existingResult.ResolveMessage}.{couponText.TrimStart()}";
            
            // For feedback processing notifications:
            // - If user has app (PushNotificationToken exists) and push enabled: send push notification only (no SMS)
            // - If user has app but push disabled: only notification in app history (no SMS)
            // - If user has no app (no PushNotificationToken) and SMS enabled: send SMS only
            var hasApp = setting != null && !string.IsNullOrEmpty(setting.PushNotificationToken);
            
            if (hasApp)
            {
                // User has app - send push/notification in app (no SMS)
                await _firebaseService.SendMessageAsync(notificationTitle, notificationBody, "feedback", (byte)existingResult.BranchId, customer.Id);
            }
            else if (setting?.EnableSmsNotifications == true && !string.IsNullOrEmpty(customer.PhoneNumber))
            {
                // User has no app - send SMS if enabled
                var smsBody = string.IsNullOrWhiteSpace(couponSmsText)
                    ? existingResult.ResolveMessage
                    : $"{existingResult.ResolveMessage}.{couponSmsText.TrimStart()}";
                var message = $"Ваш відгук #{existingResult.Id} оброблено. {smsBody}";
                
                string? senderId = null;
                switch ((byte)existingResult.BranchId)
                {
                    case 1:
                        senderId = _smsSettings.From.BirdJet.Id;
                        _logger.Information("[FeedbackService.ProcessAsync] BranchId: {BranchId}, Using BirdJet SenderId: {SenderId}, BirdJet Name: {BirdJetName}", 
                            existingResult.BranchId, senderId, _smsSettings.From.BirdJet.Name);
                        break;
                    case 2:
                        senderId = _smsSettings.From.CatJet.Id;
                        _logger.Information("[FeedbackService.ProcessAsync] BranchId: {BranchId}, Using CatJet SenderId: {SenderId}, CatJet Name: {CatJetName}", 
                            existingResult.BranchId, senderId, _smsSettings.From.CatJet.Name);
                        break;
                    default:
                        _logger.Error("[FeedbackService.ProcessAsync] Invalid BranchId: {BranchId}", existingResult.BranchId);
                        throw new ArgumentException("Invalid branch ID");
                }

                if (string.IsNullOrWhiteSpace(senderId))
                {
                    _logger.Error("[FeedbackService.ProcessAsync] SenderId is null or empty for BranchId: {BranchId}", existingResult.BranchId);
                    throw new InvalidOperationException($"SenderId is not configured for BranchId: {existingResult.BranchId}");
                }

                var smsMessage = new SmsMessage
                {
                    Message = message,
                    Recievers = new List<string> { customer.PhoneNumber },
                    Sender = senderId
                };

                _logger.Information("[FeedbackService.ProcessAsync] Sending SMS - FeedbackId: {FeedbackId}, CustomerId: {CustomerId}, BranchId: {BranchId}, SenderId: {SenderId}, PhoneNumber: {PhoneNumber}, MessageLength: {MessageLength}", 
                    existingResult.Id, customer.Id, existingResult.BranchId, senderId, customer.PhoneNumber, message.Length);

                await _notificationService.SendSmsAsync(smsMessage);
            }
            if (branchIds != null && branchIds.Any())
            {
                foreach (var branchId in branchIds)
                {
                    if (branchId == null) continue;
                    var title = "Ваучер доступний";
                    var body = $"{couponText} вже доступний для використання";
                    await _firebaseService.SendMessageAsync(title, body, "coupons", branchId.Value, customer.Id);
                }
            }
        }

        public async Task ChangeStatusAndAssigneeAsync(int feedbackId, FeedbackStatus? status = null, int? assigneeId = null)
        {
            var existingResult = await _unitOfWork.Feedbacks.GetAll().FirstOrDefaultAsync(x => x.Id == feedbackId);
            if (existingResult == null)
            {
                throw new ArgumentException("Заявку не знайдено");
            }

            existingResult.AssigneeId = FieldUpdater.UpdateIfNotNullOrEmpty(existingResult.AssigneeId, assigneeId);
            existingResult.UpdatedAt = DateTime.UtcNow.SetKindUtc();
            if (status.HasValue)
            {
                existingResult.Status = status.Value;
                if (status.Value == FeedbackStatus.Completed)
                {
                    existingResult.ProcessingDate = DateTime.UtcNow.SetKindUtc();
                }
            }

            await _unitOfWork.Save();
        }
    }
}
