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
        public IActionResult Create()
        {
            var model = new DugnadstimeViewModel();

            FyllDugnader(model);
            FyllTimerAlternativer(model.TimerAlternativer);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DugnadstimeViewModel model)
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

            var dugnadstime = new Dugnadstime
            {
                DugnadId = model.DugnadId,
                BeboerId = beboer.Id,
                Timer = model.Timer!.Value,
                Kommentar = model.Kommentar
            };

            _context.Dugnadstimer.Add(dugnadstime);

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

            var historikk = await _context.Dugnadstimer
                .Include(d => d.Dugnad)
                .Where(d => d.BeboerId == beboer.Id)
                .OrderByDescending(d => d.Registrert)
                .Select(d => new DugnadstimeHistorikkViewModel
                {
                    Id = d.Id,
                    Registrert = d.Registrert,
                    Dugnad = d.Dugnad.Tittel,
                    Timer = d.Timer,
                    Kommentar = d.Kommentar
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

            var model = new DugnadstimeHistorikkSideViewModel
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

            var dugnadstime = await _context.Dugnadstimer
                .Include(d => d.Dugnad)
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

            var model = new EditDugnadstimeViewModel
            {
                Id = dugnadstime.Id,
                Dugnad = dugnadstime.Dugnad.Tittel,
                Timer = dugnadstime.Timer,
                Kommentar = dugnadstime.Kommentar
            };

            FyllTimerAlternativer(model.TimerAlternativer);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditDugnadstimeViewModel model)
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

            var dugnadstime = await _context.Dugnadstimer
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

            dugnadstime.Timer = model.Timer!.Value;
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

            var dugnadstime = await _context.Dugnadstimer
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

            _context.Dugnadstimer.Remove(dugnadstime);

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

        private void FyllDugnader(DugnadstimeViewModel model)
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

        private static bool KanEndresEllerSlettes(Dugnadstime dugnadstime)
        {
            return dugnadstime.Registrert > DateTime.UtcNow.AddHours(-1);
        }
    }
}