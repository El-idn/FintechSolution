namespace SharedKernel.Events
{
    // Account Events for Open Banking
    public record AccountCreatedEvent
    {
        public Guid AccountId { get; init; }
        public Guid UserId { get; init; }
        public string AccountNumber { get; init; } = string.Empty;
        public decimal InitialBalance { get; init; }
        public DateTime CreatedAt { get; init; }
        public string AccountType { get; init; } = string.Empty;
    }

    public record AccountBalanceChangedEvent
    {
        public Guid AccountId { get; init; }
        public Guid UserId { get; init; }
        public decimal PreviousBalance { get; init; }
        public decimal NewBalance { get; init; }
        public decimal ChangeAmount { get; init; }
        public string ChangeReason { get; init; } = string.Empty;
        public DateTime ChangedAt { get; init; }
        public Guid? TransactionId { get; init; }
    }

    public record AccountAccessGrantedEvent
    {
        public Guid AccountId { get; init; }
        public Guid UserId { get; init; }
        public string ObnClientId { get; init; } = string.Empty; // Open Banking Nigeria Client ID
        public string ConsentId { get; init; } = string.Empty;
        public string[] Permissions { get; init; } = Array.Empty<string>();
        public DateTime GrantedAt { get; init; }
        public DateTime ExpiresAt { get; init; }
    }

    public record AccountAccessRevokedEvent
    {
        public Guid AccountId { get; init; }
        public Guid UserId { get; init; }
        public string ObnClientId { get; init; } = string.Empty;
        public string ConsentId { get; init; } = string.Empty;
        public string RevocationReason { get; init; } = string.Empty;
        public DateTime RevokedAt { get; init; }
    }

    // Consent Management Events
    public record ConsentGrantedEvent
    {
        public string ConsentId { get; init; } = string.Empty;
        public Guid UserId { get; init; }
        public string ObnClientId { get; init; } = string.Empty;
        public string[] AccountIds { get; init; } = Array.Empty<string>();
        public string[] Permissions { get; init; } = Array.Empty<string>();
        public DateTime GrantedAt { get; init; }
        public DateTime ExpiresAt { get; init; }
    }

    public record ConsentRevokedEvent
    {
        public string ConsentId { get; init; } = string.Empty;
        public Guid UserId { get; init; }
        public string ObnClientId { get; init; } = string.Empty;
        public string RevocationReason { get; init; } = string.Empty;
        public DateTime RevokedAt { get; init; }
    }

    // Notification Events (PSD2 requirement)
    public record AccountAccessNotificationEvent
    {
        public Guid AccountId { get; init; }
        public Guid UserId { get; init; }
        public string ObnClientId { get; init; } = string.Empty;
        public string NotificationType { get; init; } = string.Empty; // "GRANTED", "REVOKED", "EXPIRED"
        public DateTime NotificationTime { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    public record PaymentNotificationEvent
    {
        public Guid TransactionId { get; init; }
        public Guid AccountId { get; init; }
        public Guid UserId { get; init; }
        public string NotificationType { get; init; } = string.Empty; // "INITIATED", "AUTHORIZED", "COMPLETED", "FAILED"
        public DateTime NotificationTime { get; init; }
        public string Message { get; init; } = string.Empty;
    }
} 