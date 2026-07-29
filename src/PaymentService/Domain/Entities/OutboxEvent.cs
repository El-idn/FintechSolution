using System;

namespace PaymentService.Domain.Entities
{
    public class OutboxEvent
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public bool Processed { get; set; } = false;
        public DateTime? ProcessedAt { get; set; }
    }
} 