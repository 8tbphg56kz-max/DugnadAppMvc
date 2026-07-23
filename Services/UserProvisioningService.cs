using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using Microsoft.AspNetCore.Identity;

namespace DugnadAppMvc.Services;

public class UserProvisioningService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UserProvisioningService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<UserProvisioningResult> CreateUserAsync(Beboer beboer)
    {
        var existingUser = await _userManager.FindByEmailAsync(beboer.Epost);

        if (existingUser != null)
        {
            // Koble beboeren til eksisterende Identity-bruker dersom nødvendig
            if (beboer.ApplicationUserId != existingUser.Id)
            {
                beboer.ApplicationUserId = existingUser.Id;
                await _context.SaveChangesAsync();
            }

            // Sørg for at brukeren alltid har Beboer-rollen
            if (!await _userManager.IsInRoleAsync(existingUser, IdentityRoles.Beboer))
            {
                await _userManager.AddToRoleAsync(existingUser, IdentityRoles.Beboer);
            }

            return new UserProvisioningResult
            {
                User = existingUser,
                IsNewUser = false
            };
        }

        var user = new ApplicationUser
        {
            UserName = beboer.Epost,
            Email = beboer.Epost,
            FirstName = beboer.Fornavn,
            LastName = beboer.Etternavn,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(Environment.NewLine,
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(errors);
        }

        // Alle nye brukere er beboere
        await _userManager.AddToRoleAsync(user, IdentityRoles.Beboer);

        // Koble Identity-brukeren til beboeren
        beboer.ApplicationUserId = user.Id;

        await _context.SaveChangesAsync();

        return new UserProvisioningResult
        {
            User = user,
            IsNewUser = true
        };
    }

    public async Task<string> GenerateActivationTokenAsync(ApplicationUser user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }
}