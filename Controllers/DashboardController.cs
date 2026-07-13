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

            decimal totalHours = 0;

            if (currentUser != null)
            {
                var beboer = await _context.Beboere
                    .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

                if (beboer != null)
                {
                    totalHours = await _context.Dugnadstimer
                        .Where(dt => dt.BeboerId == beboer.Id)
                        .SumAsync(dt => (decimal?)dt.Timer) ?? 0;
                }
            }

            var model = new DashboardViewModel
            {
                FirstName = currentUser?.FirstName ?? "",
                TotalHours = totalHours,
                ActiveTasks = 0,
                HasCommonDugnad = false,
                BoardMessage = null
            };

            return View(model);
        }
    }

}