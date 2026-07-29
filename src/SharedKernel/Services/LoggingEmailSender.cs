using Microsoft.Extensions.Logging;
using SharedKernel.Interfaces;
using System.Threading.Tasks;



namespace SharedKernel.Services
{
    public class LoggingEmailSender : IEmailSender
    {
        private readonly ILogger<LoggingEmailSender> _logger;

        public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string body)
        {
            _logger.LogInformation("Simulated email to {Email}\nSubject: {Subject}\nBody: {Body}", to, subject, body);
            return Task.CompletedTask;
        }
    }
}

