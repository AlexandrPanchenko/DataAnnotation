using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Extensions;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Extensions;
using JetFlight.Shared.Models.Message;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models.Targets;
using JetFlight.Shared.UserContext;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.Service.Services
{
    public interface ITargetNotificationService
    {
        Task<TargetSmsMessageDTO> CreateTargetSmsMessageAsync(CreateTargetSmsMessageDTO model);
        Task<ScheduledCustomerSmsMessageDTO> GetScheduledCustomerSmsMessageAsync(int id);
        Task<PagedListDTO<ScheduledCustomerSmsMessageDTO>> GetScheduledCustomerSmsMessagesAsync(PagingDTO pagingDTO, byte? branchId = null, int? targetSmsMessageId = null, ScheduledCustomerMessageStatus? status = null, MessageTheme? theme = null, DateOnly? date = null);
        Task<TargetSmsMessageDTO> GetTargetSmsMessageAsync(int id);
        Task<PagedListDTO<TargetSmsMessageDTO>> GetTargetSmsMessagesAsync(PagingDTO pagingDTO, byte? branchId = null, TargetMessageStatus? status = null, MessageTheme? theme = null, DateOnly? date = null);


        Task<TargetEmailMessageDTO> GetTargetEmailMessageAsync(int id);
        Task<PagedListDTO<TargetEmailMessageDTO>> GetTargetEmailMessagesAsync(PagingDTO pagingDTO, byte? branchId = null, TargetMessageStatus? status = null, MessageTheme? theme = null, DateOnly? date = null);
        Task<ScheduledCustomerEmailMessageDTO> GetScheduledCustomerEmailMessageAsync(int id);
        Task<PagedListDTO<ScheduledCustomerEmailMessageDTO>> GetScheduledCustomerEmailMessagesAsync(PagingDTO pagingDTO, byte? branchId = null, int? targetEmailMessageId = null, ScheduledCustomerMessageStatus? status = null, MessageTheme? theme = null, DateOnly? date = null);
        Task<TargetEmailMessageDTO> CreateTargetEmailMessageAsync(CreateTargetEmailMessageDTO model);

        Task<string> GenerateTargetEmailBodyAsync(CreateEmailParametersDTO parameters);
        Task<(string, List<EmailAttachment>)> GenerateTargetEmailBodyWithEmailAttachmentsAsync(CreateEmailParametersDTO parameters);
        Task<string> GenerateTargetEmailBodyAsync(int targetEmailMessageId);
        Task<string> GenerateTargetEmailBodyAsync(TargetEmailMessage targetEmailMessage);

        Task SendTestEmailAsync(int targetEmailMessageId);
        Task SendTestEmailAsync(PreviewEmailParametersDTO parameters);
    }

    public class TargetNotificationService : ITargetNotificationService
    {
        private readonly IJobSchedulerService _jobSchedulerService;
        private readonly ITargetService _targetService;
        private readonly IntegrationDataContext _integrationDataContext;
        private readonly IMediaService _mediaService;
        private readonly IAvatarService _avatarService;
        private readonly IHtmlGenerationService _htmlGenerationService;
        private readonly INotificationService _notificationService;
        private readonly IAdminService _adminService;
        private readonly IUserContext _userContext;

        public TargetNotificationService(
            IJobSchedulerService jobSchedulerService,
            ITargetService targetService,
            IntegrationDataContext integrationDataContext,
            IMediaService mediaService,
            IAvatarService avatarService,
            IHtmlGenerationService htmlGenerationService,
            INotificationService notificationService,
            IAdminService adminService,
            IUserContext userContext
            )
        {
            _jobSchedulerService = jobSchedulerService;
            _targetService = targetService;
            _integrationDataContext = integrationDataContext;
            _mediaService = mediaService;
            _avatarService = avatarService;
            _htmlGenerationService = htmlGenerationService;
            _notificationService = notificationService;
            _adminService = adminService;
            _userContext = userContext;
        }

        public async Task<TargetSmsMessageDTO> CreateTargetSmsMessageAsync(CreateTargetSmsMessageDTO model)
        {
            var target = await _targetService.GetTargetEntityAsync(model.targetId);

            if (target == null)
            {
                throw new ArgumentException("Таргет не знайден");
            }

            var entity = new TargetSmsMessage
            {
                Status = TargetMessageStatus.Created,
                BranchId = model.branchId,
                TargetId = model.targetId,
                Theme = model.theme,
                Message = model.message,
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                ScheduledDate = model.scheduledDate,
            };

            await _integrationDataContext.TargetSmsMessages.AddAsync(entity);
            await _integrationDataContext.SaveChangesAsync();

            await _jobSchedulerService.SetSmsPopulationAsync(entity.Id);

            var dto = ToTargetSmsDTO(entity);
            dto.Target = await _targetService.GetAsync(dto.TargetId);
            return dto;
        }

        private static TargetSmsMessageDTO ToTargetSmsDTO(TargetSmsMessage entity)
            => new TargetSmsMessageDTO
            {
                Id = entity.Id,
                Message = entity.Message,
                BranchId = entity.BranchId,
                CreatedAt = entity.CreatedAt,
                ScheduledDate = entity.ScheduledDate,
                TargetId = entity.TargetId,
                Theme = entity.Theme,
                Status = entity.Status,
            };

        public async Task<TargetSmsMessageDTO> GetTargetSmsMessageAsync(int id)
        {
            var entity = await _integrationDataContext.TargetSmsMessages
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                throw new ArgumentException("Таргет повідомлення не знайдено");
            }

            var dto = ToTargetSmsDTO(entity);

            dto.PopulatedMessagesCount = await _integrationDataContext.CustomerSmsMessages.CountAsync(x => x.SmsMessageId == dto.Id);
            dto.Target = await _targetService.GetAsync(dto.TargetId);

            return dto;
        }

        public async Task<PagedListDTO<TargetSmsMessageDTO>> GetTargetSmsMessagesAsync(
            PagingDTO pagingDTO,
            byte? branchId = null,
            TargetMessageStatus? status = null,
            MessageTheme? theme = null,
            DateOnly? date = null
            )
        {
            var query = _integrationDataContext.TargetSmsMessages
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (date.HasValue)
            {
                var dateTime = date.Value.ToDateTime(TimeOnly.MinValue);
                var dateTimeFrom = dateTime.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                var dateTimeTo = dateTime.AddDays(1).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                query = query.Where(x => x.ScheduledDate >= dateTimeFrom && x.ScheduledDate < dateTimeTo);
            }

            if (theme.HasValue)
            {
                query = query.Where(x => x.Theme == theme);
            }

            var dtos = await query.GetPagedListAsync(pagingDTO, ToTargetSmsDTO);

            Dictionary<int, TargetDto> targets = new Dictionary<int, TargetDto>();

            foreach (var message in dtos.Items)
            {
                message.PopulatedMessagesCount = await _integrationDataContext.CustomerSmsMessages.CountAsync(x => x.SmsMessageId == message.Id);

                targets.TryGetValue(message.TargetId, out var target);
                if (target == null)
                {
                    target = await _targetService.GetAsync(message.TargetId);
                    targets.Add(message.TargetId, target);
                }

                message.Target = target;
            }

            return dtos;
        }

        public async Task<ScheduledCustomerSmsMessageDTO> GetScheduledCustomerSmsMessageAsync(int id)
        {
            var entity = await _integrationDataContext.CustomerSmsMessages
                .Include(x => x.SmsMessage)
                .Include(x => x.Customer)
                .ThenInclude(x => x.CustomerSettings)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                throw new ArgumentException("Повідомлення не знайдено");
            }

            return ToCustomerSmsDTO(entity);
        }

        public async Task<PagedListDTO<ScheduledCustomerSmsMessageDTO>> GetScheduledCustomerSmsMessagesAsync(
            PagingDTO pagingDTO,
            byte? branchId = null,
            int? targetSmsMessageId = null,
            ScheduledCustomerMessageStatus? status = null,
            MessageTheme? theme = null,
            DateOnly? date = null
            )
        {
            var query = _integrationDataContext.CustomerSmsMessages
                .Include(x => x.SmsMessage)
                .Include(x => x.Customer)
                .ThenInclude(x => x.CustomerSettings)
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.SmsMessage.BranchId == branchId.Value);
            }

            if (targetSmsMessageId.HasValue)
            {
                query = query.Where(x => x.SmsMessageId == targetSmsMessageId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (date.HasValue)
            {
                var dateTime = date.Value.ToDateTime(TimeOnly.MinValue);
                var dateTimeFrom = dateTime.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                var dateTimeTo = dateTime.AddDays(1).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                query = query.Where(x => x.SmsMessage.ScheduledDate >= dateTimeFrom && x.SmsMessage.ScheduledDate < dateTimeTo);
            }

            if (theme.HasValue)
            {
                query = query.Where(x => x.SmsMessage.Theme == theme);
            }

            return await query.GetPagedListAsync(pagingDTO, ToCustomerSmsDTO);
        }

        private ScheduledCustomerSmsMessageDTO ToCustomerSmsDTO(CustomerSmsMessage entity)
            => new ScheduledCustomerSmsMessageDTO
            {
                Id = entity.Id,
                SmsMessageId = entity.SmsMessageId,
                Message = entity.SmsMessage.Message,

                BranchId = entity.SmsMessage.BranchId,
                CustomerId = entity.CustomerId,
                CustomerName = entity.Customer.FirstName,
                CustomerPhoneNumber = entity.Customer.PhoneNumber,
                CustomerAvatar = _avatarService.GetDto(entity.Customer.CustomerSettings.First(x => x.BranchId == entity.SmsMessage.BranchId).Avatar, (Branches)entity.SmsMessage.BranchId).Path,
                Theme = entity.SmsMessage.Theme,
                Status = entity.Status,
            };

        public async Task<TargetEmailMessageDTO> CreateTargetEmailMessageAsync(CreateTargetEmailMessageDTO model)
        {
            var target = await _targetService.GetTargetEntityAsync(model.targetId);

            if (target == null)
            {
                throw new ArgumentException("Таргет не знайден");
            }

            var mainImageUrl = model.mainImage?.file != null
                ? (await _mediaService.UploadAsync(model.mainImage.file)).ToString()
                : null;

            var blocks = await Task.WhenAll(model.blocks.Select(async x => new EmailBlock
            {
                ImageUrl = x.image?.file != null
                ? (await _mediaService.UploadAsync(x.image.file)).ToString()
                : null,
                Link = ToEmailLink(x.link),
                Header = x.header,
                Text = x.text,
            }));

            var entity = new TargetEmailMessage
            {
                Status = TargetMessageStatus.Created,
                BranchId = model.branchId,
                TargetId = model.targetId,

                Theme = model.theme,

                MainImageUrl = mainImageUrl,
                MainHeader = model.mainHeader,
                SecondHeader = model.secondHeader,
                Text = model.text,
                Link = ToEmailLink(model.link),
                
                Blocks = blocks.ToList(),

                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                ScheduledDate = model.scheduledDate,
            };

            await _integrationDataContext.TargetEmailMessages.AddAsync(entity);
            await _integrationDataContext.SaveChangesAsync();

            await _jobSchedulerService.SetEmailPopulationAsync(entity.Id);

            var dto = ToTargetEmailMessageDTO(entity);
            dto.Target = await _targetService.GetAsync(dto.TargetId);

            return dto;
        }

        private static EmailLinkDTO? ValidateLinkDTO(EmailLinkDTO? emailLinkDTO)
        {
            if (emailLinkDTO == null)
            {
                return null;
            }
            
            if (emailLinkDTO.text == null && emailLinkDTO.url == null)
            {
                return null;
            }

            if (emailLinkDTO.text == null || emailLinkDTO.url == null)
            {
                throw new ArgumentException("Link is invalid");
            }

            return emailLinkDTO;
        }

        private static EmailLink? ToEmailLink(EmailLinkDTO? link)
        {
            link = ValidateLinkDTO(link);
            return link != null
                ? new EmailLink
                {
                    Url = link.url!,
                    Text = link.text!,
                }
                : null;
        }


        private static TargetEmailMessageDTO ToTargetEmailMessageDTO(TargetEmailMessage entity)
            => new TargetEmailMessageDTO
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                ScheduledDate = entity.ScheduledDate,
                BranchId = entity.BranchId,
                TargetId = entity.TargetId,
                Status = entity.Status,
                Theme = entity.Theme,
                MainImageUrl = entity.MainImageUrl,
                MainHeader = entity.MainHeader,
                SecondHeader = entity.SecondHeader,
                Text = entity.Text,
                Link = ToLinkDTO(entity.Link),
                Blocks = entity.Blocks.Select(x => new EmailBlockDTO
                {
                    imageUrl = x.ImageUrl,
                    header = x.Header,
                    text = x.Text,
                    link = ToLinkDTO(x.Link),
                }).ToList(),
            };

        private static EmailLinkDTO? ToLinkDTO(EmailLink? link)
            => link != null
            ? new EmailLinkDTO
            {
                text = link.Text,
                url = link.Url
            }
            : null;

        public async Task<TargetEmailMessageDTO> GetTargetEmailMessageAsync(int id)
        {
            var entity = await _integrationDataContext.TargetEmailMessages
                .Include(x => x.Link)
                .Include(x => x.Blocks)
                    .ThenInclude(x => x.Link)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                throw new ArgumentException("Таргет повідомлення не знайдено");
            }

            var dto = ToTargetEmailMessageDTO(entity);

            dto.PopulatedMessagesCount = await _integrationDataContext.CustomerEmailMessages.CountAsync(x => x.EmailMessageId == dto.Id);
            dto.Target = await _targetService.GetAsync(dto.TargetId);

            return dto;
        }

        public async Task<PagedListDTO<TargetEmailMessageDTO>> GetTargetEmailMessagesAsync(
            PagingDTO pagingDTO,
            byte? branchId = null,
            TargetMessageStatus? status = null,
            MessageTheme? theme = null,
            DateOnly? date = null
            )
        {
            var query = _integrationDataContext.TargetEmailMessages
                .Include(x => x.Link)
                .Include(x => x.Blocks)
                    .ThenInclude(x => x.Link)
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (date.HasValue)
            {
                var dateTime = date.Value.ToDateTime(TimeOnly.MinValue);
                var dateTimeFrom = dateTime.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                var dateTimeTo = dateTime.AddDays(1).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                query = query.Where(x => x.ScheduledDate >= dateTimeFrom && x.ScheduledDate < dateTimeTo);
            }

            if (theme.HasValue)
            {
                query = query.Where(x => x.Theme == theme);
            }

            var dtos = await query.GetPagedListAsync(pagingDTO, ToTargetEmailMessageDTO);

            Dictionary<int, TargetDto> targets = new Dictionary<int, TargetDto>();

            foreach (var message in dtos.Items)
            {
                message.PopulatedMessagesCount = await _integrationDataContext.CustomerEmailMessages.CountAsync(x => x.EmailMessageId == message.Id);

                targets.TryGetValue(message.TargetId, out var target);
                if (target == null)
                {
                    target = await _targetService.GetAsync(message.TargetId);
                    targets.Add(message.TargetId, target);
                }

                message.Target = target;
            }

            return dtos;
        }

        public async Task<ScheduledCustomerEmailMessageDTO> GetScheduledCustomerEmailMessageAsync(int id)
        {
            var entity = await _integrationDataContext.CustomerEmailMessages
                .Include(x => x.EmailMessage)
                .Include(x => x.Customer)
                .ThenInclude(x => x.CustomerSettings)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                throw new ArgumentException("Повідомлення не знайдено");
            }

            return ToCustomerEmailDTO(entity);
        }

        public async Task<PagedListDTO<ScheduledCustomerEmailMessageDTO>> GetScheduledCustomerEmailMessagesAsync(
            PagingDTO pagingDTO,
            byte? branchId = null,
            int? targetEmailMessageId = null,
            ScheduledCustomerMessageStatus? status = null,
            MessageTheme? theme = null,
            DateOnly? date = null
            )
        {
            var query = _integrationDataContext.CustomerEmailMessages
                .Include(x => x.EmailMessage)
                .Include(x => x.Customer)
                .ThenInclude(x => x.CustomerSettings)
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.EmailMessage.BranchId == branchId.Value);
            }

            if (targetEmailMessageId.HasValue)
            {
                query = query.Where(x => x.EmailMessageId == targetEmailMessageId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (theme.HasValue)
            {
                query = query.Where(x => x.EmailMessage.Theme == theme);
            }

            if (date.HasValue)
            {
                var dateTime = date.Value.ToDateTime(TimeOnly.MinValue);
                var dateTimeFrom = dateTime.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                var dateTimeTo = dateTime.AddDays(1).FromTimeZoneToUtc(TimeZoneConstants.UATimezone);
                query = query.Where(x => x.EmailMessage.ScheduledDate >= dateTimeFrom && x.EmailMessage.ScheduledDate < dateTimeTo);
            }

            return await query.GetPagedListAsync(pagingDTO, ToCustomerEmailDTO);
        }

        private ScheduledCustomerEmailMessageDTO ToCustomerEmailDTO(CustomerEmailMessage entity)
            => new ScheduledCustomerEmailMessageDTO
            {
                Id = entity.Id,
                EmailMessageId = entity.EmailMessageId,
                BranchId = entity.EmailMessage.BranchId,
                CustomerId = entity.CustomerId,
                CustomerName = entity.Customer.FirstName,
                CustomerEmail = entity.Customer.Email,
                CustomerPhoneNumber = entity.Customer.PhoneNumber,
                CustomerAvatar = _avatarService.GetDto(entity.Customer.CustomerSettings.First(x => x.BranchId == entity.EmailMessage.BranchId).Avatar, (Branches)entity.EmailMessage.BranchId).Path,
                Status = entity.Status,
            };

        public async Task<string> GenerateTargetEmailBodyAsync(CreateEmailParametersDTO parameters)
        {
            var emailParametersDTO = new TargetEmailHandlebarParameters
            {
                mainImageUrl = parameters.mainImage?.file != null ? await GetBase64StringFromImageAsSrcAsync(parameters.mainImage.file) : null,
                mainHeader = parameters.mainHeader,
                secondHeader = parameters.secondHeader,
                text = parameters.text,
                link = ValidateLinkDTO(parameters.link),
                blocks = (await Task.WhenAll(parameters.blocks.Select(async x => new EmailBlockDTO
                {
                    imageUrl = x.image?.file != null ? await GetBase64StringFromImageAsSrcAsync(x.image.file) : null,
                    link = ValidateLinkDTO(x.link),
                    text = x.text,
                    header = x.header
                }))).ToList(),
            };

            return await GenerateTargetEmailBodyAsync(emailParametersDTO, (Branches)parameters.branchId);
        }

        public async Task<(string, List<EmailAttachment>)> GenerateTargetEmailBodyWithEmailAttachmentsAsync(CreateEmailParametersDTO parameters)
        {
            List<EmailAttachment> attachments = new List<EmailAttachment>();
            var emailParametersDTO = new TargetEmailHandlebarParameters
            {
                mainHeader = parameters.mainHeader,
                secondHeader = parameters.secondHeader,
                text = parameters.text,
                link = ValidateLinkDTO(parameters.link)
            };

            if (parameters.mainImage?.file != null)
            {
                var mainImage = new EmailAttachment
                {
                    FileName = parameters.mainImage.file.FileName,
                    Cid = Guid.NewGuid().ToString(),
                    Base64Content = await GetBase64StringAsync(parameters.mainImage.file),
                };
                mainImage.FileName = GetFileName(parameters.mainImage.file, mainImage.Cid);
                emailParametersDTO.mainImageUrl = GetCid(mainImage.Cid);
                attachments.Add(mainImage);
            }

            emailParametersDTO.blocks ??= new();
            foreach (var block in parameters.blocks)
            {
                var emailBlock = new EmailBlockDTO
                {
                    link = ValidateLinkDTO(block.link),
                    text = block.text,
                    header = block.header
                };

                if (block.image?.file != null)
                {
                    var blockImage = new EmailAttachment
                    {
                        Cid = Guid.NewGuid().ToString(),
                        Base64Content = await GetBase64StringAsync(block.image.file),
                    };

                    blockImage.FileName = GetFileName(block.image.file, blockImage.Cid);
                    emailBlock.imageUrl = GetCid(blockImage.Cid);
                    attachments.Add(blockImage);
                }

                emailParametersDTO.blocks.Add(emailBlock);
            }

            var emailTemplate = await GenerateTargetEmailBodyAsync(emailParametersDTO, (Branches)parameters.branchId);

            return (emailTemplate, attachments);
        }

        private async Task<string> GetBase64StringFromImageAsSrcAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var base64string = Convert.ToBase64String(memoryStream.ToArray());
                return $"data:{file.ContentType};base64,{base64string}";
            }
        }

        private async Task<string> GetBase64StringAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        private string GetFileName(IFormFile file, string guid)
        {
            return guid + Path.GetExtension(file.FileName);
        }

        private string GetCid(string guid)
        {
            return $"cid:{guid}";
        }

        private async Task<string> GenerateTargetEmailBodyAsync(TargetEmailHandlebarParameters parameters, Branches branchId)
        {
            var branchImage = branchId == Branches.BirdJet ? BranchImageConstants.BirdJet : BranchImageConstants.CatJet;
            parameters.logoUrl = new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
            {
                Path = $"{StorageConstants.AppPath}/{branchImage}"
            }.ToString();

            parameters.branchEmail = branchId == Branches.BirdJet ? "birdjet@birdjet.com" : "catjet@catjet.com";

            return await _htmlGenerationService.GenerateTargetEmailAsync(parameters);
        }

        public async Task<string> GenerateTargetEmailBodyAsync(TargetEmailMessage targetEmailMessage)
        {
            var handlebarParams = new TargetEmailHandlebarParameters
            {
                mainImageUrl = targetEmailMessage.MainImageUrl,
                mainHeader = targetEmailMessage.MainHeader,
                secondHeader = targetEmailMessage.SecondHeader,
                text = targetEmailMessage.Text,
                link = ToLinkDTO(targetEmailMessage.Link),
                blocks = targetEmailMessage.Blocks.Select(x => new EmailBlockDTO
                {
                    imageUrl = x.ImageUrl,
                    header = x.Header,
                    link = ToLinkDTO(x.Link),
                    text = x.Text,
                }).ToList(),
            };

            return await GenerateTargetEmailBodyAsync(handlebarParams, (Branches) targetEmailMessage.BranchId);
        }

        private async Task<TargetEmailMessage> GetTargetEmailMessageEntityAsync(int targetEmailMessageId)
        {
            var message = await _integrationDataContext.TargetEmailMessages
                .Include(x => x.Link)
                .Include(x => x.Blocks)
                .ThenInclude(x => x.Link)
                .FirstOrDefaultAsync(x => x.Id == targetEmailMessageId);

            if (message == null)
            {
                throw new ArgumentException("Повідомлення не знайдено");
            }

            return message;
        }

        public async Task<string> GenerateTargetEmailBodyAsync(int targetEmailMessageId)
        {
            var message = await GetTargetEmailMessageEntityAsync(targetEmailMessageId);
            return await GenerateTargetEmailBodyAsync(message);
        }

        public async Task SendTestEmailAsync(PreviewEmailParametersDTO parameters)
        {
            var subject = parameters.theme.GetEnumMemberValue();
            (string content, List<EmailAttachment> attachments)  = await GenerateTargetEmailBodyWithEmailAttachmentsAsync(parameters);

            var admin = await _adminService.GetById(_userContext.AdminId!.Value);

            var email = new EmailMessage
            {
                Subject = subject,
                Body = content,
                From = (EmailFrom)parameters.branchId,
                To = new List<string> { admin.Email },
                Attachments = attachments,
            };

            await _notificationService.SendEmailAsync(email);
        }

        public async Task SendTestEmailAsync(int targetEmailMessageId)
        {
            var message = await GetTargetEmailMessageEntityAsync(targetEmailMessageId);
            var subject = message.Theme.GetEnumMemberValue();
            var content = await GenerateTargetEmailBodyAsync(message);

            var admin = await _adminService.GetById(_userContext.AdminId!.Value);

            var email = new EmailMessage
            {
                Subject = subject,
                Body = content,
                From = (EmailFrom)message.BranchId,
                To = new List<string> { admin.Email }
            };

            await _notificationService.SendEmailAsync(email);
        }
    }
}