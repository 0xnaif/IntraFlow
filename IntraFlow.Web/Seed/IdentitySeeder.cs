using IntraFlow.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace IntraFlow.Web.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = ["Employee", "Approver", "Admin"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        
        var adminEmail = "admin@intraflow.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Admin"
            };

            var createResult = await userManager.CreateAsync(admin, "Admin123!");
            
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
