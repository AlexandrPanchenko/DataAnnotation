using MassTransit;
using JetFlight.Shared.Models.Shared;
using Serilog;
using JetFlight.Service.Services;
using Microsoft.Extensions.Hosting;

namespace JetFlight.Service.Consumers
{
    public class SmsConsumer : IConsumer<SmsMessage>
    {
        private readonly ISMSServiceClient _smsServiceClient;
        private readonly ILogger _logger;
        public SmsConsumer(ISMSServiceClient smsServiceClient, ILogger logger)
        {
            _smsServiceClient = smsServiceClient;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SmsMessage> context)
        {
            try
            {
                var smsRequest = context.Message;

                var senderId = smsRequest.Sender;
                var toNumbers = smsRequest.Recievers;
                var body = smsRequest.Message;

                _logger.Information("[SmsConsumer.Consume] Received SMS from queue - MessageId: {MessageId}, SenderId: {SenderId}, Receivers: {Receivers}, MessageLength: {MessageLength}", 
                    context.MessageId, senderId, string.Join(", ", toNumbers), body?.Length ?? 0);

                if (string.IsNullOrWhiteSpace(senderId))
                {
                    _logger.Error("[SmsConsumer.Consume] SenderId is null or empty. MessageId: {MessageId}, Receivers: {Receivers}", 
                        context.MessageId, string.Join(", ", toNumbers));
                    throw new ArgumentException("SenderId is required");
                }

                var token = await _smsServiceClient.GetTokenAsync();
                _logger.Information("[SmsConsumer.Consume] Got token, sending SMS - MessageId: {MessageId}, SenderId: {SenderId}", 
                    context.MessageId, senderId);

                await _smsServiceClient.SendSMSAsync(token, senderId, toNumbers, body);
                _logger.Information("[SmsConsumer.Consume] SMS successfully sent - MessageId: {MessageId}, SenderId: {SenderId}, Receivers: {Receivers}", 
                    context.MessageId, senderId, string.Join(", ", toNumbers));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[SmsConsumer.Consume] Failed to send SMS - MessageId: {MessageId}, SenderId: {SenderId}, Receivers: {Receivers}", 
                    context.MessageId, context.Message?.Sender, string.Join(", ", context.Message?.Recievers ?? new List<string>()));
                throw;
            }
        }
    }
}