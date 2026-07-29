namespace SharedKernel.Events
{
    /// <summary>
    /// Event published when a new user registers
    /// </summary>
    public class UserRegisteredEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public bool IsEmailVerified { get; set; }
        public string? ObnClientId { get; set; } // Third Party Provider ID for Open Banking
        public string? ObnConsentId { get; set; }
        public bool IsOpenBankingUser { get; set; }
    }

    /// <summary>
    /// Event published when a user successfully logs in
    /// </summary>
    public class UserLoggedInEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime LoggedInAt { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
        public bool IsOpenBankingLogin { get; set; }
    }

    /// <summary>
    /// Event published when a user logs out
    /// </summary>
    public class UserLoggedOutEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime LoggedOutAt { get; set; }
        public string? IPAddress { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
    }

    /// <summary>
    /// Event published when a user's email is verified
    /// </summary>
    public class UserEmailVerifiedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime VerifiedAt { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
    }

    /// <summary>
    /// Event published when a user's account is locked
    /// </summary>
    public class UserAccountLockedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime LockedAt { get; set; }
        public string LockReason { get; set; } = string.Empty;
        public string? IPAddress { get; set; }
        public string? ObnClientId { get; set; }
    }

    /// <summary>
    /// Event published when a user's account is unlocked
    /// </summary>
    public class UserAccountUnlockedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime UnlockedAt { get; set; }
        public string? UnlockedBy { get; set; }
        public string? ObnClientId { get; set; }
    }

    /// <summary>
    /// Event published when a user's password is changed
    /// </summary>
    public class UserPasswordChangedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public string? IPAddress { get; set; }
        public string? ObnClientId { get; set; }
    }

    /// <summary>
    /// Event published when a user's password is reset
    /// </summary>
    public class UserPasswordResetEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime ResetAt { get; set; }
        public string? IPAddress { get; set; }
        public string? ObnClientId { get; set; }
    }

    /// <summary>
    /// Event published when a refresh token is used
    /// </summary>
    public class RefreshTokenUsedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public DateTime UsedAt { get; set; }
        public string? IPAddress { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnConsentId { get; set; }
    }

    /// <summary>
    /// Event published when a refresh token is revoked
    /// </summary>
    public class RefreshTokenRevokedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public DateTime RevokedAt { get; set; }
        public string RevocationReason { get; set; } = string.Empty;
        public string? RevokedBy { get; set; }
        public string? ObnClientId { get; set; }
    }

    /// <summary>
    /// Event published for Open Banking Nigeria Strong Customer Authentication (SCA) initiation
    /// </summary>
    public class ObnSCAInitiatedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string ObnConsentId { get; set; } = string.Empty;
        public string ObnClientId { get; set; } = string.Empty;
        public string ObnClientName { get; set; } = string.Empty;
        public string SCAMethod { get; set; } = string.Empty; // SMS, EMAIL, APP, etc.
        public string TransactionType { get; set; } = string.Empty; // PAYMENT, CONSENT, etc.
        public DateTime InitiatedAt { get; set; }
        public string? RedirectUrl { get; set; }
        public string? ChallengeData { get; set; }
    }

    /// <summary>
    /// Event published for Open Banking Nigeria Strong Customer Authentication (SCA) completion
    /// </summary>
    public class ObnSCACompletedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string ObnConsentId { get; set; } = string.Empty;
        public string ObnClientId { get; set; } = string.Empty;
        public string SCAMethod { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public string AuthorizationStatus { get; set; } = string.Empty; // APPROVED, REJECTED, TIMEOUT
        public DateTime CompletedAt { get; set; }
        public string? AuthorizationCode { get; set; }
        public string? FailureReason { get; set; }
    }

    /// <summary>
    /// Event published when a user's consent is granted for Open Banking Nigeria
    /// </summary>
    public class UserConsentGrantedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string ObnConsentId { get; set; } = string.Empty;
        public string ObnClientId { get; set; } = string.Empty;
        public string ObnClientName { get; set; } = string.Empty;
        public string[] Permissions { get; set; } = Array.Empty<string>();
        public string[] AccountIds { get; set; } = Array.Empty<string>();
        public DateTime GrantedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? SCAMethod { get; set; }
    }

    /// <summary>
    /// Event published when a user's consent is revoked for Open Banking Nigeria
    /// </summary>
    public class UserConsentRevokedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string ObnConsentId { get; set; } = string.Empty;
        public string ObnClientId { get; set; } = string.Empty;
        public string RevocationReason { get; set; } = string.Empty;
        public DateTime RevokedAt { get; set; }
        public string? RevokedBy { get; set; }
    }

    /// <summary>
    /// Event published when a user's consent expires for Open Banking Nigeria
    /// </summary>
    public class UserConsentExpiredEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string ObnConsentId { get; set; } = string.Empty;
        public string ObnClientId { get; set; } = string.Empty;
        public DateTime ExpiredAt { get; set; }
        public DateTime OriginalExpiryDate { get; set; }
    }

    /// <summary>
    /// Event published for failed authentication attempts
    /// </summary>
    public class AuthenticationFailedEvent
    {
        public string Email { get; set; } = string.Empty;
        public string FailureReason { get; set; } = string.Empty;
        public DateTime FailedAt { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ObnClientId { get; set; }
        public int FailedAttempts { get; set; }
        public bool IsAccountLocked { get; set; }
    }

    /// <summary>
    /// Event published for suspicious activity detection
    /// </summary>
    public class SuspiciousActivityDetectedEvent
    {
        public Guid? UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty; // LOGIN_ATTEMPT, PASSWORD_RESET, etc.
        public string RiskLevel { get; set; } = string.Empty; // LOW, MEDIUM, HIGH, CRITICAL
        public string Description { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ObnClientId { get; set; }
        public bool RequiresManualReview { get; set; }
    }
} 