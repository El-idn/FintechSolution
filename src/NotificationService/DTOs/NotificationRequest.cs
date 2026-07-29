using System.ComponentModel.DataAnnotations;

namespace NotificationService.DTOs
{
    /// <summary>
    /// Base notification request
    /// </summary>
    public class NotificationRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        public string NotificationType { get; set; } = string.Empty; // EMAIL, SMS, PUSH, IN_APP

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? TemplateName { get; set; }
        public string? Language { get; set; } = "en";
        public Dictionary<string, object>? TemplateData { get; set; }

        // Open Banking fields
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
        public bool IsOpenBankingNotification { get; set; }
        public string? RegulationType { get; set; } // PSD2, GDPR, AML, etc.
    }

    /// <summary>
    /// Email notification request
    /// </summary>
    public class EmailNotificationRequest : NotificationRequest
    {
        public string? ReplyTo { get; set; }
        public string[]? CC { get; set; }
        public string[]? BCC { get; set; }
        public bool IsHtml { get; set; } = true;
        public string? Priority { get; set; } // LOW, NORMAL, HIGH, URGENT
    }

    /// <summary>
    /// SMS notification request
    /// </summary>
    public class SmsNotificationRequest : NotificationRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? SenderId { get; set; }
        public bool IsUrgent { get; set; }
    }

    /// <summary>
    /// Push notification request
    /// </summary>
    public class PushNotificationRequest : NotificationRequest
    {
        public string? DeviceToken { get; set; }
        public string? Platform { get; set; } // IOS, ANDROID, WEB
        public Dictionary<string, object>? Payload { get; set; }
        public int? TTL { get; set; } // Time to live in seconds
        public bool IsSilent { get; set; }
    }

    /// <summary>
    /// In-app notification request
    /// </summary>
    public class InAppNotificationRequest : NotificationRequest
    {
        public string? Category { get; set; }
        public string? ActionUrl { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public bool RequiresAcknowledgment { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// PSD2 SCA notification request
    /// </summary>
    public class PSD2SCANotificationRequest : NotificationRequest
    {
        [Required]
        public string ObnClientName { get; set; } = string.Empty;

        [Required]
        public string SCAMethod { get; set; } = string.Empty; // SMS, EMAIL, APP, etc.

        [Required]
        public string TransactionType { get; set; } = string.Empty; // PAYMENT, CONSENT, etc.

        public string? ChallengeData { get; set; }
        public string? AuthorizationCode { get; set; }
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);
    }

    /// <summary>
    /// Open Banking consent notification request
    /// </summary>
    public class OpenBankingConsentNotificationRequest : NotificationRequest
    {
        [Required]
        public string ObnClientName { get; set; } = string.Empty;

        public string[] Permissions { get; set; } = Array.Empty<string>();
        public string[] AccountIds { get; set; } = Array.Empty<string>();
        public DateTime ExpiresAt { get; set; }
        public string? ActionRequired { get; set; }
    }

    /// <summary>
    /// Transaction notification request
    /// </summary>
    public class TransactionNotificationRequest : NotificationRequest
    {
        [Required]
        public Guid TransactionId { get; set; }

        [Required]
        public string TransactionType { get; set; } = string.Empty; // DEPOSIT, WITHDRAWAL, TRANSFER, etc.

        [Required]
        public string TransactionStatus { get; set; } = string.Empty; // PENDING, COMPLETED, FAILED, etc.

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = string.Empty;

        public string? AccountId { get; set; }
        public string? CounterpartyAccount { get; set; }
        public string? Reference { get; set; }
    }

    /// <summary>
    /// Security notification request
    /// </summary>
    public class SecurityNotificationRequest : NotificationRequest
    {
        [Required]
        public string SecurityEventType { get; set; } = string.Empty; // LOGIN_ATTEMPT, PASSWORD_CHANGE, ACCOUNT_LOCKED, etc.

        [Required]
        public string RiskLevel { get; set; } = string.Empty; // LOW, MEDIUM, HIGH, CRITICAL

        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Location { get; set; }
        public bool RequiresAction { get; set; }
        public string? ActionRequired { get; set; }
    }

    /// <summary>
    /// Bulk notification request
    /// </summary>
    public class BulkNotificationRequest
    {
        [Required]
        public List<NotificationRequest> Notifications { get; set; } = new();

        public string? BatchId { get; set; }
        public bool SendInParallel { get; set; } = true;
        public int? MaxConcurrency { get; set; } = 10;
    }
} 