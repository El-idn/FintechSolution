using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public bool IsEmailVerified { get; set; }
    }
}
