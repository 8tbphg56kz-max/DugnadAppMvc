using Microsoft.AspNetCore.Identity;

namespace DugnadAppMvc.Infrastructure.Identity;

public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles =
        {
            IdentityRoles.SystemAdministrator,
            IdentityRoles.Administrator,
            IdentityRoles.Styremedlem,
            IdentityRoles.Beboer
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}