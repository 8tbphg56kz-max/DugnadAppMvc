using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using DugnadAppMvc.Helpers;

namespace DugnadAppMvc.Controllers
{
    [Authorize(Roles = IdentityRoles.BoardAccess)]
    public class AdminTimeforingerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminTimeforingerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? leilighetId, string? aktivitet, int? beboerId)
        {
             var query = _context.Timeforinger
    .Include(d => d.Dugnad)
    .Include(d => d.Oppgave)
    .Include(d => d.Beboer)
        .ThenInclude(b => b.Leilighet)
    .AsQueryable();

            if (leilighetId.HasValue)
            {
                query = query.Where(d => d.Beboer.LeilighetId == leilighetId.Value);
            }

            if (!string.IsNullOrWhiteSpace(aktivitet))
            {
                if (aktivitet.StartsWith("D-"))
                {
                    var dugnadId = int.Parse(aktivitet[2..]);
                    query = query.Where(t => t.DugnadId == dugnadId);
                }
                else if (aktivitet.StartsWith("O-"))
                {
                    var oppgaveId = int.Parse(aktivitet[2..]);
                    query = query.Where(t => t.OppgaveId == oppgaveId);
                }
            }

            if (beboerId.HasValue)
            {
                query = query.Where(d => d.BeboerId == beboerId.Value);
            }

            var model = new AdminTimeforingerIndexViewModel();

            model.Beboere = await _context.Beboere
    .OrderBy(b => b.Etternavn)
    .ThenBy(b => b.Fornavn)
    .Select(b => new SelectListItem
    {
        Value = b.Id.ToString(),
        Text = b.Fornavn + " " + b.Etternavn
    })
    .ToListAsync();

            model.Leiligheter = await _context.Leiligheter
                .OrderBy(l => l.Leilighetsnummer)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Leilighetsnummer
                })
                .ToListAsync();

            model.Aktiviteter = new List<SelectListItem>();

            model.Aktiviteter.AddRange(
                await _context.Dugnader
                    .OrderBy(d => d.Tittel)
                    .Select(d => new SelectListItem
                    {
                        Value = $"D-{d.Id}",
                        Text = $"📅 {d.Tittel}"
                    })
                    .ToListAsync());

            model.Aktiviteter.AddRange(
                await _context.Oppgaver
                    .OrderBy(o => o.Navn)
                    .Select(o => new SelectListItem
                    {
                        Value = $"O-{o.Id}",
                        Text = $"🛠 {o.Navn}"
                    })
                    .ToListAsync());

            model.Dugnadstimer = await query
                .OrderByDescending(d => d.RegistrertDato)
                .Select(d => new AdminTimeforingViewModel
                {
                    Id = d.Id,
                    Registrert = d.RegistrertDato,
                    Aktivitet = d.OppgaveId != null ? d.Oppgave!.Navn : d.Dugnad!.Tittel,
                    Beboer = d.Beboer.Fornavn + " " + d.Beboer.Etternavn,
                    Timer = d.AntallTimer,
                    Kommentar = d.Kommentar
                })
                .ToListAsync();

            model.LeilighetId = leilighetId;
            model.Aktivitet = aktivitet;

            return View(model);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpGet]
        public IActionResult Create()
        {
            var model = new AdminCreateTimeforingViewModel
            {
                Beboere = _context.Beboere
                    .OrderBy(b => b.Etternavn)
                    .ThenBy(b => b.Fornavn)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = b.Etternavn + ", " + b.Fornavn
                    })
                    .ToList()
            };

            model.Aktiviteter = new List<SelectListItem>();

            model.Aktiviteter.AddRange(
                _context.Dugnader
                    .Where(d => d.ErSynlig)
                    .OrderBy(d => d.StartDato)
                    .Select(d => new SelectListItem
                    {
                        Value = $"D-{d.Id}",
                        Text = $"📅 {d.Tittel}"
                    })
                    .ToList());

            model.Aktiviteter.AddRange(
                _context.Oppgaver
                    .OrderBy(o => o.Navn)
                    .Select(o => new SelectListItem
                    {
                        Value = $"O-{o.Id}",
                        Text = $"🛠 {o.Navn}"
                    })
                    .ToList());

            model.TimerAlternativer = TimerAlternativerHelper.Hent();

            return View(model);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCreateTimeforingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Aktiviteter = new List<SelectListItem>();

                model.Aktiviteter.AddRange(
                    _context.Dugnader
                        .Where(d => d.ErSynlig)
                        .OrderBy(d => d.StartDato)
                        .Select(d => new SelectListItem
                        {
                            Value = $"D-{d.Id}",
                            Text = $"📅 {d.Tittel}"
                        })
                        .ToList());

                model.Aktiviteter.AddRange(
                    _context.Oppgaver
                        .OrderBy(o => o.Navn)
                        .Select(o => new SelectListItem
                        {
                            Value = $"O-{o.Id}",
                            Text = $"🛠 {o.Navn}"
                        })
                        .ToList());

                model.Beboere = _context.Beboere
                    .OrderBy(b => b.Etternavn)
                    .ThenBy(b => b.Fornavn)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = b.Etternavn + ", " + b.Fornavn
                    })
                    .ToList();


                model.TimerAlternativer = TimerAlternativerHelper.Hent();

                return View(model);
            }

            var timeforing = new Timeforing
            {
                BeboerId = model.BeboerId!.Value,
                AntallTimer = decimal.Parse(
    model.Timer!,
    System.Globalization.CultureInfo.InvariantCulture),
                Kommentar = model.Kommentar
            };

            if (model.Aktivitet.StartsWith("D-"))
            {
                timeforing.DugnadId = int.Parse(model.Aktivitet[2..]);
            }
            else if (model.Aktivitet.StartsWith("O-"))
            {
                timeforing.OppgaveId = int.Parse(model.Aktivitet[2..]);
            }

            _context.Timeforinger.Add(timeforing);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }         

    [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var timeforing = await _context.Timeforinger
                .Include(t => t.Dugnad)
                .Include(t => t.Oppgave)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (timeforing == null)
            {
                return NotFound();
            }

            var model = new EditTimeforingViewModel
            {
                Id = timeforing.Id,

                Aktivitet = timeforing.OppgaveId != null
                    ? timeforing.Oppgave!.Navn
                    : timeforing.Dugnad!.Tittel,

                BeboerId = timeforing.BeboerId,
                Timer = timeforing.AntallTimer,
                Kommentar = timeforing.Kommentar,

                Beboere = _context.Beboere
                    .OrderBy(b => b.Etternavn)
                    .ThenBy(b => b.Fornavn)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = b.Etternavn + ", " + b.Fornavn
                    })
                    .ToList()
            };

            model.TimerAlternativer = TimerAlternativerHelper.Hent();

            return View(model);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTimeforingViewModel model)
        {
            if (!ModelState.IsValid)
            {   
                model.Beboere = _context.Beboere
                    .OrderBy(b => b.Etternavn)
                    .ThenBy(b => b.Fornavn)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = b.Etternavn + ", " + b.Fornavn
                    })
                    .ToList();


                model.TimerAlternativer = TimerAlternativerHelper.Hent();

                var aktivitet = await _context.Timeforinger
    .Include(t => t.Dugnad)
    .Include(t => t.Oppgave)
    .FirstOrDefaultAsync(t => t.Id == id);

                if (aktivitet != null)
                {
                    model.Aktivitet = aktivitet.OppgaveId != null
                        ? aktivitet.Oppgave!.Navn
                        : aktivitet.Dugnad!.Tittel;
                }

                return View(model);
            }

            var timeforing = await _context.Timeforinger.FindAsync(id);

            if (timeforing == null)
            {
                return NotFound();
            }
           
            timeforing.BeboerId = model.BeboerId!.Value;
            timeforing.AntallTimer = model.Timer!.Value;
            timeforing.Kommentar = model.Kommentar;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Timeføringen ble oppdatert.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _context.Timeforinger
                .Include(d => d.Dugnad)
                .Include(d => d.Oppgave)
                .Include(d => d.Beboer)
                .Where(d => d.Id == id)
                .Select(d => new AdminTimeforingViewModel
                {
                    Id = d.Id,
                    Registrert = d.RegistrertDato,
                    Aktivitet = d.OppgaveId != null ? d.Oppgave!.Navn : d.Dugnad!.Tittel,
                    Beboer = d.Beboer.Fornavn + " " + d.Beboer.Etternavn,
                    Timer = d.AntallTimer,
                    Kommentar = d.Kommentar
                })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var timeforing = await _context.Timeforinger
                .FirstOrDefaultAsync(t => t.Id == id);

            if (timeforing == null)
                return NotFound();

            // Hvis timeføringen gjelder en oppgave,
            // slett også tilhørende påmelding
            if (timeforing.OppgaveId.HasValue)
            {
                var pamelding = await _context.OppgavePameldinger
                    .FirstOrDefaultAsync(p =>
                        p.OppgaveId == timeforing.OppgaveId.Value &&
                        p.BeboerId == timeforing.BeboerId);

                if (pamelding != null)
                {
                    _context.OppgavePameldinger.Remove(pamelding);
                }
            }

            _context.Timeforinger.Remove(timeforing);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Timeføringen og tilhørende påmelding ble slettet.";

            return RedirectToAction(nameof(Index));
        }
    }
}