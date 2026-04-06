using FinanceApp2.Api.Models;
using FinanceApp2.Api.Services.Queues;
using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FinanceApp2.Api.Services.Background
{
    public class EmailSenderService : BackgroundService
    {
        private readonly IEmailSenderQueue _queue;
        private readonly ILogger<EmailSenderService> _logger;
        private readonly IEmailSender _emailSender;

        public EmailSenderService(IEmailSenderQueue queue, ILogger<EmailSenderService> logger, IEmailSender emailSender)
        {
            _queue = queue;
            _logger = logger;
            _emailSender = emailSender;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var emailDetails in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                await ProcessEmailRequestAsync(emailDetails);
            }
        }

        private async Task ProcessEmailRequestAsync(EmailDetails emailDetails)
        {
            using (_logger.BeginLoggingScope(nameof(EmailSenderService), nameof(ProcessEmailRequestAsync)))
            {
                try
                {
                    await _emailSender.SendEmailAsync(emailDetails.EmailAddress, emailDetails.Subject, emailDetails.MessageHtml);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while sending email. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                }
            }
        }
    }
}
