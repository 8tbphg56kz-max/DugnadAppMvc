using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    [Authorize(Roles = $"{IdentityRoles.Styremedlem},{IdentityRoles.Administrator},{IdentityRoles.SystemAdministrator}")]
    public class AdminRapporterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TimerPrLeilighet()
        {
            var innstillinger = await _context.Innstillinger.FirstAsync();

            var totaleTimerAlle = await _context.Timeforinger.SumAsync(x => x.AntallTimer);

            var timeverdi = totaleTimerAlle == 0
                ? 0
                : (decimal)innstillinger.Dugnadsbudsjett / totaleTimerAlle;

            var rapport = await _context.Timeforinger
    .Include(d => d.Beboer)
        .ThenInclude(b => b.Leilighet)
   .GroupBy(d => new
   {
       d.Beboer.Leilighet.Id,
       d.Beboer.Leilighet.Seksjonsnummer,
       d.Beboer.Leilighet.Leilighetsnummer
   })
    .Select(g => new RapportTimerPrLeilighetViewModel
    {
        LeilighetId = g.Key.Id,

        Visningsnavn = $"Seksjon {g.Key.Seksjonsnummer} - {g.Key.Leilighetsnummer}",

        Leilighetsnummer = g.Key.Leilighetsnummer,

        AntallRegistreringer = g.Count(),

        TotaleTimer = g.Sum(x => x.AntallTimer)
    })
.OrderBy(x => x.Leilighetsnummer)
.ToListAsync();

            foreach (var rad in rapport)
            {
                rad.TotalVerdi = rad.TotaleTimer * timeverdi;
            }

            return View(rapport);
        }  

        private readonly ApplicationDbContext _context;

        public AdminRapporterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> LeilighetDetaljer(int id)
        {
            var innstillinger = await _context.Innstillinger.FirstAsync();

            var totaleTimerAlle = await _context.Timeforinger.SumAsync(x => x.AntallTimer);

            var timeverdi = totaleTimerAlle == 0
                ? 0
                : (decimal)innstillinger.Dugnadsbudsjett / totaleTimerAlle;

            var dugnadstimer = await _context.Timeforinger
                .Include(d => d.Dugnad)
                .Include(d => d.Oppgave)
                .Include(d => d.Beboer)
                    .ThenInclude(b => b.Leilighet)
                .Where(d => d.Beboer.LeilighetId == id)
                .OrderByDescending(d => d.RegistrertDato)
                .ToListAsync();

            var model = new LeilighetDetaljerViewModel
            {
                LeilighetId = id,
                Leilighetsnummer = dugnadstimer.FirstOrDefault()?.Beboer.Leilighet.Leilighetsnummer ?? string.Empty,
                AntallRegistreringer = dugnadstimer.Count,
                TotaleTimer = dugnadstimer.Sum(x => x.AntallTimer),
                TotalVerdi = dugnadstimer.Sum(x => x.AntallTimer) * timeverdi,
                Dugnadstimer = dugnadstimer
            };

            return View(model);
        }

        public async Task<IActionResult> TimerPrBeboer()
        {
            var innstillinger = await _context.Innstillinger.FirstAsync();

            var totaleTimerAlle = await _context.Timeforinger.SumAsync(x => x.AntallTimer);

            var timeverdi = totaleTimerAlle == 0
                ? 0
                : (decimal)innstillinger.Dugnadsbudsjett / totaleTimerAlle;

            var model = await _context.Timeforinger
                .Include(d => d.Beboer)
                    .ThenInclude(b => b.Leilighet)
                .GroupBy(d => new
                {
                    d.BeboerId,
                    d.Beboer.Fornavn,
                    d.Beboer.Etternavn,
                    Leilighetsnummer = d.Beboer.Leilighet.Leilighetsnummer
                })
                .Select(g => new RapportTimerPrBeboerViewModel
                {
                    BeboerId = g.Key.BeboerId,
                    Navn = g.Key.Fornavn + " " + g.Key.Etternavn,
                    Leilighetsnummer = g.Key.Leilighetsnummer,
                    AntallRegistreringer = g.Count(),
                    TotaleTimer = g.Sum(x => x.AntallTimer),
                })
                .OrderBy(x => x.Navn)
                .ToListAsync();

            foreach (var rad in model)
            {
                rad.TotalVerdi = rad.TotaleTimer * timeverdi;
            }

            return View(model);
        }

        public async Task<IActionResult> BeboerDetaljer(int id)
        {
            var innstillinger = await _context.Innstillinger.FirstAsync();

            var totaleTimerAlle = await _context.Timeforinger.SumAsync(x => x.AntallTimer);

            var timeverdi = totaleTimerAlle == 0
                ? 0
                : (decimal)innstillinger.Dugnadsbudsjett / totaleTimerAlle;

            var dugnadstimer = await _context.Timeforinger
            .Include(d => d.Dugnad)
            .Include(d => d.Oppgave)
            .Include(d => d.Beboer)
                    .ThenInclude(b => b.Leilighet)
                .Where(d => d.BeboerId == id)
                .OrderByDescending(d => d.RegistrertDato)
                .ToListAsync();

            var model = new BeboerDetaljerViewModel
            {
                BeboerId = id,
                Navn = dugnadstimer.Any()
                ? $"{dugnadstimer.First().Beboer.Fornavn} {dugnadstimer.First().Beboer.Etternavn}"
                : string.Empty,
                Leilighetsnummer = dugnadstimer.FirstOrDefault()?.Beboer.Leilighet.Leilighetsnummer ?? string.Empty,
                AntallRegistreringer = dugnadstimer.Count,
                TotaleTimer = dugnadstimer.Sum(x => x.AntallTimer),
                Dugnadstimer = dugnadstimer
            };

            model.TotalVerdi = model.TotaleTimer * timeverdi;

            return View(model);
        }

        public async Task<IActionResult> TimerPrDugnad()
        {
            var dugnader = await _context.Timeforinger
     .Where(t => t.DugnadId != null)
     .GroupBy(t => new
     {
         t.Dugnad!.StartDato,
         Navn = t.Dugnad.Tittel
     })
     .Select(g => new RapportTimerPrDugnadViewModel
     {
         Dato = g.Min(x => x.RegistrertDato),
         Dugnad = g.Key.Navn,
         Type = "Dugnad",
         Registreringer = g.Count(),
         Timer = g.Sum(x => x.AntallTimer),
         Verdi = 0
     })
     .ToListAsync();

            var oppgaver = await _context.Timeforinger
    .Where(t => t.OppgaveId != null)
    .GroupBy(t => t.Oppgave!.Navn)
    .Select(g => new RapportTimerPrDugnadViewModel
    {
        Dato = g.Min(x => x.RegistrertDato),
        Dugnad = g.Key,
        Type = "Oppgave",
        Registreringer = g.Count(),
        Timer = g.Sum(x => x.AntallTimer),
        Verdi = 0
    })
    .ToListAsync();

            var model = dugnader
                .Concat(oppgaver)
                .OrderBy(x => x.Type)
                .ThenBy(x => x.Dato)
                .ThenBy(x => x.Dugnad)
                .ToList();

            return View(model);
        }
        
            public async Task<IActionResult> DugnadDetaljer(int id)
        {
            var innstillinger = await _context.Innstillinger.FirstAsync();

            var totaleTimerAlle = await _context.Timeforinger.SumAsync(x => x.AntallTimer);

            var timeverdi = totaleTimerAlle == 0
                ? 0
                : (decimal)innstillinger.Dugnadsbudsjett / totaleTimerAlle;

            var dugnadstimer = await _context.Timeforinger
                .Include(d => d.Dugnad)
                .Include(d => d.Beboer)
                    .ThenInclude(b => b.Leilighet)
                .Where(d => d.DugnadId == id)
                .OrderByDescending(d => d.RegistrertDato)
                .ToListAsync();

            var model = new DugnadDetaljerViewModel
            {
                DugnadId = id,
                Tittel = dugnadstimer.FirstOrDefault()?.Dugnad.Tittel ?? string.Empty,
                Dato = dugnadstimer.FirstOrDefault()?.Dugnad.StartDato,
                AntallRegistreringer = dugnadstimer.Count,
                TotaleTimer = dugnadstimer.Sum(x => x.AntallTimer),
                TotalVerdi = dugnadstimer.Sum(x => x.AntallTimer) * timeverdi,
                Dugnadstimer = dugnadstimer
            };

            return View(model);
        }    
    }
}