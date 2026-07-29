using System;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentStatus Status { get; set; }
        public string? Reference { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTime? ExpiresAt { get; set; }
        public string? IdempotencyKey { get; set; }

        public const int DescriptionMaxLength = 200;
        public static readonly string[] ForbiddenWords = new[] { "fraud", "illegal", "scam" };

        public static bool IsValidStatusTransition(PaymentStatus from, PaymentStatus to)
        {
            if (from == to) return true;
            return from switch
            {
                PaymentStatus.Pending => to == PaymentStatus.Succeeded || to == PaymentStatus.Failed || to == PaymentStatus.Expired,
                PaymentStatus.Failed => to == PaymentStatus.Pending, // allow retry
                _ => false
            };
        }
    }
} 