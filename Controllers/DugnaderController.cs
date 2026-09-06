using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using DugnadAppMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    [Authorize(Roles = IdentityRoles.BoardAccess)]
    public class DugnaderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DugnadBildeService _bildeService;

        public DugnaderController(
            ApplicationDbContext context,
            DugnadBildeService bildeService)
        {
            _context = context;
            _bildeService = bildeService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var dugnader = await _context.Dugnader
                .Include(d => d.Bilder)
                .OrderByDescending(d => d.StartDato)
                .ThenByDescending(d => d.Id)
                .ToListAsync();

            return View(dugnader);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Dugnad
            {
                StartDato = DateOnly.FromDateTime(DateTime.Today),
                SluttDato = DateOnly.FromDateTime(DateTime.Today),
                ErSynlig = true
            });
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Dugnad dugnad,
            List<IFormFile>? bilder)
        {
            if (!ModelState.IsValid)
                return View(dugnad);

            _context.Dugnader.Add(dugnad);
            await _context.SaveChangesAsync();

            try
            {
                var nyeBilder = await _bildeService.LagreBilderAsync(
                    dugnad.Id,
                    bilder);

                if (nyeBilder.Count > 0)
                {
                    _context.DugnadBilder.AddRange(nyeBilder);
                    await _context.SaveChangesAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dugnad = await _context.Dugnader
                .Include(d => d.Bilder)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dugnad == null)
            {
                return NotFound();
            }

            return View(dugnad);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Dugnad dugnad,
            List<IFormFile>? bilder)
        {
            if (id != dugnad.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                dugnad.Bilder = await _context.DugnadBilder
                    .Where(b => b.DugnadId == id)
                    .ToListAsync();

                return View(dugnad);
            }

            try
            {
                var eksisterendeBilder = await _context.DugnadBilder
                    .Where(b => b.DugnadId == id)
                    .CountAsync();

                var antallNyeBilder = bilder?
                    .Count(f => f != null && f.Length > 0)
                    ?? 0;

                if (eksisterendeBilder + antallNyeBilder > 5)
                {
                    TempData["Error"] =
                        "Du kan ha maksimalt 5 bilder per fellesdugnad.";

                    dugnad.Bilder = await _context.DugnadBilder
                        .Where(b => b.DugnadId == id)
                        .ToListAsync();

                    return View(dugnad);
                }

                _context.Update(dugnad);
                await _context.SaveChangesAsync();

                var nyeBilder = await _bildeService.LagreBilderAsync(
                    dugnad.Id,
                    bilder);

                if (nyeBilder.Count > 0)
                {
                    _context.DugnadBilder.AddRange(nyeBilder);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] =
                    "Dugnaden ble oppdatert.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Dugnader.Any(d => d.Id == dugnad.Id))
                {
                    return NotFound();
                }

                throw;
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisible(int id)
        {
            var dugnad = await _context.Dugnader.FindAsync(id);

            if (dugnad == null)
            {
                return NotFound();
            }

            dugnad.ErSynlig = !dugnad.ErSynlig;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = dugnad.ErSynlig
                ? "Dugnaden er nå synlig."
                : "Dugnaden er nå skjult.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Bilde(int id)
        {
            var bilde = await _context.DugnadBilder
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bilde == null)
            {
                return NotFound();
            }

            var filbane = _bildeService.HentFilbane(bilde.Filnavn);

            if (!System.IO.File.Exists(filbane))
            {
                return NotFound();
            }

            var utvidelse = Path.GetExtension(bilde.Filnavn)
                .ToLowerInvariant();

            var contentType = utvidelse switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            return PhysicalFile(filbane, contentType);
        }

        [Authorize(Roles = IdentityRoles.BoardAccess)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SlettBilde(int id)
        {
            var bilde = await _context.DugnadBilder
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bilde == null)
            {
                return NotFound();
            }

            var dugnadId = bilde.DugnadId;

            _bildeService.SlettBilde(bilde);

            _context.DugnadBilder.Remove(bilde);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Bildet ble slettet.";

            return RedirectToAction(nameof(Edit), new { id = dugnadId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var dugnad = await _context.Dugnader
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dugnad == null)
            {
                return NotFound();
            }

            return View(dugnad);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dugnad = await _context.Dugnader
                .Include(d => d.Bilder)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dugnad == null)
            {
                return NotFound();
            }

            try
            {
                // Slett bildefilene fysisk fra Synology
                if (dugnad.Bilder != null)
                {
                    foreach (var bilde in dugnad.Bilder)
                    {
                        _bildeService.SlettBilde(bilde);
                    }
                }

                _context.Dugnader.Remove(dugnad);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Dugnaden ble slettet.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Dugnaden kan ikke slettes fordi det finnes påmeldinger på den.";

                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}