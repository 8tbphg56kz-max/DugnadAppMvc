using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using DugnadAppMvc.Models.Enums;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;

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

            Beboer? beboer = null;

            if (currentUser != null)
            {
                beboer = await _context.Beboere
                   .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

                if (beboer != null)
                {
                    totalHours = await _context.Timeforinger
                   .Where(tf => tf.BeboerId == beboer.Id)
                   .SumAsync(tf => (decimal?)tf.AntallTimer) ?? 0;
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
                beboer = await _context.Beboere
                    .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

                if (beboer != null)
                {
                    foreach (var oppgave in oppgaver)
                    {
                        var pamelding = oppgave.Pameldinger
                            .FirstOrDefault(p => p.BeboerId == beboer.Id);

                        oppgave.ErPameldt =
                        pamelding != null &&
                        pamelding.Status != OppgaveStatus.TimerRegistrert;
                        oppgave.MinStatus = pamelding?.Status;
                    }
                }
            }

            var iDag = DateOnly.FromDateTime(DateTime.Today);

            var nesteDugnad = await _context.Dugnader
            .Where(d => d.ErSynlig)
            .OrderBy(d => d.StartDato)
            .FirstOrDefaultAsync();

            var antallRegistreringerPaAktivDugnad = 0;

            if (beboer != null && nesteDugnad != null)
            {
                antallRegistreringerPaAktivDugnad = await _context.Timeforinger
                    .CountAsync(tf =>
                        tf.BeboerId == beboer.Id &&
                        tf.DugnadId == nesteDugnad.Id);
            }

            var sisteStyremelding = await _context.BoardMessages
            .OrderByDescending(m => m.PublisertDato)
            .FirstOrDefaultAsync();

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
        .Where(o => !o.ErPameldt &&
                    o.MinStatus != OppgaveStatus.TimerRegistrert)
        .ToList(),

                NesteDugnad = nesteDugnad,
                AntallRegistreringerPaAktivDugnad = antallRegistreringerPaAktivDugnad,

                HasCommonDugnad = false,
                SisteStyremelding = sisteStyremelding
            };

            return View(model);
        }
    }
}