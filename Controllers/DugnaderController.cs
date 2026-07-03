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
    }
}
    