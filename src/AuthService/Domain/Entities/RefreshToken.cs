namespace AuthService.Domain.Entities
{
    // public class RefreshToken
    // {
    //     public int Id { get; set; }
    //     public string Token { get; set; } = string.Empty;
    //     public DateTime Expires { get; set; }
    //     public DateTime Created { get; set; }
    //     public bool IsRevoked { get; set; } = false;
    // }

    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public bool IsExpired => DateTime.UtcNow > Expires;
    }
}
