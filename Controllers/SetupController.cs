using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using DugnadAppMvc.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    public class SetupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SetupController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (await _context.Beboere.AnyAsync())
                return RedirectToAction("Index", "Home");

            return View(new SetupViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(SetupViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Beboere.AnyAsync())
                return RedirectToAction("Index", "Home");

            // Opprett sameie
            var sameie = new Sameie
            {
                Navn = model.SameieNavn
            };

            _context.Sameier.Add(sameie);
            await _context.SaveChangesAsync();

            // Opprett første leilighet
            var leilighet = new Leilighet
            {
                Seksjonsnummer = 1,
                Leilighetsnummer = model.Leilighet
            };

            _context.Leiligheter.Add(leilighet);
            await _context.SaveChangesAsync();

            // Opprett Identity-bruker
            var user = new ApplicationUser
            {
                UserName = model.Epost,
                Email = model.Epost,
                EmailConfirmed = true,
                FirstName = model.Fornavn,
                LastName = model.Etternavn
            };

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            // Opprett beboer
            var beboer = new Beboer
            {
                Fornavn = model.Fornavn,
                Etternavn = model.Etternavn,
                Epost = model.Epost,
                ErAdmin = true,
                Aktiv = true,
                LeilighetId = leilighet.Id,
                ApplicationUserId = user.Id
            };

            _context.Beboere.Add(beboer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "Account");
        }
    }
}