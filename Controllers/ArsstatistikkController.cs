using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = IdentityRoles.BoardAccess)]
public class ArsstatistikkController : Controller
{
    private readonly ApplicationDbContext _context;

    public ArsstatistikkController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var aar = GetDugnadsStartAar();

        // -------------------------------------------------
        // HISTORISK ÅRSSTATISTIKK
        // -------------------------------------------------

        var arsstatistikker = await _context.Arsstatistikker
            .OrderByDescending(a => a.Aar)
            .ToListAsync();

        var byggStatistikk = await _context.ArsstatistikkBygg
            .OrderByDescending(b => b.Aar)
            .ThenBy(b => b.ByggKode)
            .ToListAsync();


        // -------------------------------------------------
        // LØPENDE STATISTIKK FOR PÅGÅENDE DUGNADSÅR
        // -------------------------------------------------

        var antallDugnader =
            await _context.Dugnader.CountAsync();

        var antallOppgaver =
            await _context.Oppgaver.CountAsync();

        var antallPameldinger =
            await _context.OppgavePameldinger.CountAsync();

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
        // LEILIGHETER PER BYGG
        // -------------------------------------------------

        var leiligheter =
            await _context.Leiligheter
                .AsNoTracking()
                .ToListAsync();

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
        // TIMEFØRINGER PER BYGG
        // -------------------------------------------------

        var timeforingerMedBygg =
            await _context.Timeforinger
                .Include(t => t.Beboer)
                    .ThenInclude(b => b.Leilighet)
                .AsNoTracking()
                .ToListAsync();

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
        // BEREGN LØPENDE BYGGSTATISTIKK
        // -------------------------------------------------

        var paagaaendeByggStatistikk =
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

            paagaaendeByggStatistikk.Add(
                new ArsstatistikkBygg
                {
                    Aar = aar,
                    ByggKode = byggKode,
                    AntallLeiligheter = antallLeiligheter,
                    AndelLeiligheter = andelLeiligheter,
                    Dugnadstimer = dugnadstimer,
                    AndelDugnadstimer = andelDugnadstimer,
                    Dugnadsindeks = dugnadsindeks
                });
        }


        // -------------------------------------------------
        // VIEWMODEL
        // -------------------------------------------------

        var model = new ArsstatistikkViewModel
        {
            Aar = aar,

            AntallAktiviteter =
                antallDugnader + antallOppgaver,

            AntallPameldinger =
                antallPameldinger,

            AntallDeltakere =
                antallDeltakere,

            AntallTimer =
                antallTimer,

            Arsstatistikker =
                arsstatistikker,

            ByggStatistikk =
                byggStatistikk,

            PaagaaendeByggStatistikk =
                paagaaendeByggStatistikk
        };

        return View(model);
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
}