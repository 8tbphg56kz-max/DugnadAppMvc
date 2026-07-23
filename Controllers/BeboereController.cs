using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using DugnadAppMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    [Authorize(Roles = IdentityRoles.BoardAccess)]
      public class BeboereController : Controller
    {
        private readonly ApplicationDbContext _context;
   
        private readonly UserProvisioningService _userProvisioningService;


        private readonly UserManager<ApplicationUser> _userManager;

        public BeboereController(
    ApplicationDbContext context,
    UserProvisioningService userProvisioningService,
    UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userProvisioningService = userProvisioningService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var beboere = await _context.Beboere
                .Include(b => b.Leilighet)
                .OrderBy(b => b.Etternavn)
                .ThenBy(b => b.Fornavn)
                .ToListAsync();

            return View(beboere);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpGet]
        public IActionResult Create()
        {
            FyllLeiligheter();

            return View();
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Beboer beboer)
        {
            if (!ModelState.IsValid)
            {
                FyllLeiligheter();
                return View(beboer);
            }

            _context.Beboere.Add(beboer);
            await _context.SaveChangesAsync();

            await _userProvisioningService.CreateUserAsync(beboer);

            TempData["SuccessMessage"] =
                $"Beboeren {beboer.Fornavn} {beboer.Etternavn} er opprettet.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var beboer = await _context.Beboere.FindAsync(id);

            if (beboer == null)
            {
                return NotFound();
            }

            FyllLeiligheter();

            return View(beboer);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Beboer beboer)
        {
            if (id != beboer.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Leiligheter = _context.Leiligheter
                    .OrderBy(l => l.Seksjonsnummer)
                    .ToList();

                return View(beboer);
            }

            _context.Update(beboer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var beboer = await _context.Beboere
                .Include(b => b.Leilighet)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (beboer == null)
            {
                return NotFound();
            }

            return View(beboer);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var beboer = await _context.Beboere
                .Include(b => b.ApplicationUser)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (beboer == null)
            {
                return NotFound();
            }

            var harTimer = await _context.Dugnadstimer
                .AnyAsync(t => t.BeboerId == id);

            if (harTimer)
            {
                TempData["ErrorMessage"] =
                    "Beboeren kan ikke slettes fordi det finnes registrerte dugnadstimer. Disse må slettes først.";

                return RedirectToAction(nameof(Index));
            }

            if (beboer.ApplicationUser != null)
            {
                var result = await _userManager.DeleteAsync(beboer.ApplicationUser);

                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] =
                        "Kunne ikke slette innloggingskontoen.";

                    return RedirectToAction(nameof(Index));
                }
            }

            _context.Beboere.Remove(beboer);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Beboeren ble slettet.";

            return RedirectToAction(nameof(Index));
        }

        private void FyllLeiligheter()
        {
            ViewBag.Leiligheter = _context.Leiligheter
                .OrderBy(l => l.Seksjonsnummer)
                .ToList();
        }
    }
}