using DugnadAppMvc.Configuration;
using DugnadAppMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DugnadAppMvc.Infrastructure.Identity;

public static class UserSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("UserSeeder");

        var options = serviceProvider
            .GetRequiredService<IOptions<DefaultAdminOptions>>()
            .Value;

        // Finnes det allerede en systemadministrator?
        var admins = await userManager.GetUsersInRoleAsync(
            IdentityRoles.SystemAdministrator);

        if (admins.Any())
        {
            logger.LogInformation("SystemAdministrator finnes allerede.");
            return;
        }

        logger.LogInformation("Oppretter første SystemAdministrator...");

        var adminUser = new ApplicationUser
        {
            UserName = options.Email,
            Email = options.Email,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            IsActivated = true,
            ActivatedDate = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, options.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(Environment.NewLine,
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(errors);
        }

        var roleResult = await userManager.AddToRoleAsync(
            adminUser,
            IdentityRoles.SystemAdministrator);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(Environment.NewLine,
                roleResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(errors);
        }

        logger.LogInformation("SystemAdministrator opprettet.");
    }
}