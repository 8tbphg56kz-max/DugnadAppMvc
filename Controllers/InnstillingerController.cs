using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    [Authorize]
    public class InnstillingerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InnstillingerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var innstillinger = await _context.Innstillinger.FirstOrDefaultAsync();

            if (innstillinger == null)
            {
                innstillinger = new Innstillinger();

                _context.Innstillinger.Add(innstillinger);

                await _context.SaveChangesAsync();
            }

            return View(innstillinger);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Innstillinger model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Innstillinger.Update(model);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Innstillingene ble lagret.";

            return RedirectToAction(nameof(Index));
        }
    }
}