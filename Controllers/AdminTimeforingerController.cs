using DugnadAppMvc.Data;
using DugnadAppMvc.Helpers;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace DugnadAppMvc.Controllers
{
    [Authorize(Roles = IdentityRoles.BoardAccess)]
    public class AdminTimeforingerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminTimeforingerController(
     ApplicationDbContext context,
     UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
        Text = b.Etternavn + ", " + b.Fornavn
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
                    Beboer = d.Beboer.Etternavn + ", " + d.Beboer.Fornavn,
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

            var bruker = await _userManager.GetUserAsync(User);

            if (bruker == null)
            {
                return Challenge();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var timeforing = new Timeforing
                {
                    BeboerId = model.BeboerId!.Value,
                    AntallTimer = decimal.Parse(
                        model.Timer!,
                        CultureInfo.InvariantCulture),
                    Kommentar = model.Kommentar
                };

                string aktivitetNavn;

                if (model.Aktivitet.StartsWith("D-"))
                {
                    var dugnadId = int.Parse(model.Aktivitet[2..]);

                    timeforing.DugnadId = dugnadId;

                    aktivitetNavn = await _context.Dugnader
                        .Where(d => d.Id == dugnadId)
                        .Select(d => d.Tittel)
                        .FirstOrDefaultAsync()
                        ?? "Ukjent dugnad";
                }
                else if (model.Aktivitet.StartsWith("O-"))
                {
                    var oppgaveId = int.Parse(model.Aktivitet[2..]);

                    timeforing.OppgaveId = oppgaveId;

                    aktivitetNavn = await _context.Oppgaver
                        .Where(o => o.Id == oppgaveId)
                        .Select(o => o.Navn)
                        .FirstOrDefaultAsync()
                        ?? "Ukjent oppgave";
                }
                else
                {
                    aktivitetNavn = "Ukjent aktivitet";
                }

                // Lagre timeføringen først slik at Id blir generert
                _context.Timeforinger.Add(timeforing);

                await _context.SaveChangesAsync();

                // Nå har timeforing.Id fått sin faktiske database-ID
                var endringslogg = new Endringslogg
                {
                    Tidspunkt = DateTime.UtcNow,
                    BrukerId = bruker.Id,
                    Handling = "Registrert",
                    Begrunnelse = model.Begrunnelse!,

                    TimeforingId = timeforing.Id,
                    BeboerId = timeforing.BeboerId,

                    Aktivitet = aktivitetNavn,

                    GamleTimer = null,
                    NyeTimer = timeforing.AntallTimer,

                    GammelKommentar = null,
                    NyKommentar = timeforing.Kommentar
                };

                _context.Endringslogger.Add(endringslogg);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    "Timeføringen ble registrert og logget.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Det oppstod en feil ved registrering av timeføringen.");

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
                Timer = timeforing.AntallTimer.ToString(
                 CultureInfo.InvariantCulture),
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

            var timeforing = await _context.Timeforinger
                .Include(t => t.Dugnad)
                .Include(t => t.Oppgave)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (timeforing == null)
            {
                return NotFound();
            }

            // Ta vare på verdiene før endringen
            var gammelBeboerId = timeforing.BeboerId;
            var gamleTimer = timeforing.AntallTimer;
            var gammelKommentar = timeforing.Kommentar;

            var aktivitetNavn = timeforing.OppgaveId != null
            ? timeforing.Oppgave!.Navn
            : timeforing.Dugnad!.Tittel;

            // Gjør selve endringen
            timeforing.BeboerId = model.BeboerId!.Value;
            timeforing.AntallTimer = decimal.Parse(
                model.Timer!,
                CultureInfo.InvariantCulture);
            timeforing.Kommentar = model.Kommentar;           

            // Hent innlogget bruker
            var brukerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (brukerId == null)
            {
                return Forbid();
            }

            // Opprett endringslogg
            var endringslogg = new Endringslogg
            {
                BrukerId = brukerId,
                Handling = "Endret",
                TimeforingId = timeforing.Id,
                BeboerId = gammelBeboerId,
                Aktivitet = aktivitetNavn,
                GamleTimer = gamleTimer,
                NyeTimer = timeforing.AntallTimer,
                GammelKommentar = gammelKommentar,
                NyKommentar = timeforing.Kommentar,
                Begrunnelse = model.Begrunnelse!
            };

            _context.Endringslogger.Add(endringslogg);

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
        public async Task<IActionResult> DeleteConfirmed(
    int id,
    AdminTimeforingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var timeforingForVisning = await _context.Timeforinger
                    .Include(t => t.Dugnad)
                    .Include(t => t.Oppgave)
                    .Include(t => t.Beboer)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (timeforingForVisning == null)
                {
                    return NotFound();
                }

                model.Id = timeforingForVisning.Id;
                model.Registrert = timeforingForVisning.RegistrertDato;
                model.Beboer = timeforingForVisning.Beboer.Fornavn + " " +
                               timeforingForVisning.Beboer.Etternavn;
                model.Timer = timeforingForVisning.AntallTimer;
                model.Kommentar = timeforingForVisning.Kommentar;
                model.Aktivitet = timeforingForVisning.OppgaveId != null
                    ? timeforingForVisning.Oppgave!.Navn
                    : timeforingForVisning.Dugnad!.Tittel;

                return View(model);
            }

            var timeforing = await _context.Timeforinger
                .Include(t => t.Dugnad)
                .Include(t => t.Oppgave)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (timeforing == null)
            {
                return NotFound();
            }

            var bruker = await _userManager.GetUserAsync(User);

            if (bruker == null)
            {
                return Challenge();
            }

            var aktivitet = timeforing.OppgaveId != null
                ? timeforing.Oppgave!.Navn
                : timeforing.Dugnad!.Tittel;

            var endringslogg = new Endringslogg
            {
                Tidspunkt = DateTime.UtcNow,
                BrukerId = bruker.Id,
                Handling = "Slettet",
                Begrunnelse = model.Begrunnelse!,
                TimeforingId = timeforing.Id,
                BeboerId = timeforing.BeboerId,
                Aktivitet = aktivitet,
                GamleTimer = timeforing.AntallTimer,
                NyeTimer = null,
                GammelKommentar = timeforing.Kommentar,
                NyKommentar = null
            };

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

            _context.Endringslogger.Add(endringslogg);
            _context.Timeforinger.Remove(timeforing);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Timeføringen og tilhørende påmelding ble slettet.";

            return RedirectToAction(nameof(Index));
        }
    }
}