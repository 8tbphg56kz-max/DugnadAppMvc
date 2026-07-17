using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    //[Authorize(Roles = "Administrator")]
    public class AdminDugnadstimerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDugnadstimerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _context.Dugnadstimer
                .Include(d => d.Dugnad)
                .Include(d => d.Beboer)
                .OrderByDescending(d => d.Registrert)
                .Select(d => new AdminDugnadstimeViewModel
                {
                    Id = d.Id,
                    Registrert = d.Registrert,
                    Dugnad = d.Dugnad.Tittel,
                    Beboer = d.Beboer.Fornavn + " " + d.Beboer.Etternavn,
                    Timer = d.Timer,
                    Kommentar = d.Kommentar
                })
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new DugnadstimeViewModel
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DugnadstimeViewModel model)
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

            var dugnadstime = new Dugnadstime
            {
                DugnadId = model.DugnadId,
                BeboerId = model.BeboerId!.Value,
                Timer = model.Timer!.Value,
                Kommentar = model.Kommentar
            };

            _context.Dugnadstimer.Add(dugnadstime);

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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dugnadstime = await _context.Dugnadstimer
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dugnadstime == null)
            {
                return NotFound();
            }

            var model = new DugnadstimeViewModel
            {
                DugnadId = dugnadstime.DugnadId,
                BeboerId = dugnadstime.BeboerId,
                Timer = dugnadstime.Timer,
                Kommentar = dugnadstime.Kommentar,

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
    }
}