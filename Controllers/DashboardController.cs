using DugnadAppMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DugnadAppMvc.Data;
using DugnadAppMvc.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    { 

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var model = new DashboardViewModel
            {
                FirstName = currentUser?.FirstName ?? "",
                TotalHours = 12.5m,           // Midlertidig
                ActiveTasks = 0,              // Midlertidig
                HasCommonDugnad = false,      // Midlertidig
                BoardMessage = null           // Midlertidig
            };

            return View(model);
        }
    }

}