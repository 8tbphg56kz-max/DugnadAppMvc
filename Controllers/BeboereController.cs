using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using DugnadAppMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    [Authorize]
    public class BeboereController : Controller
    {
        private readonly ApplicationDbContext _context;
   
        private readonly UserProvisioningService _userProvisioningService;

        private readonly EmailService _emailService;

        private readonly UserManager<ApplicationUser> _userManager;

        public BeboereController(
    ApplicationDbContext context,
    UserProvisioningService userProvisioningService,
    EmailService emailService,
    UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userProvisioningService = userProvisioningService;
            _emailService = emailService;
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

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Leiligheter = _context.Leiligheter
                .OrderBy(l => l.Seksjonsnummer)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Beboer beboer)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Leiligheter = _context.Leiligheter
                    .OrderBy(l => l.Seksjonsnummer)
                    .ToList();

                return View(beboer);
            }

            _context.Beboere.Add(beboer);
            await _context.SaveChangesAsync();

            await _userProvisioningService.CreateUserAsync(beboer);

            TempData["SuccessMessage"] =
    $"Beboeren {beboer.Fornavn} {beboer.Etternavn} er opprettet.";

            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var beboer = await _context.Beboere.FindAsync(id);

            if (beboer == null)
                return NotFound();

            ViewBag.Leiligheter = _context.Leiligheter
                .OrderBy(l => l.Seksjonsnummer)
                .ToList();

            return View(beboer);
        }

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

        [HttpGet]
        public async Task<IActionResult> Deactivate(int id)
        {
            var beboer = await _context.Beboere
                .Include(b => b.Leilighet)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (beboer == null)
                return NotFound();

            return View(beboer);
        }

        [HttpPost, ActionName("Deactivate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateConfirmed(int id)
        {
            var beboer = await _context.Beboere.FindAsync(id);

            if (beboer == null)
                return NotFound();

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

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

        [HttpPost]
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
    }
}