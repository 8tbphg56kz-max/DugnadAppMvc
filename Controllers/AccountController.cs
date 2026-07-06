using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using DugnadAppMvc.Services;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;
        private readonly LoginCodeService _loginCodeService;

        public AccountController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    EmailService emailService,
    LoginCodeService loginCodeService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
             _loginCodeService = loginCodeService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (!_context.Beboere.Any())
            {
                return RedirectToAction("Index", "Setup");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Finn beboeren
            var beboer = await _context.Beboere
                .FirstOrDefaultAsync(b => b.Epost == model.Email);

            // Ikke avslør om e-posten finnes
            if (beboer == null)
                return View("LoginLinkSent", model);

            // Finn eller opprett Identity-bruker
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    FirstName = beboer.Fornavn,
                    LastName = beboer.Etternavn
                };

                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Kunne ikke opprette bruker.");
                    return View(model);
                }

                // Koble beboeren til Identity-brukeren
                beboer.ApplicationUserId = user.Id;
                await _context.SaveChangesAsync();
            }

            if (beboer.ApplicationUserId != user.Id)
            {
                beboer.ApplicationUserId = user.Id;
                await _context.SaveChangesAsync();
            }

            // Lag engangskode
            var kode = await _loginCodeService.CreateCodeAsync(model.Email);

            // Send e-post
            await _emailService.SendLoginCodeAsync(model.Email, kode);

            // Gå til siden der brukeren skriver inn koden
            return RedirectToAction(nameof(VerifyCode), new { epost = model.Email });
        }

        [HttpGet]
        public IActionResult VerifyCode(string epost)
        {
            return View(new VerifyCodeViewModel
            {
                Epost = epost
            });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var loginCode = await _loginCodeService.ValidateCodeAsync(
                model.Epost,
                model.Kode);

            if (loginCode == null)
            {
                ModelState.AddModelError("", "Ugyldig eller utløpt kode.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Epost);

            if (user == null)
            {
                ModelState.AddModelError("", "Brukeren finnes ikke.");
                return View(model);
            }

            await _loginCodeService.MarkAsUsedAsync(loginCode);

            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Activate(string userId, string token)
        {
            var model = new ActivateViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(ActivateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}