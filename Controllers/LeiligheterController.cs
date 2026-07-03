using DugnadAppMvc.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DugnadAppMvc.Models;

namespace DugnadAppMvc.Controllers
{
    [Authorize]
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
                .OrderBy(l => l.Seksjonsnummer)
                .ToListAsync();

            return View(leiligheter);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var leilighet = await _context.Leiligheter.FindAsync(id);

            if (leilighet == null)
                return NotFound();

            return View(leilighet);
        }

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
                ModelState.AddModelError("", "Leiligheten kan ikke slettes fordi den har registrerte beboere.");
                return View(leilighet);
            }

            _context.Leiligheter.Remove(leilighet);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}