// AuthService/Repositories/Interfaces/IEmailVerificationTokenRepository.cs
using AuthService.Domain.Entities;

namespace AuthService.Interfaces
{
    public interface IEmailVerificationTokenRepository
    {
        Task AddAsync(EmailVerificationToken token);
        Task<EmailVerificationToken?> GetByTokenAsync(string token);
        Task SaveChangesAsync();
    }
}
