using System;

namespace AuthService.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ReferenceId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
