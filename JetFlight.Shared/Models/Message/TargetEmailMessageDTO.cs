using JetFlight.Shared.Models.Targets;
using Microsoft.AspNetCore.Http;

namespace JetFlight.Shared.Models.Message
{
    public class CreateTargetEmailMessageDTO : CreateEmailParametersDTO
    {
        public DateTime scheduledDate { get; set; }
        public MessageTheme theme { get; set; }
        public int targetId { get; set; }
    }

    public class CreateEmailParametersDTO
    {
        public byte branchId { get; set; }
        public CreateEmailImageDTO? mainImage { get; set; }
        public string? mainHeader { get; set; }
        public string? secondHeader { get; set; }
        public string? text { get; set; }
        public EmailLinkDTO? link { get; set; } = null;
        public List<CreateEmailBlockDTO> blocks { get; set; } = new List<CreateEmailBlockDTO>();
    }

    public class PreviewEmailParametersDTO : CreateEmailParametersDTO
    {
        public MessageTheme theme { get; set; }
    }

    public class CreateEmailBlockDTO
    {
        public CreateEmailImageDTO? image { get; set; } = null;
        public string? header { get; set; }
        public string? text { get; set; }
        public EmailLinkDTO? link { get; set; } = null;
    }

    public class CreateEmailImageDTO
    {
        public string? name { get; set; } = null;
        public IFormFile? file { get; set; } = null;
    }

    public class EmailLinkDTO
    {
        public string? url { get; set; }
        public string? text { get; set; }
    }

    public class EmailBlockDTO
    {
        public string? imageUrl { get; set; }
        public string? header { get; set; }
        public string? text { get; set; }
        public EmailLinkDTO? link { get; set; }
    }

    public class TargetEmailMessageDTO
    {
        public int Id { get; set; }
        public byte BranchId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TargetMessageStatus Status { get; set; }
        public MessageTheme Theme { get; set; }
        public string? MainImageUrl { get; set; }
        public string? MainHeader { get; set; }
        public string? SecondHeader { get; set; }
        public string? Text { get; set; }
        public EmailLinkDTO? Link { get; set; }
        public List<EmailBlockDTO> Blocks { get; set; } = new List<EmailBlockDTO>();
        public int TargetId { get; set; }
        public TargetDto Target { get; set; }
        public int PopulatedMessagesCount { get; set; }
    }

    public class ScheduledCustomerEmailMessageDTO
    {
        public int Id { get; set; }
        public int EmailMessageId { get; set; }
        public byte BranchId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerAvatar { get; set; }
        public MessageTheme Theme { get; set; }
        public ScheduledCustomerMessageStatus Status { get; set; }
    }
}
