using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = string.Empty;

    // Open Banking Nigeria fields
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? ObnClientId { get; set; }
    public string? ObnConsentId { get; set; }
    public string? ObnClientName { get; set; }
}
