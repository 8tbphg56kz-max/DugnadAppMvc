using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DugnadAppMvc.Data;
using DugnadAppMvc.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    //[Authorize(Roles = "Administrator")]
    public class AdminRapporterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TimerPrLeilighet()
        {
            var rapport = await _context.Dugnadstimer
                .Include(d => d.Beboer)
                    .ThenInclude(b => b.Leilighet)
                .GroupBy(d => new
                {
                    d.Beboer.Leilighet.Id,
                    d.Beboer.Leilighet.Leilighetsnummer
                })
                .Select(g => new RapportTimerPrLeilighetViewModel
                {
                    LeilighetId = g.Key.Id,
                    Leilighetsnummer = g.Key.Leilighetsnummer,
                    AntallRegistreringer = g.Count(),
                    TotaleTimer = g.Sum(x => x.Timer)
                })
                .OrderBy(x => x.Leilighetsnummer)
                .ToListAsync();

            return View(rapport);
        }

        public IActionResult TimerPrBeboer()
        {
            return View();
        }

        public IActionResult TimerPrDugnad()
        {
            return View();
        }

        private readonly ApplicationDbContext _context;

        public AdminRapporterController(ApplicationDbContext context)
        {
            _context = context;
        }        
    }
}