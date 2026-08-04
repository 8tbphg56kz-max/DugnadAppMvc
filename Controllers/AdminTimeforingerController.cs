using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Index(int? leilighetId, int? dugnadId, int? beboerId)
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

            if (dugnadId.HasValue)
            {
                query = query.Where(d => d.DugnadId == dugnadId.Value);
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

            model.Dugnader = await _context.Dugnader
    .OrderBy(d => d.Tittel)
    .Select(d => new SelectListItem
    {
        Value = d.Id.ToString(),
        Text = d.Tittel
    })
    .ToListAsync();

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
            model.DugnadId = dugnadId;

            return View(model);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpGet]
        public IActionResult Create()
        {
            var model = new TimeforingViewModel
            {

                Dugnader = _context.Dugnader
                    .Where(d => d.ErSynlig)
                    .OrderBy(d => d.StartDato)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Tittel
                    })
                    .ToList(),

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

            model.TimerAlternativer.Add(new SelectListItem
            {
                Value = "",
                Text = "Velg timer..."
            });

            FyllTimerAlternativer(model.TimerAlternativer);

            return View(model);
        }

        [Authorize(Roles = IdentityRoles.AdminAccess)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCreateTimeforingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Dugnader = _context.Dugnader
                    .Where(d => d.ErSynlig)
                    .OrderBy(d => d.StartDato)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Tittel
                    })
                    .ToList();

                model.Beboere = _context.Beboere
                    .OrderBy(b => b.Etternavn)
                    .ThenBy(b => b.Fornavn)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = b.Etternavn + ", " + b.Fornavn
                    })
                    .ToList();

                model.TimerAlternativer.Add(new SelectListItem
                {
                    Value = "",
                    Text = "Velg timer..."
                });

                FyllTimerAlternativer(model.TimerAlternativer);

                return View(model);
            }

            var timeforing = new Timeforing
            {
                DugnadId = model.DugnadId,
                BeboerId = model.BeboerId!.Value,
                AntallTimer = model.Timer!.Value,
                Kommentar = model.Kommentar
            };

            _context.Timeforinger.Add(timeforing);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void FyllTimerAlternativer(List<SelectListItem> liste)
        {
            for (decimal timer = 0.5m; timer <= 10m; timer += 0.5m)
            {
                liste.Add(new SelectListItem
                {
                    Value = timer.ToString("0.#"),
                    Text = timer.ToString("0.#")
                });
            }
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

            FyllTimerAlternativer(model.TimerAlternativer);

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


                FyllTimerAlternativer(model.TimerAlternativer);

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
            var timeforing = await _context.Timeforinger.FindAsync(id);

            if (timeforing == null)
            {
                return NotFound();
            }

            _context.Timeforinger.Remove(timeforing);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Timeføringen ble slettet.";

            return RedirectToAction(nameof(Index));
        }
    }
}