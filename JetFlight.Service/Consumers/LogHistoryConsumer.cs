using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetFlight.Service.Services;
using JetFlight.Shared.Models.LogHistory;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JetFlight.Service.Consumers
{
    public class LogHistoryConsumer : IConsumer<LogHistoryMessage>
    {
        private readonly ILogger<LogHistoryConsumer> _logger;
        private readonly ILogHistoryService _logHistoryService;

        public LogHistoryConsumer(ILogger<LogHistoryConsumer> logger, ILogHistoryService logHistoryService)
        {
            _logHistoryService = logHistoryService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<LogHistoryMessage> context)
        {
            try
            {
                var historyLogs = context.Message.Logs;

                if (historyLogs == null || !historyLogs.Any())
                {
                    _logger.LogInformation("No logs to process in LogHistoryMessage.");
                    return;
                }

                await _logHistoryService.AddRangeAsync(historyLogs);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process LogHistoryMessage. MessageId: {MessageId}", context.MessageId);
                throw;
            }
        }
    }
}
