using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using Microsoft.EntityFrameworkCore;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.Shared.Models.ContactUs;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Constants;
using JetFlight.Service.Extensions;
using Microsoft.Extensions.Options;

namespace JetFlight.Service.Services;



public interface IContactUsService
{
    Task<PagedListDTO<GetContactUsResponseDTO>> GetAll(PagingDTO pagingDTO, int? branchId = null, RequestStatus? requestStatus = null, DateTime? createdTime = null, int? topicId = null);
    public Task<GetContactUsResponseDTO> GetById(int contactUsId);

    public Task<ContactUsDTO> CreateContactUs(ContactUsDTO contactUsDTO, byte branchId);
    Task<List<TopicDTO>> GetAllTopics();
    Task<ContactUsUpdateResponse> UpdateContactUs(ContactUsUpdateRequest contactUs);
    Task<ContactUsChangeStatusAndAssigneResponse> ChangeStatusAndAssignee(int contactUsId, RequestStatus? status = null, int? assigneeId = null);
}

public class ContactUsService : IContactUsService
{
    private readonly IDataUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IHtmlGenerationService _htmlGenerationService;
    private readonly IMediaService _mediaService;
    private readonly SmsSettings _smsSettings;
    public ContactUsService(
        IDataUnitOfWork unitOfWork,
        INotificationService notificationService,
        IHtmlGenerationService htmlGenerationService,
        IMediaService mediaService,
        IOptions<SmsSettings> smsSettings)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _htmlGenerationService = htmlGenerationService;
        _mediaService = mediaService;
        _smsSettings = smsSettings.Value;
    }

    public async Task<List<TopicDTO>> GetAllTopics()
    {
        var query = _unitOfWork.Topic.GetAll().AsQueryable();

        var topics = await query.ToListAsync();
        return topics
             .Select(cu => new TopicDTO
             {
                 Name = cu.Name,
                 Title = cu.Title,
                 Id = cu.Id
             }).ToList();
    }

    public async Task<PagedListDTO<GetContactUsResponseDTO>> GetAll(PagingDTO pagingDTO, int? branchId = null, RequestStatus? requestStatus = null, DateTime? createdTime = null, int? topicId = null)
    {
        var query = _unitOfWork.ContactUs.GetAllWithImages();

        if (branchId.HasValue)
        {
            query = query.Where(cu => cu.BranchId == branchId.Value);
        }

        if (requestStatus.HasValue)
        {
            query = query.Where(cu => cu.Status == requestStatus.Value);
        }

        if (createdTime.HasValue)
        {
            query = query.Where(cu => cu.CreatedAt.Date == createdTime.Value.Date);
        }

        if (topicId.HasValue)
        {
            query = query.Where(cu => cu.Topic.Id == topicId.Value);
        }

        var contactUs = await query.ToListAsync();

        var result = await query.GetPagedListAsync(pagingDTO, ToDTO);
        return result;
    }

    private GetContactUsResponseDTO ToDTO(ContactUs cu)
    {
        return new GetContactUsResponseDTO
        {
            AssigneeId = cu.AssigneeId,
            CreatedAt = cu.CreatedAt,
            CustomerId = cu.CustomerId,
            Email = cu.Email,
            FirstName = cu.FirstName,
            LastName = cu.LastName,
            Id = cu.Id,
            TopicId = cu.TopicId,
            ProcessingDate = cu.ProcessingDate,
            Message = cu.Message,
            BranchId = cu.BranchId,
            PhoneNumber = cu.PhoneNumber,
            ResolveMessage = cu.ResolveMessage,
            ResolveSignature = cu.ResolveSignature,
            Status = cu.Status,
            Files = cu.Attachments.Select(img => new ContactUsAttachmentsDTO
            {
                Id = img.Id,
                FilePath = new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
                {
                    Path = $"{StorageConstants.AppPath}/{img.FilePath}"
                }.ToString(),
                MimeType = img.MimeType,
                Name = img.FileName,
                CreatedAt = img.CreatedAt
            }).ToList()
        };
    }


    public async Task<GetContactUsResponseDTO> GetById(int contactUsId)
    {
        var contactUs = await _unitOfWork.ContactUs.GetAllWithImages().Where(x => x.Id == contactUsId).ToListAsync();
        if (contactUs == null)
        {
            return new GetContactUsResponseDTO();
        }

        return contactUs
            .Select(cu => new GetContactUsResponseDTO
            {
                AssigneeId = cu.AssigneeId,
                CreatedAt = cu.CreatedAt,
                CustomerId = cu.CustomerId,
                Email = cu.Email,
                FirstName = cu.FirstName,
                LastName = cu.LastName,
                Id = cu.Id,
                TopicId = cu.TopicId,
                Message = cu.Message,
                BranchId = cu.BranchId,
                PhoneNumber = cu.PhoneNumber,
                ResolveMessage = cu.ResolveMessage,
                ProcessingDate = cu.ProcessingDate,
                ResolveSignature = cu.ResolveSignature,
                Status = cu.Status,
                Files = cu.Attachments.Select(img => new ContactUsAttachmentsDTO
                {
                    Id = img.Id,
                    FilePath = new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
                    {
                        Path = $"{StorageConstants.AppPath}/{img.FilePath}"
                    }.ToString(),
                    MimeType = img.MimeType,
                    Name = img.FileName,
                    CreatedAt = img.CreatedAt
                }).ToList()
            }).First();
    }

        public async Task<ContactUsDTO> CreateContactUs(ContactUsDTO contactUsDTO, byte branchId)
    {
        var contactUs = new ContactUs()
        {
            AssigneeId = contactUsDTO.assigneeId,
            CreatedAt = DateTime.UtcNow.SetKindUtc(),
            CustomerId = contactUsDTO.customerId,
            Email = contactUsDTO.email,
            FirstName = contactUsDTO.firstName,
            LastName = contactUsDTO.lastName,
            Message = contactUsDTO.message,
            TopicId = contactUsDTO.topicId,
            PhoneNumber = contactUsDTO.phoneNumber,
            ResolveMessage = contactUsDTO.resolveMessage,
            Status = RequestStatus.Pending,
            BranchId = branchId,
        };

        contactUsDTO.branchId = contactUs.BranchId;

        var result = await _unitOfWork.ContactUs.Add(contactUs);
        await _unitOfWork.Save();

        if (contactUsDTO.files != null)
        {
            await AddedFilesForContactUsForm(contactUsDTO.files.ToList(), contactUs.Id);
        }

        if (!string.IsNullOrEmpty(contactUs.Email))
        {
                // Use topic title/name for email text instead of full message body
                var topic = await _unitOfWork.Topic.GetAll()
                    .FirstOrDefaultAsync(t => t.Id == contactUs.TopicId);
                var topicLabel = topic?.Title ?? topic?.Name ?? "звернення";

                var message = await _htmlGenerationService.GenerateContactUsCreateEmail(
                    $"{contactUs.FirstName} {contactUs.LastName}",
                    topicLabel,
                    branchId);
                var from = (EmailFrom)branchId;

            var emailMessage = new EmailMessage
            {
                    Subject = $"Заявка № {contactUs.Id} створена",
                From = from,
                Body = message,
                To = new List<string> { contactUs.Email }
            };

            await _notificationService.SendEmailAsync(emailMessage);
        }

        if (!string.IsNullOrEmpty(contactUs.PhoneNumber))
        {
            var message = $"Ваша заявка #{contactUs.Id} створена.";
            var smsMessage = new SmsMessage
            {
                Message = message,
                Recievers = new List<string> { contactUs.PhoneNumber },
                 
            };

            switch ((byte)contactUs.BranchId)
            {
                case 1:
                    smsMessage.Sender = _smsSettings.From.BirdJet.Id;
                    break;
                case 2:
                    smsMessage.Sender = _smsSettings.From.CatJet.Id;
                    break;
                default:
                    throw new ArgumentException("Invalid branch ID");
            }

            await _notificationService.SendSmsAsync(smsMessage);
        }

        return contactUsDTO;
    }

    private async Task AddedFilesForContactUsForm(List<ContactUsFileDTO?> files, int contactId)
    {
        foreach (var file in files.Where(x => x != null).Select(x => x.file))
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file), "Файл не може бути null");
            }
            if (contactId < 0)
            {
                throw new ArgumentException("Контакт форма не знайдена");
            }

            if (string.IsNullOrEmpty(file.FileName))
            {
                throw new ArgumentException("Назва файла обовязкова", nameof(file.FileName));
            }

            if (string.IsNullOrEmpty(file.ContentType))
            {
                throw new ArgumentException("Контент не може бути пустим", nameof(file.ContentType));
            }

            var filePath = await _mediaService.UploadAsync(file);
            var uniqueName = Path.GetFileName(filePath.ToString());

            var media = new ContactUsAttachment
            {
                MimeType = file.ContentType,
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                FilePath = uniqueName,
                FileName = file.FileName,
                ContactUsId = contactId
            };

            // Save the MediaFiles entity to the database
            var result = await _unitOfWork.ContactUsAttachments.Add(media);
            await _unitOfWork.Save(true);
        }

    }

    public async Task<ContactUsUpdateResponse> UpdateContactUs(ContactUsUpdateRequest contactUs)
    {
        var existingResult = await _unitOfWork.ContactUs.GetAll().Where(x => x.Id == contactUs.Id).SingleOrDefaultAsync();
        var response = new ContactUsUpdateResponse();


        if (existingResult == null)
        {
            response.Errors.Add("Заявку не знайдено");
            return response;
        }

        existingResult.ResolveMessage = contactUs.ResolveMessage;
        existingResult.ResolveSignature = contactUs.ResolveSignature;
        existingResult.Status = RequestStatus.Completed;
        existingResult.ProcessingDate = DateTime.UtcNow.SetKindUtc();

        await _unitOfWork.Save();

        response.Item = new ContactUsDTO
        {
            id = existingResult.Id,
            customerId = existingResult.CustomerId,
            firstName = existingResult.FirstName,
            lastName = existingResult.LastName,
            email = existingResult.Email,
            phoneNumber = existingResult.PhoneNumber,
            topicId = existingResult.TopicId.GetValueOrDefault(),
            message = existingResult.Message,
            updatedAt = DateTime.UtcNow.SetKindUtc(),
            processingDate = existingResult.ProcessingDate,
            assigneeId = existingResult.AssigneeId,
            resolveMessage = existingResult.ResolveMessage,
            resolveSignature = existingResult.ResolveSignature,
            status = existingResult.Status,
            branchId = existingResult.BranchId,
        };

        if (!string.IsNullOrEmpty(existingResult.Email))
        {
            var message = await _htmlGenerationService.GenerateContactUsProcessedEmail($"{existingResult.FirstName} {existingResult.LastName}", existingResult.Message, contactUs.ResolveMessage, contactUs.ResolveSignature, existingResult.BranchId);
            var from = (EmailFrom)existingResult.BranchId;

            var emailMessage = new EmailMessage
            {
                Subject = $"Заявка оброблена #{existingResult.Id}",
                From = from,
                Body = message,
                To = new List<string> { existingResult.Email }
            };

            await _notificationService.SendEmailAsync(emailMessage);
        }

        if (!string.IsNullOrEmpty(existingResult.PhoneNumber))
        {
            var message = $"Ваша заявка #{existingResult.Id} оброблена. {contactUs.ResolveMessage}.";
            var smsMessage = new SmsMessage
            {
                Message = message,
                Recievers = new List<string> { existingResult.PhoneNumber },
            };

            switch ((byte)existingResult.BranchId)
            {
                case 1:
                    smsMessage.Sender = _smsSettings.From.BirdJet.Id;
                    break;
                case 2:
                    smsMessage.Sender = _smsSettings.From.CatJet.Id;
                    break;
                default:
                    throw new ArgumentException("Invalid branch ID");
            }

            await _notificationService.SendSmsAsync(smsMessage);
        }

        return response;
    }

    public async Task<ContactUsChangeStatusAndAssigneResponse> ChangeStatusAndAssignee(int contactUsId, RequestStatus? status = null, int? assigneeId = null)
    {
        var existingResult = await _unitOfWork.ContactUs.GetAll().Where(x => x.Id == contactUsId).SingleOrDefaultAsync();
        var response = new ContactUsChangeStatusAndAssigneResponse();

        if (existingResult == null)
        {
            response.Errors.Add("Заявку не знайдено");
            return response;
        }

        existingResult.AssigneeId = FieldUpdater.UpdateIfNotNullOrEmpty(existingResult.AssigneeId, assigneeId);
        existingResult.UpdatedAt = DateTime.UtcNow.SetKindUtc();
        if (status.HasValue)
        {
            existingResult.Status = status.Value;
            if (status.Value == RequestStatus.Completed || status.Value == RequestStatus.CompletedByPhoneCall)
            {
                existingResult.ProcessingDate = DateTime.UtcNow.SetKindUtc();
            }
        }

        await _unitOfWork.Save();

        response.Response = true;
        return response;
    }
}