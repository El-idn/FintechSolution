using AuthService.Domain.Entities;

namespace AuthService.Interfaces;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
    RefreshToken GenerateRefreshToken();
}
