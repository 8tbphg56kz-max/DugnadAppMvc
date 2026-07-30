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

        // POST: BOARDMESSAGES/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tittel,Innhold,PublisertDato")] BoardMessage boardmessage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(boardmessage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(boardmessage);
        }

        // GET: BOARDMESSAGES/Edit/5
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

        // POST: BOARDMESSAGES/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                try
                {
                    _context.Update(boardmessage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BoardMessageExists(boardmessage.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
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
