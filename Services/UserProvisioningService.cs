using DugnadAppMvc.Data;
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

    /// <summary>
    /// Oppretter en Identity-bruker for en beboer hvis den ikke finnes.
    /// </summary>
    /// <summary>
    /// Oppretter en Identity-bruker for en beboer hvis den ikke finnes.
    /// </summary>
    public async Task<UserProvisioningResult> CreateUserAsync(Beboer beboer)
    {
        var existingUser = await _userManager.FindByEmailAsync(beboer.Epost);

        if (existingUser != null)
        {
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

        beboer.ApplicationUserId = user.Id;

        await _context.SaveChangesAsync();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

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