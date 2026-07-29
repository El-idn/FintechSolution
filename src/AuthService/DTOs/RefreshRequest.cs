using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    // Open Banking Nigeria fields
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? ObnClientId { get; set; }
    public string? ObnConsentId { get; set; }
}
