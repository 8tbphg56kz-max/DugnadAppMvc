using DugnadAppMvc.Models;
using DugnadAppMvc.Services;
using DugnadAppMvc.Services.Interfaces;
using DugnadAppMvc.ViewModels;
using DugnadAppMvc.ViewModels.AdminUsers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers;

[Authorize(Roles = IdentityRoles.AdminAccess)]
public class AdminUsersController : Controller
{
    private readonly IUserAdministrationService _userService;

    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUsersController(
    IUserAdministrationService userService,
    UserManager<ApplicationUser> userManager)
    {
        _userService = userService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _userService.GetUsersAsync();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var model = await _userService.GetUserAsync(id);

        if (model == null)
            return NotFound();

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserRoleViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);

        if (user == null)
            return NotFound();
               

        if (!ModelState.IsValid)
        {
            model.Roles = IdentityRoles.All
                .Where(r => r != IdentityRoles.Beboer)
                .Select(r => new SelectListItem
                {
                    Text = r,
                    Value = r
                })
                .ToList();

            return View(model);
        }

        var result = await _userService.UpdateRoleAsync(model);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;

            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        TempData["SuccessMessage"] = "Rollen er oppdatert.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Add()
    {
        var alleBrukere = await _userManager.Users
        .OrderBy(u => u.LastName)
        .ThenBy(u => u.FirstName)
        .ToListAsync();

        var beboere = new List<SelectListItem>();

        foreach (var user in alleBrukere)
        {
            var roller = await _userManager.GetRolesAsync(user);

            if (roller.Contains(IdentityRoles.Beboer))
            {
                beboere.Add(new SelectListItem
                {
                    Value = user.Id,
                    Text = $"{user.FirstName} {user.LastName}"
                });
            }
        }

        ViewBag.Beboere = beboere;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GiTilgang(LeggTilTilgangViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Add));
        }

        var user = await _userManager.FindByIdAsync(model.UserId);

        if (user == null)
        {
            return NotFound();
        }

        if (model.Rolle == IdentityRoles.SystemAdministrator &&
    !User.IsInRole(IdentityRoles.SystemAdministrator))
        {
            TempData["Error"] =
                "Kun systemadministrator kan gi systemadministrator-tilgang.";

            return RedirectToAction(nameof(Index));
        }

        if (model.Rolle != IdentityRoles.Styremedlem &&
     model.Rolle != IdentityRoles.Administrator &&
     model.Rolle != IdentityRoles.SystemAdministrator)
        {
            return BadRequest();
        }

        if (await _userManager.IsInRoleAsync(user, model.Rolle))
        {
            TempData["Error"] =
                $"{user.FirstName} {user.LastName} har allerede rollen {model.Rolle}.";

            return RedirectToAction(nameof(Index));
        }

        await _userManager.RemoveFromRoleAsync(user, IdentityRoles.Beboer);
        await _userManager.AddToRoleAsync(user, model.Rolle);

        TempData["Success"] =
            $"{user.FirstName} {user.LastName} er nå {model.Rolle}.";

        return RedirectToAction(nameof(Index));

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = IdentityRoles.SystemAdministrator)]
    public async Task<IActionResult> FjernTilgang(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        if (user.Email == "admin@dugnadapp.no")
        {
            TempData["Error"] =
                "Systemadministrator kan ikke gjøres om til beboer.";

            return RedirectToAction(nameof(Index));
        }

        // Fjern alle utvidede roller
        var roller = await _userManager.GetRolesAsync(user);

        foreach (var rolle in roller)
        {
            if (rolle != IdentityRoles.Beboer)
            {
                await _userManager.RemoveFromRoleAsync(user, rolle);
            }
        }

        // Sørg for at brukeren har Beboer-rollen
        if (!await _userManager.IsInRoleAsync(user, IdentityRoles.Beboer))
        {
            await _userManager.AddToRoleAsync(user, IdentityRoles.Beboer);
        }

        TempData["Success"] =
            $"{user.FirstName} {user.LastName} er nå vanlig beboer.";

        return RedirectToAction(nameof(Index));
    }

}