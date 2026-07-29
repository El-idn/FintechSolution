namespace SharedKernel.Events
{
    /// <summary>
    /// Event published when a notification is sent
    /// </summary>
    public class NotificationSentEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty; // EMAIL, SMS, PUSH, IN_APP
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
        public bool IsOpenBankingNotification { get; set; }
        public string? DeliveryStatus { get; set; } // SENT, DELIVERED, FAILED
    }

    /// <summary>
    /// Event published when a notification is delivered
    /// </summary>
    public class NotificationDeliveredEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public DateTime DeliveredAt { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
        public string? DeliveryConfirmation { get; set; }
    }

    /// <summary>
    /// Event published when a notification fails to deliver
    /// </summary>
    public class NotificationFailedEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public DateTime FailedAt { get; set; }
        public string FailureReason { get; set; } = string.Empty;
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
        public int RetryCount { get; set; }
        public bool WillRetry { get; set; }
    }

    /// <summary>
    /// Event published for PSD2 Strong Customer Authentication notifications
    /// </summary>
    public class PSD2SCANotificationEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string ObnConsentId { get; set; } = string.Empty;
        public string ObnClientId { get; set; } = string.Empty;
        public string ObnClientName { get; set; } = string.Empty;
        public string SCAMethod { get; set; } = string.Empty; // SMS, EMAIL, APP, etc.
        public string TransactionType { get; set; } = string.Empty; // PAYMENT, CONSENT, etc.
        public string NotificationType { get; set; } = string.Empty; // CHALLENGE, AUTHORIZATION, etc.
        public DateTime SentAt { get; set; }
        public string? ChallengeData { get; set; }
        public string? AuthorizationCode { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Event published for Open Banking consent notifications
    /// </summary>
    public class OpenBankingConsentNotificationEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string ObnConsentId { get; set; } = string.Empty;
        public string ObnClientId { get; set; } = string.Empty;
        public string ObnClientName { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty; // GRANTED, REVOKED, EXPIRED, REMINDER
        public string[] Permissions { get; set; } = Array.Empty<string>();
        public string[] AccountIds { get; set; } = Array.Empty<string>();
        public DateTime SentAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? ActionRequired { get; set; }
    }

    /// <summary>
    /// Event published for transaction notifications
    /// </summary>
    public class TransactionNotificationEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public Guid TransactionId { get; set; }
        public string TransactionType { get; set; } = string.Empty; // DEPOSIT, WITHDRAWAL, TRANSFER, etc.
        public string TransactionStatus { get; set; } = string.Empty; // PENDING, COMPLETED, FAILED, etc.
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty; // CREATED, AUTHORIZED, COMPLETED, FAILED
        public DateTime SentAt { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
        public string? AccountId { get; set; }
        public string? CounterpartyAccount { get; set; }
        public string? Reference { get; set; }
    }

    /// <summary>
    /// Event published for security notifications
    /// </summary>
    public class SecurityNotificationEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string SecurityEventType { get; set; } = string.Empty; // LOGIN_ATTEMPT, PASSWORD_CHANGE, ACCOUNT_LOCKED, etc.
        public string RiskLevel { get; set; } = string.Empty; // LOW, MEDIUM, HIGH, CRITICAL
        public string NotificationType { get; set; } = string.Empty; // ALERT, WARNING, CONFIRMATION
        public DateTime SentAt { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Location { get; set; }
        public string? ObnClientId { get; set; }
        public bool RequiresAction { get; set; }
        public string? ActionRequired { get; set; }
    }

    /// <summary>
    /// Event published for account notifications
    /// </summary>
    public class AccountNotificationEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public Guid AccountId { get; set; }
        public string AccountType { get; set; } = string.Empty; // SAVINGS, CHECKING, CREDIT, etc.
        public string NotificationType { get; set; } = string.Empty; // CREATED, BALANCE_UPDATE, LIMIT_CHANGE, etc.
        public DateTime SentAt { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
        public decimal? BalanceChange { get; set; }
        public string? Currency { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Event published for regulatory notifications (PSD2 compliance)
    /// </summary>
    public class RegulatoryNotificationEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string RegulationType { get; set; } = string.Empty; // PSD2, GDPR, AML, etc.
        public string NotificationType { get; set; } = string.Empty; // COMPLIANCE_UPDATE, POLICY_CHANGE, etc.
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
        public bool RequiresAcknowledgment { get; set; }
        public string? ActionRequired { get; set; }
    }

    /// <summary>
    /// Event published for system maintenance notifications
    /// </summary>
    public class SystemMaintenanceNotificationEvent
    {
        public Guid NotificationId { get; set; }
        public string ServiceName { get; set; } = string.Empty; // AUTH_SERVICE, TRANSACTION_SERVICE, etc.
        public string MaintenanceType { get; set; } = string.Empty; // SCHEDULED, EMERGENCY, UPGRADE, etc.
        public string NotificationType { get; set; } = string.Empty; // SCHEDULED, STARTED, COMPLETED, CANCELLED
        public DateTime SentAt { get; set; }
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        public string? Description { get; set; }
        public string? Impact { get; set; } // NONE, MINIMAL, MODERATE, HIGH
        public bool IsOpenBankingService { get; set; }
        public string? ObnClientId { get; set; }
    }

    /// <summary>
    /// Event published for fraud detection notifications
    /// </summary>
    public class FraudDetectionNotificationEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string FraudType { get; set; } = string.Empty; // SUSPICIOUS_ACTIVITY, UNAUTHORIZED_ACCESS, etc.
        public string RiskLevel { get; set; } = string.Empty; // LOW, MEDIUM, HIGH, CRITICAL
        public string NotificationType { get; set; } = string.Empty; // ALERT, CONFIRMATION, RESOLUTION
        public DateTime SentAt { get; set; }
        public string? IPAddress { get; set; }
        public string? Location { get; set; }
        public string? DeviceInfo { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
        public bool RequiresImmediateAction { get; set; }
        public string? ActionRequired { get; set; }
        public string? TransactionId { get; set; }
    }

    /// <summary>
    /// Event published for notification preferences updates
    /// </summary>
    public class NotificationPreferencesUpdatedEvent
    {
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty; // EMAIL, SMS, PUSH, IN_APP
        public bool IsEnabled { get; set; }
        public string[] EnabledCategories { get; set; } = Array.Empty<string>();
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
    }

    /// <summary>
    /// Event published for notification template updates
    /// </summary>
    public class NotificationTemplateUpdatedEvent
    {
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty; // EMAIL, SMS, PUSH, IN_APP
        public string Language { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public string? Version { get; set; }
        public bool IsOpenBankingTemplate { get; set; }
        public string? RegulationType { get; set; } // PSD2, GDPR, etc.
    }
} 