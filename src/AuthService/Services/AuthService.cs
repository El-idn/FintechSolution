using System;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Domain.Entities;
using AuthService.DTOs;
using AuthService.Interfaces;
using SharedKernel.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Controllers;
using AuthService.Repositories;
using SharedKernel.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MassTransit;
using SharedKernel.Events;
using System.Net;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtService _jwtService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
        private readonly string _frontendBaseUrl;
        private readonly ILogger<AuthService> _logger;
        private readonly IAuditLogger _auditLogger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly bool _bypassEmailConfirmation;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IRefreshTokenRepository refreshTokenRepository,
            IJwtService jwtService,
            IEmailSender emailSender,
            IEmailVerificationTokenRepository emailVerificationTokenRepository,
            IConfiguration configuration,
            IAuditLogger auditLogger,
            ILogger<AuthService> logger,
            IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
            _emailSender = emailSender;
            _emailVerificationTokenRepository = emailVerificationTokenRepository;
            _auditLogger = auditLogger;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _frontendBaseUrl = configuration["Frontend:BaseUrl"]
                ?? throw new Exception("Frontend BaseUrl is missing in configuration.");
            _bypassEmailConfirmation = configuration.GetValue<bool>("Auth:BypassEmailConfirmation");
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation("Processing registration for email: {Email}", request.Email);

            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - user already exists: {Email}", request.Email);
                await PublishAuthenticationFailedEvent(request.Email, "User already exists", null, null, 0, false);
                throw new Exception("User already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User creation failed for {Email}: {Errors}", request.Email, errorMessage);
                await PublishAuthenticationFailedEvent(request.Email, errorMessage, null, null, 0, false);
                throw new Exception($"Failed to create user: {errorMessage}");
            }

            // Assign CreateAccount permission claim to new user
            var claimResult = await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Permission", SharedKernel.Constants.PermissionConstants.CreateAccount));
            if (!claimResult.Succeeded)
            {
                var errorMessage = string.Join(", ", claimResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to add CreateAccount claim for {Email}: {Errors}", request.Email, errorMessage);
                throw new Exception($"Failed to add CreateAccount claim: {errorMessage}");
            }

            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);

            // Build verification links
            var apiLink = $"{_frontendBaseUrl}/api/auth/verify-email?userId={user.Id}&token={encodedToken}";
            var frontendLink = $"{_frontendBaseUrl}/verify-email?userId={user.Id}&token={encodedToken}";

            var emailBody = $@"
                <p>Thank you for registering!</p>
                <p>Please verify your email by clicking one of the links below:</p>
                <ul>
                    <li><a href='{frontendLink}'>Verify via Frontend</a></li>
                    <li><a href='{apiLink}'>Verify via API</a></li>
                </ul>
                <p>If you did not register, please ignore this email.</p>
            ";

            await _emailSender.SendEmailAsync(user.Email, "Verify your email", emailBody);

            // Publish Open Banking event
            await PublishUserRegisteredEvent(user, request.ObnClientId, request.ObnConsentId);

            _logger.LogInformation("User registered successfully: {UserId}, Email: {Email}", user.Id, user.Email);
            _logger.LogInformation("[DEV ONLY] Email verification token for {Email}: {Token}", user.Email, token);

            return new AuthResponse
            {
                Token = string.Empty, // No JWT until email is confirmed
                RefreshToken = string.Empty, // No refresh token until email is confirmed
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    ObnClientId = request.ObnClientId,
                    ObnConsentId = request.ObnConsentId,
                    ObnClientName = request.ObnClientName
                }
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("Processing login for email: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
                await PublishAuthenticationFailedEvent(request.Email, "Invalid credentials", request.IPAddress, request.UserAgent, 1, false);
                throw new Exception("Invalid credentials.");
            }

            var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isValidPassword)
            {
                await _userManager.AccessFailedAsync(user);
                var failedCount = await _userManager.GetAccessFailedCountAsync(user);

                if (await _userManager.IsLockedOutAsync(user))
                {
                    _logger.LogWarning("Login failed - account locked: {Email}", request.Email);
                    await PublishUserAccountLockedEvent(user, "Too many failed attempts", request.IPAddress);
                    await PublishAuthenticationFailedEvent(request.Email, "Account locked", request.IPAddress, request.UserAgent, failedCount, true);
                    throw new Exception("Account locked. Try again later.");
                }

                _logger.LogWarning("Login failed - invalid password: {Email}, Failed attempts: {FailedCount}", request.Email, failedCount);
                await PublishAuthenticationFailedEvent(request.Email, "Invalid credentials", request.IPAddress, request.UserAgent, failedCount, false);
                throw new Exception("Invalid credentials.");
            }

            if (!_bypassEmailConfirmation && !user.EmailConfirmed)
            {
                _logger.LogWarning("Login failed - email not confirmed: {Email}", request.Email);
                await PublishAuthenticationFailedEvent(request.Email, "Email not confirmed", request.IPAddress, request.UserAgent, 0, false);
                throw new Exception("Email not confirmed.");
            }

            // Reset access failed count
            await _userManager.ResetAccessFailedCountAsync(user);

            var jwtToken = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            refreshToken.UserId = user.Id;
            await _refreshTokenRepository.AddAsync(refreshToken);

            // Publish Open Banking event
            await PublishUserLoggedInEvent(user, request.IPAddress, request.UserAgent, request.ObnClientId, request.ObnConsentId);

            _logger.LogInformation("User logged in successfully: {UserId}, Email: {Email}", user.Id, user.Email);

            return new AuthResponse
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    ObnClientId = request.ObnClientId,
                    ObnConsentId = request.ObnConsentId,
                    ObnClientName = request.ObnClientName
                }
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshRequest request)
        {
            _logger.LogInformation("Processing token refresh");

            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (refreshToken == null || refreshToken.IsExpired)
            {
                _logger.LogWarning("Token refresh failed - invalid or expired refresh token");
                throw new Exception("Invalid or expired refresh token.");
            }

            var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
            if (user == null)
            {
                _logger.LogWarning("Token refresh failed - user not found: {UserId}", refreshToken.UserId);
                throw new Exception("User not found.");
            }

            // Revoke the old refresh token
            await _refreshTokenRepository.RevokeAsync(refreshToken.Token);

            // Generate new tokens
            var jwtToken = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            newRefreshToken.UserId = user.Id;
            await _refreshTokenRepository.AddAsync(newRefreshToken);

            // Publish Open Banking event
            await PublishRefreshTokenUsedEvent(user, refreshToken.Token, request.IPAddress, request.ObnClientId, request.ObnConsentId);

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);

            return new AuthResponse
            {
                Token = jwtToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken, string? ipAddress = null, string? obnClientId = null)
        {
            _logger.LogInformation("Processing logout");

            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token != null)
            {
                await _refreshTokenRepository.RevokeAsync(refreshToken);
                
                var user = await _userManager.FindByIdAsync(token.UserId.ToString());
                if (user != null)
                {
                    await PublishUserLoggedOutEvent(user, ipAddress, obnClientId);
                    await PublishRefreshTokenRevokedEvent(user, token.Token, "User logout", null, obnClientId);
                }

                _logger.LogInformation("User logged out successfully: {UserId}", token.UserId);
            }

            return true;
        }

        public async Task<VerifyEmailResult> VerifyEmailAsync(string userId, string token)
        {
            _logger.LogInformation("Processing email verification for user: {UserId}", userId);

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                await _auditLogger.LogAsync("unknown", "EMAIL_VERIFICATION_FAILED", "Missing token or user ID.");
                return new VerifyEmailResult { Succeeded = false, Message = "Missing token or user ID." };
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                await _auditLogger.LogAsync(userId, "EMAIL_VERIFICATION_FAILED", "User not found.");
                return new VerifyEmailResult { Succeeded = false, Message = "User not found." };
            }

            var decodedToken = Uri.UnescapeDataString(token);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
                await _auditLogger.LogAsync(userId, "EMAIL_VERIFICATION_FAILED", errorMsg);
                _logger.LogWarning("Email verification failed for user {UserId}: {Error}", userId, errorMsg);
                return new VerifyEmailResult { Succeeded = false, Message = errorMsg };
            }

            await _emailSender.SendEmailAsync(user.Email ?? "no-email", "Email Verified", "Your email has been successfully verified.");

            // Publish Open Banking event
            await PublishUserEmailVerifiedEvent(user);

            await _auditLogger.LogAsync(userId, "EMAIL_VERIFICATION_SUCCESS", "User verified their email successfully.");
            _logger.LogInformation("Email verified successfully for user: {UserId}, Email: {Email}", user.Id, user.Email);

            return new VerifyEmailResult { Succeeded = true, Message = "Email verified successfully." };
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword, string? ipAddress = null, string? obnClientId = null)
        {
            _logger.LogInformation("Processing password change for user: {UserId}", userId);

            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Password change failed - invalid user ID format: {UserId}", userId);
                return false;
            }

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null)
            {
                _logger.LogWarning("Password change failed - user not found: {UserId}", userId);
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Password change failed for user {UserId}: {Error}", userId, errorMsg);
                return false;
            }

            // Publish Open Banking event
            await PublishUserPasswordChangedEvent(user, userId, ipAddress, obnClientId);

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string? ipAddress = null, string? obnClientId = null)
        {
            _logger.LogInformation("Processing password reset for email: {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Password reset failed - user not found: {Email}", email);
                return false;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{_frontendBaseUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            var emailBody = $@"
                <p>You requested a password reset.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you did not request this, please ignore this email.</p>
            ";

            await _emailSender.SendEmailAsync(email, "Password Reset", emailBody);

            // Publish Open Banking event
            await PublishUserPasswordResetEvent(user, ipAddress, obnClientId);

            _logger.LogInformation("Password reset email sent to: {Email}", email);
            return true;
        }

        // Event Publishing Methods
        private async Task PublishUserRegisteredEvent(ApplicationUser user, string? obnClientId, string? obnConsentId)
        {
            var @event = new UserRegisteredEvent
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                RegisteredAt = DateTime.UtcNow,
                IsEmailVerified = user.EmailConfirmed,
                ObnClientId = obnClientId,
                ObnConsentId = obnConsentId,
                IsOpenBankingUser = !string.IsNullOrEmpty(obnClientId) || !string.IsNullOrEmpty(obnConsentId)
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published UserRegisteredEvent for User: {UserId}", user.Id);
        }

        private async Task PublishUserLoggedInEvent(ApplicationUser user, string? ipAddress, string? userAgent, string? obnClientId, string? obnConsentId)
        {
            var @event = new UserLoggedInEvent
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                LoggedInAt = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                ObnClientId = obnClientId,
                ObnConsentId = obnConsentId,
                IsOpenBankingLogin = !string.IsNullOrEmpty(obnClientId) || !string.IsNullOrEmpty(obnConsentId)
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published UserLoggedInEvent for User: {UserId}", user.Id);
        }

        private async Task PublishUserLoggedOutEvent(ApplicationUser user, string? ipAddress, string? obnClientId)
        {
            var @event = new UserLoggedOutEvent
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                LoggedOutAt = DateTime.UtcNow,
                IPAddress = ipAddress,
                ObnClientId = obnClientId
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published UserLoggedOutEvent for User: {UserId}", user.Id);
        }

        private async Task PublishUserEmailVerifiedEvent(ApplicationUser user)
        {
            var @event = new UserEmailVerifiedEvent
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                VerifiedAt = DateTime.UtcNow
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published UserEmailVerifiedEvent for User: {UserId}", user.Id);
        }

        private async Task PublishUserAccountLockedEvent(ApplicationUser user, string lockReason, string? ipAddress)
        {
            var @event = new UserAccountLockedEvent
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                LockedAt = DateTime.UtcNow,
                LockReason = lockReason,
                IPAddress = ipAddress
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published UserAccountLockedEvent for User: {UserId}", user.Id);
        }

        private async Task PublishUserPasswordChangedEvent(ApplicationUser user, string changedBy, string? ipAddress, string? obnClientId)
        {
            var @event = new UserPasswordChangedEvent
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = changedBy,
                IPAddress = ipAddress,
                ObnClientId = obnClientId
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published UserPasswordChangedEvent for User: {UserId}", user.Id);
        }

        private async Task PublishUserPasswordResetEvent(ApplicationUser user, string? ipAddress, string? obnClientId)
        {
            var @event = new UserPasswordResetEvent
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                ResetAt = DateTime.UtcNow,
                IPAddress = ipAddress,
                ObnClientId = obnClientId
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published UserPasswordResetEvent for User: {UserId}", user.Id);
        }

        private async Task PublishRefreshTokenUsedEvent(ApplicationUser user, string tokenId, string? ipAddress, string? obnClientId, string? obnConsentId)
        {
            var @event = new RefreshTokenUsedEvent
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                TokenId = tokenId,
                UsedAt = DateTime.UtcNow,
                IPAddress = ipAddress,
                ObnClientId = obnClientId,
                ObnConsentId = obnConsentId
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published RefreshTokenUsedEvent for User: {UserId}", user.Id);
        }

        private async Task PublishRefreshTokenRevokedEvent(ApplicationUser user, string tokenId, string revocationReason, string? revokedBy, string? obnClientId)
        {
            var @event = new RefreshTokenRevokedEvent
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                TokenId = tokenId,
                RevokedAt = DateTime.UtcNow,
                RevocationReason = revocationReason,
                RevokedBy = revokedBy,
                ObnClientId = obnClientId
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published RefreshTokenRevokedEvent for User: {UserId}", user.Id);
        }

        private async Task PublishAuthenticationFailedEvent(string email, string failureReason, string? ipAddress, string? userAgent, int failedAttempts, bool isAccountLocked)
        {
            var @event = new AuthenticationFailedEvent
            {
                Email = email,
                FailureReason = failureReason,
                FailedAt = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                FailedAttempts = failedAttempts,
                IsAccountLocked = isAccountLocked
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published AuthenticationFailedEvent for Email: {Email}", email);
        }
    }
}