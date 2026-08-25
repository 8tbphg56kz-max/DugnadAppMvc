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
        var userManager = serviceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("UserSeeder");

        var options = serviceProvider
            .GetRequiredService<IOptions<DefaultAdminOptions>>()
            .Value;

        // Finn Systemadministrator-brukeren på e-postadressen
        var adminUser = await userManager.FindByEmailAsync(options.Email);

        // Brukeren finnes ikke – opprett den
        if (adminUser == null)
        {
            logger.LogInformation(
                "Systemadministrator-brukeren finnes ikke. Oppretter {Email}.",
                options.Email);

            adminUser = new ApplicationUser
            {
                UserName = options.Email,
                Email = options.Email,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                IsActivated = true,
                ActivatedDate = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(
                adminUser,
                options.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    Environment.NewLine,
                    createResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Kunne ikke opprette Systemadministrator:{Environment.NewLine}{errors}");
            }

            logger.LogInformation(
                "Systemadministrator-brukeren {Email} ble opprettet.",
                options.Email);
        }
        else
        {
            logger.LogInformation(
                "Systemadministrator-brukeren {Email} finnes allerede.",
                options.Email);
        }

        // Sørg for at brukeren har Systemadministrator-rollen
        var harSystemadministratorRolle =
            await userManager.IsInRoleAsync(
                adminUser,
                IdentityRoles.SystemAdministrator);

        if (!harSystemadministratorRolle)
        {
            logger.LogWarning(
                "Brukeren {Email} mangler Systemadministrator-rollen. Tildeler rollen.",
                adminUser.Email);

            var roleResult = await userManager.AddToRoleAsync(
                adminUser,
                IdentityRoles.SystemAdministrator);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    Environment.NewLine,
                    roleResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Kunne ikke tildele Systemadministrator-rollen:{Environment.NewLine}{errors}");
            }

            logger.LogInformation(
                "Systemadministrator-rollen ble tildelt {Email}.",
                adminUser.Email);
        }
        else
        {
            logger.LogInformation(
                "Brukeren {Email} har allerede Systemadministrator-rollen.",
                adminUser.Email);
        }
    }
}