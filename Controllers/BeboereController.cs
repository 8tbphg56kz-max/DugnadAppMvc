using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    [Authorize]
    public class BeboereController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BeboereController(ApplicationDbContext context)
        {
            _context = context;
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

            return RedirectToAction(nameof(Index));

            _context.Beboere.Add(beboer);
            await _context.SaveChangesAsync();

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

            beboer.Aktiv = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}