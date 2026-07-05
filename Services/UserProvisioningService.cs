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
    public async Task<ApplicationUser> CreateUserAsync(Beboer beboer)
    {
        // Finnes brukeren allerede?
        var user = await _userManager.FindByEmailAsync(beboer.Epost);

        if (user != null)
            return user;

        user = new ApplicationUser
        {
            UserName = beboer.Epost,
            Email = beboer.Epost,
            FirstName = beboer.Fornavn,
            LastName = beboer.Etternavn,
            EmailConfirmed = false
        };

        // Opprettes uten passord.
        var result = await _userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(Environment.NewLine,
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(errors);
        }

        beboer.ApplicationUserId = user.Id;

        await _context.SaveChangesAsync();

        return user;
    }
}