using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
   [Authorize]
    public class DugnadstimerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DugnadstimerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create(int? dugnadId)
        {
            var model = new TimeforingViewModel();

            if (dugnadId.HasValue)
            {
                model.DugnadId = dugnadId.Value;

                var dugnad = _context.Dugnader
                    .FirstOrDefault(d => d.Id == dugnadId.Value);

                if (dugnad != null)
                {
                    model.DugnadNavn = dugnad.Tittel;
                }
            }

            FyllDugnader(model);
            FyllTimerAlternativer(model.TimerAlternativer);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TimeforingViewModel model)
        {
            ModelState.Remove(nameof(model.BeboerId));

            if (!ModelState.IsValid)
            {
                FyllDugnader(model);
                FyllTimerAlternativer(model.TimerAlternativer);
                return View(model);
            }

            var beboer = await HentInnloggetBeboerAsync();

            if (beboer == null)
            {
                return Challenge();
            }

            var timeforing = new Timeforing
            {
                DugnadId = model.DugnadId,
                BeboerId = beboer.Id,
                AntallTimer = model.Timer!.Value,
                Kommentar = model.Kommentar
            };

            _context.Timeforinger.Add(timeforing);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Dugnadstimen ble registrert.";

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var beboer = await HentInnloggetBeboerAsync();

            if (beboer == null)
            {
                return Challenge();
            }

            var historikk = await _context.Timeforinger
    .Include(t => t.Oppgave)
    .Include(t => t.Dugnad)
    .Where(t => t.BeboerId == beboer.Id)
    .OrderByDescending(t => t.RegistrertDato)
    .Select(t => new TimeforingHistorikkViewModel
    {
        Id = t.Id,
        Registrert = t.RegistrertDato,

        Type = t.OppgaveId != null ? "Oppgave" : "Dugnad",

        Aktivitet = t.OppgaveId != null
            ? t.Oppgave!.Navn
            : t.Dugnad!.Tittel,

        // Midlertidig slik at eksisterende view fortsatt virker
        Dugnad = t.OppgaveId != null
            ? t.Oppgave!.Navn
            : t.Dugnad!.Tittel,

        Timer = t.AntallTimer,
        Kommentar = t.Kommentar
    })
    .ToListAsync();

            if (historikk.Any())
            {
                foreach (var registrering in historikk)
                {
                    var kanEndres = registrering.Registrert > DateTime.UtcNow.AddHours(-1);

                    registrering.KanRedigeres = kanEndres;
                    registrering.KanSlettes = kanEndres;
                }
            }

            var model = new TimeforingHistorikkSideViewModel
            {
                AntallRegistreringer = historikk.Count,
                TotaltAntallTimer = historikk.Sum(h => h.Timer),
                Historikk = historikk
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var beboer = await HentInnloggetBeboerAsync();

            if (beboer == null)
            {
                return Challenge();
            }

            var dugnadstime = await _context.Timeforinger
                .Include(d => d.Dugnad)
                .Include(d => d.Oppgave)
                .FirstOrDefaultAsync(d =>
                    d.Id == id &&
                    d.BeboerId == beboer.Id);

            if (dugnadstime == null)
            {
                return NotFound();
            }

            if (!KanEndresEllerSlettes(dugnadstime))
            {
                TempData["ErrorMessage"] =
                    "Dugnadstimen kan ikke lenger redigeres.";

                return RedirectToAction(nameof(Index));
            }

            var model = new EditTimeforingViewModel
            {
                Id = dugnadstime.Id,
                Aktivitet = dugnadstime.OppgaveId != null
    ? dugnadstime.Oppgave!.Navn
    : dugnadstime.Dugnad!.Tittel,
                Timer = dugnadstime.AntallTimer,
                Kommentar = dugnadstime.Kommentar
            };

            FyllTimerAlternativer(model.TimerAlternativer);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTimeforingViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                FyllTimerAlternativer(model.TimerAlternativer);
                return View(model);
            }

            var beboer = await HentInnloggetBeboerAsync();

            if (beboer == null)
            {
                return Challenge();
            }

            var dugnadstime = await _context.Timeforinger
                .FirstOrDefaultAsync(d =>
                    d.Id == id &&
                    d.BeboerId == beboer.Id);

            if (dugnadstime == null)
            {
                return NotFound();
            }

            if (!KanEndresEllerSlettes(dugnadstime))
            {
                TempData["ErrorMessage"] =
                    "Dugnadstimen kan ikke endres etter én time.";

                return RedirectToAction(nameof(Index));
            }

            dugnadstime.AntallTimer = model.Timer!.Value;
            dugnadstime.Kommentar = model.Kommentar;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Dugnadstimen ble oppdatert.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var beboer = await HentInnloggetBeboerAsync();

            if (beboer == null)
            {
                return Challenge();
            }

            var dugnadstime = await _context.Timeforinger
                .FirstOrDefaultAsync(d =>
                    d.Id == id &&
                    d.BeboerId == beboer.Id);

            if (dugnadstime == null)
            {
                return NotFound();
            }

            if (!KanEndresEllerSlettes(dugnadstime))
            {
                TempData["ErrorMessage"] =
                    "Dugnadstimen kan ikke slettes etter én time.";

                return RedirectToAction(nameof(Index));
            }

            _context.Timeforinger.Remove(dugnadstime);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Dugnadstimen ble slettet.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<Beboer?> HentInnloggetBeboerAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return null;
            }

            return await _context.Beboere
                .SingleOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);
        }

        private void FyllDugnader(TimeforingViewModel model)
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
        }

        private void FyllTimerAlternativer(List<SelectListItem> liste)
        {
            liste.Clear();

            liste.Add(new SelectListItem
            {
                Value = "",
                Text = "Velg timer..."
            });

            for (decimal timer = 0.5m; timer <= 10m; timer += 0.5m)
            {
                liste.Add(new SelectListItem
                {
                    Value = timer.ToString("0.#"),
                    Text = timer.ToString("0.#")
                });
            }
        }

        private static bool KanEndresEllerSlettes(Timeforing timeforing)
        {
            return timeforing.RegistrertDato > DateTime.UtcNow.AddHours(-1);
        }
    }
}