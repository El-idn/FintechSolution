using System.Threading.Tasks;
using AuthService.Domain.Entities;

namespace AuthService.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeAsync(string token);
}
