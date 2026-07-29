using System.Threading.Tasks;
using AuthService.DTOs;

namespace AuthService.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<AuthResponse> RefreshTokenAsync(RefreshRequest request);

    Task<bool> LogoutAsync(string refreshToken, string? ipAddress = null, string? obnClientId = null);

    Task<VerifyEmailResult> VerifyEmailAsync(string userId, string token);

    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword, string? ipAddress = null, string? obnClientId = null);

    Task<bool> ResetPasswordAsync(string email, string? ipAddress = null, string? obnClientId = null);

}