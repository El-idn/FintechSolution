// AuthService/Domain/Entities/EmailVerificationToken.cs
namespace AuthService.Domain.Entities
{
    public class EmailVerificationToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Token { get; set; } = default!;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}
