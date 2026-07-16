using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    [Authorize]
    public class DugnaderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DugnaderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dugnader = await _context.Dugnader
    .OrderByDescending(d => d.StartDato)
    .ThenByDescending(d => d.Id)
    .ToListAsync();

            return View(dugnader);
        }
    
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Dugnad dugnad)
        {
            if (!ModelState.IsValid)
                return View(dugnad);

            _context.Dugnader.Add(dugnad);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dugnad = await _context.Dugnader.FindAsync(id);

            if (dugnad == null)
            {
                return NotFound();
            }

            return View(dugnad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Dugnad dugnad)
        {
            if (id != dugnad.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(dugnad);
            }

            try
            {
                _context.Update(dugnad);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Dugnaden ble oppdatert.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Dugnader.Any(d => d.Id == dugnad.Id))
                {
                    return NotFound();
                }

                throw;
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dugnad = await _context.Dugnader
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dugnad == null)
            {
                return NotFound();
            }

            var harTimer = await _context.Dugnadstimer
                .AnyAsync(t => t.DugnadId == id);

            if (harTimer)
            {
                TempData["ErrorMessage"] =
                    "Dugnaden kan ikke slettes fordi det finnes registrerte dugnadstimer.";

                return RedirectToAction(nameof(Index));
            }

            _context.Dugnader.Remove(dugnad);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Dugnaden ble slettet.";

            return RedirectToAction(nameof(Index));
        }
    }
}
    