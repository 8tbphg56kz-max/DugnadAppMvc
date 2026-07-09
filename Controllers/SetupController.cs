using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using DugnadAppMvc.Models.ViewModels;
using DugnadAppMvc.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    public class SetupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserProvisioningService _userProvisioningService;

        public SetupController(
     ApplicationDbContext context,
     UserManager<ApplicationUser> userManager,
     UserProvisioningService userProvisioningService)
        {
            _context = context;
            _userManager = userManager;
            _userProvisioningService = userProvisioningService;
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

            // Opprett beboer
            var beboer = new Beboer
            {
                Fornavn = model.Fornavn,
                Etternavn = model.Etternavn,
                Epost = model.Epost,
                ErAdmin = true,
                Aktiv = true,
                LeilighetId = leilighet.Id
            };

            _context.Beboere.Add(beboer);
            await _context.SaveChangesAsync();

            await _userProvisioningService.CreateUserAsync(beboer);

            TempData["SuccessMessage"] =
                "Administratoren er opprettet. Du kan nå opprette passord fra innloggingssiden.";

            return RedirectToAction("Login", "Account");        
    }
    }
}