// AuthService/Repositories/EmailVerificationTokenRepository.cs
using AuthService.Data;
using AuthService.Domain.Entities;
using AuthService.Interfaces;
using AuthService.Repositories;
using Microsoft.EntityFrameworkCore;
using AuthService.Services;

namespace AuthService.Repositories
{
    public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
    {
        private readonly AuthDbContext _context;

        public EmailVerificationTokenRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(EmailVerificationToken token)
        {
            await _context.EmailVerificationTokens.AddAsync(token);
        }

        public async Task<EmailVerificationToken?> GetByTokenAsync(string token)
        {
            return await _context.EmailVerificationTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
