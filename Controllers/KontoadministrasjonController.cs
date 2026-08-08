using DugnadAppMvc.Models;
using DugnadAppMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

[Authorize(Roles = IdentityRoles.AdminAccess)]
public class KontoadministrasjonController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailService _emailService;

    public KontoadministrasjonController(
    UserManager<ApplicationUser> userManager,
    EmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<IActionResult> Index()
    {
        var ventendeKontoer = await _userManager.Users
    .Where(u => !u.IsActivated)
.OrderBy(u => u.LastName)
.ThenBy(u => u.FirstName)
.ToListAsync();

        var model = new KontoadministrasjonViewModel
        {
            VentendeKontoer = ventendeKontoer
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aktiver(string id)
    {
        var bruker = await _userManager.FindByIdAsync(id);

        if (bruker == null)
        {
            return NotFound();
        }

        // Brukes kun dersom en beboer ikke får aktivert kontoen via e-post.
        bruker.IsActivated = true;
        bruker.ActivatedDate = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(bruker);

        if (!result.Succeeded)
        {
            TempData["Error"] = "Kunne ikke aktivere brukeren.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.AddToRoleAsync(bruker, IdentityRoles.Beboer);

        TempData["Success"] = $"{bruker.FirstName} {bruker.LastName} er aktivert.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendNyAktiveringslenke(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var activationLink = await LagAktiveringslenkeAsync(user);

        await _emailService.SendActivationEmailAsync(
            user.Email!,
            activationLink);

        TempData["Success"] =
            $"Ny aktiveringslenke er sendt til {user.Email}.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<string> LagAktiveringslenkeAsync(ApplicationUser user)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        token = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token));

        return Url.Action(
            "Activate",
            "Account",
            new
            {
                userId = user.Id,
                token
            },
            protocol: Request.Scheme)!;
    }
}