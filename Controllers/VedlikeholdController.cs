using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers;

[Authorize(Roles = IdentityRoles.SystemAdministrator)]
public class VedlikeholdController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserProvisioningService _userProvisioningService;

    public VedlikeholdController(
        ApplicationDbContext context,
        UserProvisioningService userProvisioningService)
    {
        _context = context;
        _userProvisioningService = userProvisioningService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KobleEksisterendeBeboere()
    {
        var beboere = await _context.Beboere
            .Where(b => b.ApplicationUserId == null)
            .ToListAsync();

        int antall = 0;

        foreach (var beboer in beboere)
        {
            await _userProvisioningService.CreateUserAsync(beboer);
            antall++;
        }

        TempData["SuccessMessage"] =
            $"{antall} beboere ble kontrollert og koblet til Identity.";

        return RedirectToAction(nameof(Index));
    }
}