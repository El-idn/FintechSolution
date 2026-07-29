namespace SharedKernel.Events
{
    public record PaymentCreatedEvent
    {
        public Guid PaymentId { get; init; }
        public Guid AccountId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string? Reference { get; init; }
        public string? Description { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public record PaymentSucceededEvent
    {
        public Guid PaymentId { get; init; }
        public Guid AccountId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string? Reference { get; init; }
        public DateTime ProcessedAt { get; init; }
    }

    public record PaymentFailedEvent
    {
        public Guid PaymentId { get; init; }
        public Guid AccountId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string? Reference { get; init; }
        public DateTime FailedAt { get; init; }
    }

    public record PaymentStatusUpdatedEvent
    {
        public Guid PaymentId { get; init; }
        public Guid AccountId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime UpdatedAt { get; init; }
    }

    public record PaymentExpiredEvent
    {
        public Guid PaymentId { get; init; }
        public Guid AccountId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime ExpiredAt { get; init; }
    }

    public record PaymentRetryStartedEvent
    {
        public Guid PaymentId { get; init; }
        public Guid AccountId { get; init; }
        public decimal Amount { get; init; }
        public int RetryCount { get; init; }
        public DateTime StartedAt { get; init; }
    }
}
