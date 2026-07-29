using AuthService.DTOs;
using AuthService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using SharedKernel.Controllers;
using AuthService.Repositories;
using SharedKernel.Services;
using SharedKernel.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AuthService.Domain.Entities;
using AuthService.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly AuthDbContext _dbContext;
    private readonly ILogger<AuthController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(IAuthService authService, AuthDbContext dbContext, IJwtService jwtService, ILogger<AuthController> logger, UserManager<ApplicationUser> userManager)
    {
        _authService = authService;
        _dbContext = dbContext;
        _jwtService = jwtService;
        _logger = logger;
        _userManager = userManager;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration request</param>
    /// <returns>Authentication response</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.RegisterAsync(request);
            _logger.LogInformation("User registration successful for email: {Email}", request.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User registration failed for email: {Email}", request.Email);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Verifies a user's email address.
    /// </summary>
    /// <param name="token">Email verification token</param>
    /// <param name="userId">User ID</param>
    /// <returns>Verification result</returns>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Email verification failed - missing token or userId");
            return BadRequest(new { error = "Token and UserId are required" });
        }

        try
        {
            var result = await _authService.VerifyEmailAsync(userId, token);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Email verification failed for user {UserId}: {Message}", userId, result.Message);
                return BadRequest(new { error = result.Message });
            }

            _logger.LogInformation("Email verification successful for user: {UserId}", userId);
            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email verification error for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred during email verification." });
        }
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>Authentication response with tokens</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.LoginAsync(request);
            _logger.LogInformation("User login successful for email: {Email}", request.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User login failed for email: {Email}", request.Email);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New authentication response</returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.RefreshTokenAsync(request);
            _logger.LogInformation("Token refresh successful");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Logs out a user by revoking their refresh token.
    /// </summary>
    /// <param name="request">Logout request</param>
    /// <returns>Logout result</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var ipAddress = GetClientIPAddress();
            var result = await _authService.LogoutAsync(request.RefreshToken, ipAddress, request.ObnClientId);
            
            if (result)
            {
                _logger.LogInformation("User logout successful");
                return Ok(new { message = "Logged out successfully" });
            }
            
            return BadRequest(new { error = "Logout failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User logout failed");
            return StatusCode(500, new { error = "An error occurred during logout." });
        }
    }

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="request">Password change request</param>
    /// <returns>Password change result</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId().ToString();
            var ipAddress = GetClientIPAddress();
            var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, ipAddress, request.ObnClientId);
            
            if (result)
            {
                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return Ok(new { message = "Password changed successfully" });
            }
            
            return BadRequest(new { error = "Password change failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password change failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Initiates a password reset process.
    /// </summary>
    /// <param name="request">Password reset request</param>
    /// <returns>Password reset result</returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var ipAddress = GetClientIPAddress();
            var result = await _authService.ResetPasswordAsync(request.Email, ipAddress, request.ObnClientId);
            
            if (result)
            {
                _logger.LogInformation("Password reset email sent to: {Email}", request.Email);
                return Ok(new { message = "Password reset email sent successfully" });
            }
            
            return BadRequest(new { error = "Password reset failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset failed for email: {Email}", request.Email);
            return StatusCode(500, new { error = "An error occurred during password reset." });
        }
    }

    /// <summary>
    /// Gets the current user's profile information.
    /// </summary>
    /// <returns>User profile</returns>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetUserId();
            var user = await _dbContext.Users.FindAsync(userId);
            
            if (user == null)
            {
                return NotFound("User not found");
            }

            var profile = new
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                EmailConfirmed = user.EmailConfirmed
            };

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, new { error = "An error occurred while retrieving profile." });
        }
    }

    /// <summary>
    /// Gets Open Banking consent information for the current user.
    /// </summary>
    /// <returns>Consent information</returns>
    [HttpGet("consents")]
    [Authorize]
    public IActionResult GetConsents()
    {
        try
        {
            var userId = GetUserId();
            // This would typically query a consent management system
            // For now, return a placeholder response
            var consents = new
            {
                UserId = userId,
                ActiveConsents = new List<object>(),
                TotalConsents = 0
            };

            return Ok(consents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user consents");
            return StatusCode(500, new { error = "An error occurred while retrieving consents." });
        }
    }

    #if DEBUG
    /// <summary>
    /// Test-only endpoint to confirm a user's email by email address.
    /// </summary>
    [HttpPost("test/confirm-email")]
    public async Task<IActionResult> ConfirmEmailForTest([FromBody] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return NotFound();
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
        return Ok();
    }
    #endif

    private string GetClientIPAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

// Additional DTOs for new endpoints
public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
    public string? ObnClientId { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string? ObnClientId { get; set; }
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string? ObnClientId { get; set; }
}