namespace SharedKernel.Events
{
    /// <summary>
    /// Event published when a new transaction is created
    /// </summary>
    public class TransactionCreatedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid AccountId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Currency { get; set; } = "EUR";
        public DateTime Timestamp { get; set; }
        public bool IsOpenBankingCompliant { get; set; }
        public string? ObnConsentId { get; set; }
        public string? ObnClientId { get; set; }
    }

    /// <summary>
    /// Event published when a transaction is processed successfully
    /// </summary>
    public class TransactionProcessedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid AccountId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal NewBalance { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public bool IsOpenBankingCompliant { get; set; }
        public string? ObnConsentId { get; set; }
        public string? ObnClientId { get; set; }
    }

    /// <summary>
    /// Event published when a transaction fails
    /// </summary>
    public class TransactionFailedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid AccountId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string FailureReason { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public DateTime FailedAt { get; set; }
        public bool IsOpenBankingCompliant { get; set; }
        public string? ObnConsentId { get; set; }
        public string? ObnClientId { get; set; }
    }

    /// <summary>
    /// Event published when a transaction is reversed/refunded
    /// </summary>
    public class TransactionReversedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid OriginalTransactionId { get; set; }
        public Guid AccountId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal NewBalance { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public DateTime ReversedAt { get; set; }
        public bool IsOpenBankingCompliant { get; set; }
        public string? ObnConsentId { get; set; }
        public string? ObnClientId { get; set; }
    }

    /// <summary>
    /// Event published for PSD2 compliance - transaction initiation
    /// </summary>
    public class PSD2TransactionInitiatedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid AccountId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Currency { get; set; } = "EUR";
        public string ObnConsentId { get; set; } = string.Empty;
        public string ObnClientId { get; set; } = string.Empty;
        public string ObnClientName { get; set; } = string.Empty;
        public DateTime InitiatedAt { get; set; }
        public string? RedirectUrl { get; set; }
        public string? ChallengeData { get; set; }
    }

    /// <summary>
    /// Event published for PSD2 compliance - transaction authorization
    /// </summary>
    public class PSD2TransactionAuthorizedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid AccountId { get; set; }
        public Guid UserId { get; set; }
        public string ObnConsentId { get; set; } = string.Empty;
        public string ObnClientId { get; set; } = string.Empty;
        public string AuthorizationCode { get; set; } = string.Empty;
        public DateTime AuthorizedAt { get; set; }
        public string? SCAMethod { get; set; } // Strong Customer Authentication method
    }
} 