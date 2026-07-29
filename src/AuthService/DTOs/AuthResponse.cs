namespace AuthService.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; } = false;
    public string? VerificationLink { get; set; } = string.Empty;  // 👈 add this for dev/debugging
    public UserDto? User { get; set; } // Add this for OBN fields
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? ObnClientId { get; set; }
    public string? ObnConsentId { get; set; }
    public string? ObnClientName { get; set; }
}