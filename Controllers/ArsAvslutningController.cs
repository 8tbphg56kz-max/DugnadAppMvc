using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using DugnadAppMvc.Services.Interfaces;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = IdentityRoles.AdminAccess)]
public class ArsAvslutningController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDatabaseBackupService _backupService;

    public ArsAvslutningController(
        ApplicationDbContext context,
        IDatabaseBackupService backupService)
    {
        _context = context;
        _backupService = backupService;
    }

    [HttpGet]
    public async Task<IActionResult> Backup()
    {
        try
        {
            var result = await _backupService.CreateBackupAsync();

            ViewBag.Success = true;
            ViewBag.Message = result;

            return View();
        }
        catch (Exception ex)
        {
            ViewBag.Success = false;
            ViewBag.Message = ex.Message;

            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new ArsAvslutningViewModel
        {
            Aar = DateTime.Now.Year,

            AntallDugnader =
                await _context.Dugnader.CountAsync(),

            AntallOppgaver =
                await _context.Oppgaver.CountAsync(),

            AntallPameldinger =
                await _context.OppgavePameldinger.CountAsync(),

            AntallTimeforinger =
                await _context.Timeforinger.CountAsync(),

            AntallDeltakere =
                await _context.Timeforinger
                    .Select(t => t.BeboerId)
                    .Distinct()
                    .CountAsync(),

            AntallTimer =
                await _context.Timeforinger
                    .Select(t => (decimal?)t.AntallTimer)
                    .SumAsync() ?? 0m
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
    ArsAvslutningViewModel model)
    {
        if (!model.UtbetalingForetatt)
        {
            ModelState.AddModelError(
                nameof(model.UtbetalingForetatt),
                "Du må bekrefte at utbetaling til beboere er foretatt.");
        }

        if (!model.BekreftSletting)
        {
            ModelState.AddModelError(
                nameof(model.BekreftSletting),
                "Du må bekrefte at årets registreringer kan slettes.");
        }

        if (!ModelState.IsValid)
        {
            model.Aar = DateTime.Now.Year;

            model.AntallDugnader =
                await _context.Dugnader.CountAsync();

            model.AntallOppgaver =
                await _context.Oppgaver.CountAsync();

            model.AntallPameldinger =
                await _context.OppgavePameldinger.CountAsync();

            model.AntallTimeforinger =
                await _context.Timeforinger.CountAsync();

            return View(model);
        }

        var aar = DateTime.Now.Year;

        // Kontroller at årsstatistikken ikke allerede er lagret
        var eksisterendeStatistikk =
            await _context.Arsstatistikker
                .AnyAsync(a => a.Aar == aar);

        if (eksisterendeStatistikk)
        {
            TempData["Error"] =
                $"Årsstatistikk for {aar} finnes allerede. " +
                "Årsavslutningen kan ikke gjennomføres på nytt.";

            return RedirectToAction(nameof(Index));
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // ---------------------------------------------
            // Hent statistikk FØR registreringene slettes
            // ---------------------------------------------

            var antallDugnader =
                await _context.Dugnader.CountAsync();

            var antallOppgaver =
                await _context.Oppgaver.CountAsync();

            var antallPameldinger =
                await _context.OppgavePameldinger.CountAsync();

            var antallTimeforinger =
                await _context.Timeforinger.CountAsync();

            var antallDeltakere =
                await _context.Timeforinger
                    .Select(t => t.BeboerId)
                    .Distinct()
                    .CountAsync();

            var antallTimer =
                await _context.Timeforinger
                    .Select(t => (decimal?)t.AntallTimer)
                    .SumAsync() ?? 0m;

            // ---------------------------------------------
            // Lagre årsstatistikk
            // ---------------------------------------------

            var arsstatistikk = new Arsstatistikk
            {
                Aar = aar,

                AntallAktiviteter =
                    antallDugnader + antallOppgaver,

                AntallPameldinger =
                    antallPameldinger,

                AntallDeltakere =
                    antallDeltakere,

                AntallTimer =
                    antallTimer
            };

            _context.Arsstatistikker.Add(arsstatistikk);

            // ---------------------------------------------
            // Slett Timeforinger
            // ---------------------------------------------

            var timeforinger =
                await _context.Timeforinger.ToListAsync();

            _context.Timeforinger.RemoveRange(timeforinger);

            // ---------------------------------------------
            // Slett Oppgavepåmeldinger
            // ---------------------------------------------

            var pameldinger =
                await _context.OppgavePameldinger.ToListAsync();

            _context.OppgavePameldinger.RemoveRange(pameldinger);

            // ---------------------------------------------
            // Slett Oppgaver
            // ---------------------------------------------

            var oppgaver =
                await _context.Oppgaver.ToListAsync();

            _context.Oppgaver.RemoveRange(oppgaver);

            // ---------------------------------------------
            // Slett Dugnader
            // ---------------------------------------------

            var dugnader =
                await _context.Dugnader.ToListAsync();

            _context.Dugnader.RemoveRange(dugnader);

            // ---------------------------------------------
            // Lagre statistikk + sletting
            // ---------------------------------------------

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["Success"] =
                $"Årsavslutning for {aar} er gjennomført. " +
                "Årsstatistikken er lagret og årets registreringer er slettet.";

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await transaction.RollbackAsync();

            TempData["Error"] =
                "Det oppstod en feil under årsavslutningen. " +
                "Ingen registreringer eller årsstatistikk ble lagret.";

            return RedirectToAction(nameof(Index));
        }
    }
}
