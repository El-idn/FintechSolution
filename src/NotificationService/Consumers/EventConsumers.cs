using MassTransit;
using NotificationService.DTOs;
using NotificationService.Services;
using SharedKernel.Events;

namespace NotificationService.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<UserRegisteredConsumer> _logger;

        public UserRegisteredConsumer(INotificationService notificationService, ILogger<UserRegisteredConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Sending welcome notification for user {UserId}", message.UserId);

            await _notificationService.SendEmailAsync(new EmailNotificationRequest
            {
                UserId = message.UserId,
                UserEmail = message.Email,
                NotificationType = "EMAIL",
                Subject = "Welcome to Fintech",
                Content = $"Welcome {message.UserName}! Your account has been created.",
                ObnClientId = message.ObnClientId,
                ObnConsentId = message.ObnConsentId,
                IsOpenBankingNotification = message.IsOpenBankingUser
            });
        }
    }

    public class TransactionCreatedConsumer : IConsumer<TransactionCreatedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<TransactionCreatedConsumer> _logger;

        public TransactionCreatedConsumer(INotificationService notificationService, ILogger<TransactionCreatedConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Sending transaction created notice for {TransactionId}", message.TransactionId);

            await _notificationService.SendTransactionAsync(new TransactionNotificationRequest
            {
                UserId = message.UserId,
                UserEmail = $"{message.UserId}@users.local",
                NotificationType = "EMAIL",
                Subject = "Transaction created",
                Content = $"Transaction {message.TransactionId} ({message.TransactionType}) for {message.Amount} {message.Currency} was created.",
                TransactionId = message.TransactionId,
                TransactionType = message.TransactionType,
                TransactionStatus = "Pending",
                Amount = message.Amount,
                Currency = message.Currency,
                AccountId = message.AccountId.ToString(),
                Reference = message.Reference,
                ObnClientId = message.ObnClientId,
                ObnConsentId = message.ObnConsentId,
                IsOpenBankingNotification = message.IsOpenBankingCompliant
            });
        }
    }

    public class TransactionProcessedConsumer : IConsumer<TransactionProcessedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<TransactionProcessedConsumer> _logger;

        public TransactionProcessedConsumer(INotificationService notificationService, ILogger<TransactionProcessedConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TransactionProcessedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Sending transaction processed notice for {TransactionId}", message.TransactionId);

            await _notificationService.SendTransactionAsync(new TransactionNotificationRequest
            {
                UserId = message.UserId,
                UserEmail = $"{message.UserId}@users.local",
                NotificationType = "EMAIL",
                Subject = "Transaction processed",
                Content = $"Transaction {message.TransactionId} completed with status {message.Status}.",
                TransactionId = message.TransactionId,
                TransactionType = message.TransactionType,
                TransactionStatus = message.Status,
                Amount = message.Amount,
                Currency = "EUR",
                AccountId = message.AccountId.ToString(),
                Reference = message.Reference,
                ObnClientId = message.ObnClientId,
                ObnConsentId = message.ObnConsentId,
                IsOpenBankingNotification = message.IsOpenBankingCompliant
            });
        }
    }

    public class PaymentSucceededNotificationConsumer : IConsumer<PaymentSucceededEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentSucceededNotificationConsumer> _logger;

        public PaymentSucceededNotificationConsumer(INotificationService notificationService, ILogger<PaymentSucceededNotificationConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Sending payment succeeded notice for {PaymentId}", message.PaymentId);

            await _notificationService.SendInAppAsync(new InAppNotificationRequest
            {
                UserId = Guid.Empty,
                UserEmail = "payments@fintech.local",
                NotificationType = "IN_APP",
                Subject = "Payment succeeded",
                Content = $"Payment {message.PaymentId} for {message.Amount} {message.Currency} succeeded.",
                Category = "PAYMENT"
            });
        }
    }

    public class PaymentFailedNotificationConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentFailedNotificationConsumer> _logger;

        public PaymentFailedNotificationConsumer(INotificationService notificationService, ILogger<PaymentFailedNotificationConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Sending payment failed notice for {PaymentId}", message.PaymentId);

            await _notificationService.SendInAppAsync(new InAppNotificationRequest
            {
                UserId = Guid.Empty,
                UserEmail = "payments@fintech.local",
                NotificationType = "IN_APP",
                Subject = "Payment failed",
                Content = $"Payment {message.PaymentId} for {message.Amount} {message.Currency} failed.",
                Category = "PAYMENT"
            });
        }
    }

    public class SecurityAlertConsumer : IConsumer<AuthenticationFailedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<SecurityAlertConsumer> _logger;

        public SecurityAlertConsumer(INotificationService notificationService, ILogger<SecurityAlertConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuthenticationFailedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Sending security alert for failed auth on {Email}", message.Email);

            await _notificationService.SendSecurityAsync(new SecurityNotificationRequest
            {
                UserId = Guid.Empty,
                UserEmail = message.Email,
                NotificationType = "EMAIL",
                Subject = "Security alert: authentication failed",
                Content = $"Authentication failed: {message.FailureReason}",
                SecurityEventType = "LOGIN_ATTEMPT",
                RiskLevel = message.IsAccountLocked ? "HIGH" : "MEDIUM",
                IPAddress = message.IPAddress,
                UserAgent = message.UserAgent,
                RequiresAction = message.IsAccountLocked,
                ActionRequired = message.IsAccountLocked ? "Unlock account" : null,
                ObnClientId = message.ObnClientId
            });
        }
    }
}
