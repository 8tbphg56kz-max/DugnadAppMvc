using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers

{
    [Authorize(Roles = IdentityRoles.BoardAccess)]
    public class AdministrasjonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdministrasjonController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var budsjett = await _context.Innstillinger
                .Select(i => i.Dugnadsbudsjett)
                .FirstOrDefaultAsync();

            var registrerteTimer = await _context.Timeforinger
    .Select(t => (decimal?)t.AntallTimer)
    .SumAsync() ?? 0;

            var model = new AdminDashboardViewModel
            {
                AntallBeboere = await _context.Beboere.CountAsync(),

                AntallAktiveDugnader =
    await _context.Dugnader.CountAsync(d => !d.ErUtført && d.ErSynlig)
    + await _context.Oppgaver.CountAsync(o => !o.ErUtført),

                AntallUtforteDugnader =
    await _context.Dugnader.CountAsync(d => d.ErUtført)
    + await _context.Oppgaver.CountAsync(o => o.ErUtført),

                RegistrerteTimer = registrerteTimer,

                Dugnadsbudsjett = budsjett,

                ForelopigTimepris = registrerteTimer > 0
        ? (decimal)budsjett / registrerteTimer
        : 0,

                KommendeDugnader = await _context.Dugnader
        .Where(d => d.ErSynlig &&
                    d.StartDato >= DateOnly.FromDateTime(DateTime.Today))
        .OrderBy(d => d.StartDato)
        .Take(5)
        .Select(d => new KommendeDugnadViewModel
        {
            Id = d.Id,
            Tittel = d.Tittel,
            StartDato = d.StartDato
        })
        .ToListAsync()
            };

            return View(model);
        }
    }
}