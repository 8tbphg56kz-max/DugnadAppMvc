using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using DugnadAppMvc.Services;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;

        public AccountController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Email),
                    "Du har ikke opprettet passord ennå. Klikk «Opprett passord» nedenfor.");
                return View(model);
            }

                var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);

                return RedirectToAction(
                    nameof(DashboardController.Index),
                    "Dashboard");
            }

            ModelState.AddModelError("", "Feil e-postadresse eller passord.");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }        

        [HttpGet]
        public async Task<IActionResult> Activate(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (user.IsActivated)
            {
                TempData["Info"] = "Kontoen er allerede aktivert.";
                return RedirectToAction(nameof(Login));
            }

            var model = new ActivateViewModel
            {
                UserId = userId,
                Token = token
            };

            ViewBag.Title = "Velkommen til DugnadApp";
            ViewBag.Description = "Velg et passord for kontoen din.";
            ViewBag.ButtonText = "Aktiver konto";
            ViewBag.Action = "Activate";

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
            
            if (user.IsActivated)
            {
                TempData["Info"] = "Kontoen er allerede aktivert.";
                return RedirectToAction(nameof(Login));
            }

            model.Token = DecodeToken(model.Token);

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

            user.IsActivated = true;
            user.ActivatedDate = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Kontoen er aktivert. Du kan nå logge inn.";

            return RedirectToAction(nameof(Login));
        }        
      
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && user.IsActivated)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                token = WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes(token));

                var resetLink = Url.Action(
                    "ResetPassword",
                    "Account",
                    new
                    {
                        userId = user.Id,
                        token
                    },
                    protocol: "https");

                await _emailService.SendPasswordResetEmailAsync(
                    user.Email!,
                    resetLink!);
            }

            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            var model = new ActivateViewModel
            {
                UserId = userId,
                Token = token
            };

            ViewBag.Title = "Velg nytt passord";
            ViewBag.Description = "Skriv inn et nytt passord.";
            ViewBag.ButtonText = "Lagre nytt passord";
            ViewBag.Action = "ResetPassword";

            return View("Activate", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ActivateViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Activate", model);

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
                return NotFound();          

            model.Token = DecodeToken(model.Token);

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("Activate", model);
            }

            TempData["Success"] = "Passordet er endret. Du kan nå logge inn.";

            return RedirectToAction(nameof(Login));
        }
        private static string DecodeToken(string token)
        {
            return Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(token));
        }

        [HttpGet]
        public IActionResult RequestActivation()
        {
            return View(new RequestActivationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestActivation(RequestActivationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                token = WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes(token));

                var activationLink = Url.Action(
                    "Activate",
                    "Account",
                    new
                    {
                        userId = user.Id,
                        token = token
                    },
                    protocol: "https");

                await _emailService.SendActivationEmailAsync(
                    model.Email,
                    activationLink!);
            }

            return RedirectToAction(nameof(RequestActivationConfirmation));
        }

        [HttpGet]
        public IActionResult RequestActivationConfirmation()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MinKonto()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var beboer = await _context.Beboere
                .Include(b => b.Leilighet)
                .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

            var roller = await _userManager.GetRolesAsync(user);

            var model = new MinKontoViewModel
            {
                Fornavn = user.FirstName,
                Etternavn = user.LastName,
                Epost = user.Email ?? "",
                Leilighet = beboer?.Leilighet?.Leilighetsnummer ?? "",
                Rolle = roller.FirstOrDefault() ?? ""
            };

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult EndrePassord()
        {
            return View(new EndrePassordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndrePassord(EndrePassordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.GammeltPassord,
                model.NyttPassord);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Passordet er endret.";

            return RedirectToAction(nameof(MinKonto));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}