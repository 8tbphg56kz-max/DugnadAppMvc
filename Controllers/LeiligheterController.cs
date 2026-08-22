using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    [Authorize(Roles = IdentityRoles.BoardAccess)]
    public class LeiligheterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeiligheterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var leiligheter = await _context.Leiligheter
                .Include(l => l.Beboere)
                .OrderBy(l => l.Leilighetsnummer)
                .ToListAsync();

            return View(leiligheter);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Leilighet leilighet)
        {
            if (!ModelState.IsValid)
                return View(leilighet);

            _context.Leiligheter.Add(leilighet);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var leilighet = await _context.Leiligheter.FindAsync(id);

            if (leilighet == null)
                return NotFound();

            return View(leilighet);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Leilighet leilighet)
        {
            if (id != leilighet.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(leilighet);

            _context.Update(leilighet);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var leilighet = await _context.Leiligheter
                .Include(l => l.Beboere)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leilighet == null)
                return NotFound();

            return View(leilighet);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leilighet = await _context.Leiligheter
                .Include(l => l.Beboere)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leilighet == null)
                return NotFound();

            if (leilighet.Beboere.Any())
            {
                TempData["ErrorMessage"] =
                    "Leiligheten kan ikke slettes fordi den har registrerte beboere.";

                return RedirectToAction(nameof(Index));
            }

            _context.Leiligheter.Remove(leilighet);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}