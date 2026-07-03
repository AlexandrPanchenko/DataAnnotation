using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Shared;
using MassTransit;
using Serilog;

namespace JetFlight.Service.Services;

public interface INotificationService
{
    Task SendEmailAsync(EmailMessage mailRequest, CancellationToken ct = default);
    Task SendSmsAsync(SmsMessage smsRequest, CancellationToken ct = default);
}
public class NotificationService : INotificationService
{
    private readonly IBus _bus;
    private readonly ILogger _logger;

    public NotificationService(IBus bus, ILogger logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailMessage mailRequest, CancellationToken ct = default)
    {
        if (mailRequest.To.Count == 0)
        {
            throw new ArgumentException("Recievers are required");
        }

        _logger.Information(
            "[NotificationService.SendEmailAsync] Publishing email to queue - Subject: {Subject}, To: {To}, From: {From}",
            mailRequest.Subject, string.Join(", ", mailRequest.To), mailRequest.From);

        var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"queue:{NotificationConstant.EmailQueue}"));
        await sendEndpoint.Send(mailRequest, ct);
    }

    public async Task SendSmsAsync(SmsMessage smsRequest, CancellationToken ct = default)
    {
        if (smsRequest.Recievers.Count == 0)
        {
            throw new ArgumentException("Recievers are required");
        }

        if (string.IsNullOrWhiteSpace(smsRequest.Sender))
        {
            _logger.Error("[NotificationService.SendSmsAsync] SenderId is null or empty. Receivers: {Receivers}", 
                string.Join(", ", smsRequest.Recievers));
            throw new ArgumentException("SenderId is required");
        }

        _logger.Information("[NotificationService.SendSmsAsync] Publishing SMS to queue - SenderId: {SenderId}, Receivers: {Receivers}, MessageLength: {MessageLength}", 
            smsRequest.Sender, string.Join(", ", smsRequest.Recievers), smsRequest.Message?.Length ?? 0);

        try
        {
            var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"queue:{NotificationConstant.SmsQueue}"));
            await sendEndpoint.Send(smsRequest, ct);
            _logger.Information("[NotificationService.SendSmsAsync] SMS successfully published to queue - SenderId: {SenderId}", smsRequest.Sender);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[NotificationService.SendSmsAsync] Failed to publish SMS to queue - SenderId: {SenderId}, Receivers: {Receivers}", 
                smsRequest.Sender, string.Join(", ", smsRequest.Recievers));
            throw;
        }
    }
}
