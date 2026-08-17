using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Services;
using DugnadAppMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers;

[Authorize(Roles = IdentityRoles.SystemAdministrator)]
public class VedlikeholdController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserProvisioningService _userProvisioningService;
    private readonly IDatabaseRestoreService _databaseRestoreService;

    public VedlikeholdController(
        ApplicationDbContext context,
        UserProvisioningService userProvisioningService,
        IDatabaseRestoreService databaseRestoreService)
    {
        _context = context;
        _userProvisioningService = userProvisioningService;
        _databaseRestoreService = databaseRestoreService;
    }

    public async Task<IActionResult> Index()
    {
        var backups = await _databaseRestoreService.GetBackupsAsync();

        return View(backups);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(string backupFile)
    {
        try
        {
            await _databaseRestoreService.StartRestoreAsync(backupFile);

            TempData["SuccessMessage"] =
                $"Restore av {backupFile} er startet. DugnadApp vil være utilgjengelig mens gjenopprettingen pågår.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] =
                $"Restore kunne ikke startes: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}