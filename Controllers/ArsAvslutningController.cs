using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using DugnadAppMvc.Services;
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
    private readonly DugnadTimerPdfService _pdfService;

    public ArsAvslutningController(
        ApplicationDbContext context,
        IDatabaseBackupService backupService,
        DugnadTimerPdfService pdfService)
    {
        _context = context;
        _backupService = backupService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> Backup()
    {
        try
        {
            // Lag filnavn med dato og klokkeslett
            var tidspunkt = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");

            var pdfFilnavn =
                $"dugnadapp-timer-{tidspunkt}.pdf";

            var pdfFilbane =
                Path.Combine("/app/backups", pdfFilnavn);

            var pdfResult =
    await _pdfService.GeneratePdfAsync(pdfFilbane);

            if (!System.IO.File.Exists(pdfResult))
            {
                throw new InvalidOperationException(
                    $"PDF ble ikke funnet etter generering: {pdfResult}");
            }

            ViewBag.PdfFilnavn = Path.GetFileName(pdfResult);

            var backupResult =
                await _backupService.CreateBackupAsync();

            if (!System.IO.File.Exists(pdfResult))
            {
                throw new InvalidOperationException(
                    $"PDF-filen forsvant etter databasebackup: {pdfResult}");
            }

            ViewBag.Success = true;

            ViewBag.Message =
                $"Databasebackup gjennomført.\n" +
                $"PDF-rapport: {Path.GetFileName(pdfResult)}\n\n" +
                backupResult;

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
            Aar = GetDugnadsStartAar(),

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
            model.Aar = GetDugnadsStartAar();

            model.AntallDugnader =
                await _context.Dugnader.CountAsync();

            model.AntallOppgaver =
                await _context.Oppgaver.CountAsync();

            model.AntallPameldinger =
                await _context.OppgavePameldinger.CountAsync();

            model.AntallTimeforinger =
                await _context.Timeforinger.CountAsync();

            model.AntallDeltakere =
                await _context.Timeforinger
                    .Select(t => t.BeboerId)
                    .Distinct()
                    .CountAsync();

            model.AntallTimer =
                await _context.Timeforinger
                    .Select(t => (decimal?)t.AntallTimer)
                    .SumAsync() ?? 0m;

            return View(model);
        }

        // -------------------------------------------------
        // Finn dugnadsåret
        // -------------------------------------------------

        var aar = GetDugnadsStartAar();

        // Eksempel:
        // November 2026 -> Aar = 2025 -> 2025/2026

        if (DateTime.Now.Month != 11)
        {
            TempData["Error"] =
                "Årsavslutning kan bare gjennomføres i november.";

            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------
        // Kontroller at årsstatistikken ikke allerede finnes
        // -------------------------------------------------

        var eksisterendeStatistikk =
            await _context.Arsstatistikker
                .AnyAsync(a => a.Aar == aar);

        if (eksisterendeStatistikk)
        {
            TempData["Error"] =
                $"Årsstatistikk for dugnadsåret {aar}/{aar + 1} " +
                "finnes allerede. Årsavslutningen kan ikke gjennomføres på nytt.";

            return RedirectToAction(nameof(Index));
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // -------------------------------------------------
            // HENT ÅRETS STATISTIKK FØR SLETTING
            // -------------------------------------------------

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


            // -------------------------------------------------
            // LAGRE HOVEDSTATISTIKK
            // -------------------------------------------------

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


            // -------------------------------------------------
            // HENT LEILIGHETER
            // -------------------------------------------------

            var leiligheter =
                await _context.Leiligheter
                    .AsNoTracking()
                    .ToListAsync();

            // Vi har:
            // HBL = 25 leiligheter
            // LBL = 14 leiligheter

            var leiligheterPerBygg =
                leiligheter
                    .Where(l =>
                        !string.IsNullOrWhiteSpace(l.Leilighetsnummer) &&
                        l.Leilighetsnummer.Length >= 3)
                    .GroupBy(l =>
                        l.Leilighetsnummer
                            .Substring(0, 3)
                            .Trim()
                            .ToUpper())
                    .Where(g =>
                        g.Key == "HBL" ||
                        g.Key == "LBL")
                    .ToDictionary(
                        g => g.Key,
                        g => g.Count());

            var totaltAntallLeiligheter =
                leiligheterPerBygg.Values.Sum();


            // -------------------------------------------------
            // HENT TIMEFØRINGER MED LEILIGHET
            // -------------------------------------------------

            var timeforingerMedBygg =
                await _context.Timeforinger
                    .Include(t => t.Beboer)
                        .ThenInclude(b => b.Leilighet)
                    .AsNoTracking()
                    .ToListAsync();


            // -------------------------------------------------
            // SUMMER TIMER PER BYGG
            // -------------------------------------------------

            var timerPerBygg =
                timeforingerMedBygg
                    .Where(t =>
                        t.Beboer?.Leilighet != null &&
                        !string.IsNullOrWhiteSpace(
                            t.Beboer.Leilighet.Leilighetsnummer) &&
                        t.Beboer.Leilighet.Leilighetsnummer.Length >= 3)
                    .GroupBy(t =>
                        t.Beboer.Leilighet.Leilighetsnummer
                            .Substring(0, 3)
                            .Trim()
                            .ToUpper())
                    .Where(g =>
                        g.Key == "HBL" ||
                        g.Key == "LBL")
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(t => t.AntallTimer));


            // -------------------------------------------------
            // LAGRE BYGGSTATISTIKK
            // -------------------------------------------------

            var arsstatistikkBygg =
                new List<ArsstatistikkBygg>();

            foreach (var bygg in leiligheterPerBygg)
            {
                var byggKode = bygg.Key;

                var antallLeiligheter =
                    bygg.Value;

                var dugnadstimer =
                    timerPerBygg.TryGetValue(
                        byggKode,
                        out var timer)
                        ? timer
                        : 0m;

                var andelLeiligheter =
                    totaltAntallLeiligheter > 0
                        ? (decimal)antallLeiligheter /
                          totaltAntallLeiligheter * 100m
                        : 0m;

                var andelDugnadstimer =
                    antallTimer > 0
                        ? dugnadstimer /
                          antallTimer * 100m
                        : 0m;

                var dugnadsindeks =
                    andelLeiligheter > 0
                        ? andelDugnadstimer /
                          andelLeiligheter * 100m
                        : 0m;

                arsstatistikkBygg.Add(
                    new ArsstatistikkBygg
                    {
                        Aar = aar,

                        ByggKode = byggKode,

                        AntallLeiligheter =
                            antallLeiligheter,

                        AndelLeiligheter =
                            andelLeiligheter,

                        Dugnadstimer =
                            dugnadstimer,

                        AndelDugnadstimer =
                            andelDugnadstimer,

                        Dugnadsindeks =
                            dugnadsindeks
                    });
            }

            _context.ArsstatistikkBygg.AddRange(
                arsstatistikkBygg);


            // -------------------------------------------------
            // SLETT TIMEFØRINGER
            // -------------------------------------------------

            var timeforinger =
                await _context.Timeforinger.ToListAsync();

            _context.Timeforinger.RemoveRange(
                timeforinger);


            // -------------------------------------------------
            // SLETT OPPGAVEPÅMELDINGER
            // -------------------------------------------------

            var pameldinger =
                await _context.OppgavePameldinger
                    .ToListAsync();

            _context.OppgavePameldinger.RemoveRange(
                pameldinger);


            // -------------------------------------------------
            // SLETT OPPGAVER
            // -------------------------------------------------

            var oppgaver =
                await _context.Oppgaver.ToListAsync();

            _context.Oppgaver.RemoveRange(
                oppgaver);


            // -------------------------------------------------
            // SLETT DUGNADER
            // -------------------------------------------------

            var dugnader =
                await _context.Dugnader.ToListAsync();

            _context.Dugnader.RemoveRange(
                dugnader);


            // -------------------------------------------------
            // SLETT ENDRINGSLOGGER
            // -------------------------------------------------

            var endringslogger =
                await _context.Endringslogger
                    .ToListAsync();

            _context.Endringslogger.RemoveRange(
                endringslogger);


            // -------------------------------------------------
            // LAGRE ALT
            // -------------------------------------------------

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();


            TempData["Success"] =
                $"Årsavslutning for dugnadsåret " +
                $"{aar}/{aar + 1} er gjennomført. " +
                "Årsstatistikk og byggstatistikk er lagret, " +
                "og årets registreringer er slettet.";

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
    private static int GetDugnadsStartAar()
    {
        var now = DateTime.Now;

        // Dugnadsåret går fra desember til november.
        // Januar–november 2026 = dugnadsåret 2025/2026.
        // Desember 2026 = nytt dugnadsår 2026/2027.
        return now.Month >= 12
            ? now.Year
            : now.Year - 1;
    }

    [HttpGet]
    public IActionResult LastNedPdf(string filnavn)
    {
        if (string.IsNullOrWhiteSpace(filnavn))
        {
            return NotFound();
        }

        // Tillat kun selve filnavnet – ikke mapper/stier
        filnavn = Path.GetFileName(filnavn);

        if (!filnavn.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        var filbane = Path.Combine("/app/backups", filnavn);

        if (!System.IO.File.Exists(filbane))
        {
            return NotFound();
        }

        return PhysicalFile(
            filbane,
            "application/pdf",
            filnavn);
    }
}
