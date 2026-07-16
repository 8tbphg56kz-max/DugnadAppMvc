using DugnadAppMvc.Data;
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
            var model = new DugnadstimeViewModel
            {
                Dato = DateTime.Today,

                Dugnader = _context.Dugnader
                    .Where(d => d.ErSynlig)
                    .OrderBy(d => d.StartDato)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Tittel
                    })
                    .ToList()
            };

            // Fyll nedtrekkslisten for timer
            model.TimerAlternativer.Add(new SelectListItem
            {
                Value = "",
                Text = "Velg timer..."
            });

            FyllTimerAlternativer(model.TimerAlternativer);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DugnadstimeViewModel model)
        {           
            if (!ModelState.IsValid)
                return View(model);

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var beboer = _context.Beboere
    .SingleOrDefault(b => b.ApplicationUserId == currentUser.Id);

            if (beboer == null)
            {
                ModelState.AddModelError("", "Fant ikke tilknyttet beboer.");

                return View(model);
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

            return RedirectToAction("Index", "Dashboard");

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var beboer = await _context.Beboere
                .SingleOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

            if (beboer == null)
            {
                return NotFound();
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
                var siste = historikk.First();

                var kanEndres = siste.Registrert > DateTime.UtcNow.AddHours(-1);

                siste.KanRedigeres = kanEndres;
                siste.KanSlettes = kanEndres;
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
            var dugnadstime = await _context.Dugnadstimer
                .Include(d => d.Dugnad)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dugnadstime == null)
            {
                return NotFound();
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
        public async Task<IActionResult> Edit(EditDugnadstimeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Fyll ned timerlisten igjen
                FyllTimerAlternativer(model.TimerAlternativer);

                return View(model);
            }

            var dugnadstime = await _context.Dugnadstimer.FindAsync(model.Id);

            if (dugnadstime == null)
            {
                return NotFound();
            }

            dugnadstime.Timer = model.Timer!.Value;
            dugnadstime.Kommentar = model.Kommentar;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dugnadstime = await _context.Dugnadstimer.FindAsync(id);

            if (dugnadstime == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var beboer = await HentInnloggetBeboerAsync();

            if (beboer == null)
            {
                return Forbid();
            }

            _context.Dugnadstimer.Remove(dugnadstime);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}