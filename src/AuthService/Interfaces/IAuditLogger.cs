using AuthService.Domain.Entities;

namespace AuthService.Interfaces
{
    public interface IAuditLogger
    {
        Task LogAsync(string userId, string action, string? description);

    }
}
