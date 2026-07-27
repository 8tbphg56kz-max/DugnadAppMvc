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
            var totalActiveTasks = await _context.Oppgaver
                .CountAsync(o => !o.ErUtført);

            var oppgaver = await _context.Oppgaver
                .Include(o => o.Pameldinger)
                .Where(o => !o.ErUtført)
                .OrderBy(o => o.Prioritet)
                .ThenBy(o => o.Frist)
                .Take(4)
                .ToListAsync();

            if (currentUser != null)
            {
                var beboer = await _context.Beboere
                    .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

                if (beboer != null)
                {
                    foreach (var oppgave in oppgaver)
                    {
                        oppgave.ErPameldt = oppgave.Pameldinger
                            .Any(p => p.BeboerId == beboer.Id);
                    }
                }
            }

            var model = new DashboardViewModel
            {
                FirstName = currentUser?.FirstName ?? "",

                TotalHours = totalHours,

                ActiveTasks = oppgaver.Count,
                TotalActiveTasks = totalActiveTasks,

                MineOppgaver = oppgaver
        .Where(o => o.ErPameldt)
        .ToList(),

                LedigeOppgaver = oppgaver
        .Where(o => !o.ErPameldt)
        .ToList(),

                HasCommonDugnad = false,
                BoardMessage = null
            };

            return View(model);
        }
    }
}