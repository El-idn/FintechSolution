using AuthService.Data;
using AuthService.Domain.Entities;
using AuthService.Interfaces;

namespace AuthService.Services
{
    public class AuditLogger : IAuditLogger
    {
        private readonly AuthDbContext _dbContext;

        public AuditLogger(AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LogAsync(string actionType, string description, string? referenceId = null)
        {
            var log = new AuditLog
            {
                ActionType = actionType,
                Description = description,
                ReferenceId = referenceId
            };

            _dbContext.AuditLogs.Add(log);
            await _dbContext.SaveChangesAsync();
        }
    }
}
