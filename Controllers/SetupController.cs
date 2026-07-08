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
        private readonly EmailService _emailService;

        public SetupController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    UserProvisioningService userProvisioningService,
    EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _userProvisioningService = userProvisioningService;
            _emailService = emailService;
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

            var provisioningResult =
                await _userProvisioningService.CreateUserAsync(beboer);

            var activationLink = Url.Action(
    "Activate",
    "Account",
    new
    {
        userId = provisioningResult.User.Id,
        token = provisioningResult.ResetPasswordToken
    },
    protocol: "https");

            await _emailService.SendActivationEmailAsync(
                beboer.Epost,
                activationLink!);

            return RedirectToAction(
    "ActivationEmailSent",
    "Account",
    new { email = beboer.Epost });
        }
    }
}