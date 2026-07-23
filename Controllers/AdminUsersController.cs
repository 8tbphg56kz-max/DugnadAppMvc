using DugnadAppMvc.Models;
using DugnadAppMvc.Services.Interfaces;
using DugnadAppMvc.ViewModels.AdminUsers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DugnadAppMvc.Controllers;

[Authorize(Roles = IdentityRoles.AdminAccess)]
public class AdminUsersController : Controller
{
    private readonly IUserAdministrationService _userService;

    public AdminUsersController(
     IUserAdministrationService userService)
    {
        _userService = userService;
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

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Roles = IdentityRoles.All
                .Select(role => new SelectListItem
                {
                    Text = role,
                    Value = role
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
}