using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using AuthService.Domain.Entities;

namespace AuthService.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Seed roles
        string[] roles = { "Admin", "Customer", "Auditor" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole(role));
        }

        // 2. Seed default admin user
        var adminEmail = "admin@openbank.test";
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123@Secure");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");

                // 3. Add permissions as claims
                var claims = new[]
                {
                    new Claim("Permission", "CreateAccount"),
                    new Claim("Permission", "ViewAuditLogs"),
                    new Claim("Permission", "AccessPII")
                };

                foreach (var claim in claims)
                {
                    if (!(await userManager.GetClaimsAsync(admin)).Any(c => c.Type == claim.Type && c.Value == claim.Value))
                        await userManager.AddClaimAsync(admin, claim);
                }
            }
        }
    }
}
