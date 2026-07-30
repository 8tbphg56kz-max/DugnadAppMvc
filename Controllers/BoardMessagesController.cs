using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DugnadAppMvc.Controllers
{
    public class BoardMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BoardMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BOARDMESSAGES
        public async Task<IActionResult> Index()
        {
            return View(await _context.BoardMessages.ToListAsync());
        }

        // GET: BOARDMESSAGES/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boardmessage = await _context.BoardMessages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (boardmessage == null)
            {
                return NotFound();
            }

            return View(boardmessage);
        }

        // GET: BOARDMESSAGES/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tittel,Innhold,PublisertDato")] BoardMessage boardmessage)
        {
            if (ModelState.IsValid)
            {
                boardmessage.PublisertDato = DateTime.UtcNow;

                _context.Add(boardmessage);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(boardmessage);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boardmessage = await _context.BoardMessages.FindAsync(id);
            if (boardmessage == null)
            {
                return NotFound();
            }
            return View(boardmessage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Tittel,Innhold,PublisertDato")] BoardMessage boardmessage)
        {
            if (id != boardmessage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.BoardMessages.FindAsync(id);

                if (existing == null)
                {
                    return NotFound();
                }

                existing.Tittel = boardmessage.Tittel;
                existing.Innhold = boardmessage.Innhold;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(boardmessage);
        }

        // GET: BOARDMESSAGES/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boardmessage = await _context.BoardMessages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (boardmessage == null)
            {
                return NotFound();
            }

            return View(boardmessage);
        }

        // POST: BOARDMESSAGES/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var boardmessage = await _context.BoardMessages.FindAsync(id);
            if (boardmessage != null)
            {
                _context.BoardMessages.Remove(boardmessage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BoardMessageExists(int? id)
        {
            return _context.BoardMessages.Any(e => e.Id == id);
        }
    }
}
