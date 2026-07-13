using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

            for (decimal timer = 0.5m; timer <= 10m; timer += 0.5m)
            {
                model.TimerAlternativer.Add(new SelectListItem
                {
                    Value = timer.ToString(),
                    Text = timer.ToString("0.0")
                });
            }

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
    }
}