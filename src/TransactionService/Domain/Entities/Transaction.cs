using TransactionService.Domain.Enums;

namespace TransactionService.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal NewBalance { get; set; }
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public string? Description { get; set; }
        public string? Reference { get; set; }
        public string? ExternalReference { get; set; }
        public string? Currency { get; set; } = "EUR";
        public DateTime Timestamp { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? FailureReason { get; set; }
        public bool IsOpenBankingCompliant { get; set; } = true;
        public string? ObnConsentId { get; set; }
        public string? ObnClientId { get; set; } // Third Party Provider ID
    }
}
