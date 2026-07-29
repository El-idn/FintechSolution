namespace NotificationService.DTOs
{
    /// <summary>
    /// Base notification response
    /// </summary>
    public class NotificationResponse
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // SENT, DELIVERED, FAILED, PENDING
        public DateTime SentAt { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
        public string? ObnClientName { get; set; }
        public bool IsOpenBankingNotification { get; set; }
    }

    /// <summary>
    /// Email notification response
    /// </summary>
    public class EmailNotificationResponse : NotificationResponse
    {
        public string? MessageId { get; set; }
        public string? DeliveryConfirmation { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    /// <summary>
    /// SMS notification response
    /// </summary>
    public class SmsNotificationResponse : NotificationResponse
    {
        public string? MessageId { get; set; }
        public string? DeliveryStatus { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? Carrier { get; set; }
    }

    /// <summary>
    /// Push notification response
    /// </summary>
    public class PushNotificationResponse : NotificationResponse
    {
        public string? DeviceToken { get; set; }
        public string? Platform { get; set; }
        public string? DeliveryStatus { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    /// <summary>
    /// In-app notification response
    /// </summary>
    public class InAppNotificationResponse : NotificationResponse
    {
        public string? Category { get; set; }
        public bool RequiresAcknowledgment { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    /// <summary>
    /// PSD2 SCA notification response
    /// </summary>
    public class PSD2SCANotificationResponse : NotificationResponse
    {
        public string SCAMethod { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public string? AuthorizationCode { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }

    /// <summary>
    /// Open Banking consent notification response
    /// </summary>
    public class OpenBankingConsentNotificationResponse : NotificationResponse
    {
        public string[] Permissions { get; set; } = Array.Empty<string>();
        public string[] AccountIds { get; set; } = Array.Empty<string>();
        public DateTime ExpiresAt { get; set; }
        public string? ActionRequired { get; set; }
    }

    /// <summary>
    /// Transaction notification response
    /// </summary>
    public class TransactionNotificationResponse : NotificationResponse
    {
        public Guid TransactionId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string TransactionStatus { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? AccountId { get; set; }
        public string? CounterpartyAccount { get; set; }
        public string? Reference { get; set; }
    }

    /// <summary>
    /// Security notification response
    /// </summary>
    public class SecurityNotificationResponse : NotificationResponse
    {
        public string SecurityEventType { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string? IPAddress { get; set; }
        public string? Location { get; set; }
        public bool RequiresAction { get; set; }
        public string? ActionRequired { get; set; }
    }

    /// <summary>
    /// Bulk notification response
    /// </summary>
    public class BulkNotificationResponse
    {
        public string BatchId { get; set; } = string.Empty;
        public List<NotificationResponse> Results { get; set; } = new();
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan Duration => CompletedAt - StartedAt;
    }

    /// <summary>
    /// Notification statistics response
    /// </summary>
    public class NotificationStatisticsResponse
    {
        public DateTime Date { get; set; }
        public int TotalSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalFailed { get; set; }
        public decimal DeliveryRate => TotalSent > 0 ? (decimal)TotalDelivered / TotalSent * 100 : 0;
        public Dictionary<string, int> ByType { get; set; } = new();
        public Dictionary<string, int> ByStatus { get; set; } = new();
        public int OpenBankingNotifications { get; set; }
        public int PSD2SCANotifications { get; set; }
    }
} 