using MassTransit;
using SendGrid;
using JetFlight.Shared.Models.Shared;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using Serilog;

namespace JetFlight.Service.Consumers;

public class EmailConsumer : IConsumer<EmailMessage>
{
    private readonly ISendGridClient _sendGridClient;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger _logger;

    public EmailConsumer(ISendGridClient sendGridClient, IOptions<EmailSettings> emailSettings, ILogger logger)
    {
        _sendGridClient = sendGridClient;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmailMessage> context)
    {
        try
        {
            var mailRequest = context.Message;

            _logger.Information(
                "[EmailConsumer] Processing email - Subject: {Subject}, To: {To}, From enum: {From}",
                mailRequest.Subject, mailRequest.To != null ? string.Join(", ", mailRequest.To) : "", mailRequest.From);

            var sender = _emailSettings.From[mailRequest.From];
            var from = new EmailAddress(sender.Email, sender.Name);

            _logger.Information("[EmailConsumer] Sender resolved - Email: {Email}, Name: {Name}", from.Email, from.Name);

            var subject = mailRequest.Subject;

            var toEmails = mailRequest.To
                .Select(x => new EmailAddress(x))
                .ToList();

            var htmlContent = mailRequest.Body; ;
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, toEmails, subject, string.Empty, htmlContent);

            var ccs = mailRequest.Cc?
                .Select(x => new EmailAddress(x))
                .ToList()
                ?? new List<EmailAddress>();

            if (ccs.Any())
            {
                msg.AddCcs(ccs);
            }

            if (mailRequest.Attachments != null)
            {
                foreach (var attachment in mailRequest.Attachments)
                {
                    msg.AddAttachment(
                        filename: attachment.FileName,
                        base64Content: attachment.Base64Content,
                        disposition: "inline",
                        type: null,
                        content_id: attachment.Cid);
                }
            }

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Body.ReadAsStringAsync();
                _logger.Error(
                    "[EmailConsumer] SendGrid returned error - StatusCode: {StatusCode}, Body: {Body}, Subject: {Subject}, To: {To}",
                    response.StatusCode, content, mailRequest.Subject, string.Join(", ", mailRequest.To));
                throw new HttpRequestException($"StatusCode {response.StatusCode}, Content {content}");
            }

            _logger.Information(
                "[EmailConsumer] Email sent successfully - Subject: {Subject}, To: {To}",
                mailRequest.Subject, string.Join(", ", mailRequest.To));
        }
        catch (Exception ex)
        {
            var msg = context.Message;
            _logger.Error(ex, "[EmailConsumer] Failed to send email. MessageId: {MessageId}, Subject: {Subject}, To: {To}",
                context.MessageId, msg?.Subject ?? "", msg?.To != null ? string.Join(", ", msg.To) : "");
            throw;
        }
    }
}
