using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using DugnadAppMvc.Services;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

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
                ModelState.AddModelError("", "Feil e-postadresse eller passord.");
                return View(model);
            }

            if (!user.IsActivated)
            {
                ModelState.AddModelError("", "Kontoen er ikke aktivert. Velg 'Aktiver konto' for å aktivere kontoen.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Feil e-postadresse eller passord.");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateAccount(ActivateAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && !user.IsActivated)
            {
                // Lager en sikker token
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
                    Request.Scheme);

                await _emailService.SendActivationEmailAsync(
                    user.Email!,
                    activationLink!);
            }

            // Vis alltid samme melding
            ViewBag.Message =
                "Hvis e-postadressen er registrert hos oss, har vi sendt deg en e-post med instruksjoner.";

            return View();
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

            token = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(token));

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

            model.Token = Encoding.UTF8.GetString(
            WebEncoders.Base64UrlDecode(model.Token));

            if (user.IsActivated)
            {
                TempData["Info"] = "Kontoen er allerede aktivert.";
                return RedirectToAction(nameof(Login));
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

            user.IsActivated = true;
            user.ActivatedDate = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Kontoen er aktivert. Du kan nå logge inn.";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ActivationEmailSent(string email)
        {
            ViewBag.Email = email;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ActivateAccount()
        {
            return View();
        }       
    }
}