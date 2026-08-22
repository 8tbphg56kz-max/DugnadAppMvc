using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DugnadAppMvc.Controllers
{
    [Authorize(Roles = $"{IdentityRoles.Styremedlem},{IdentityRoles.Administrator},{IdentityRoles.SystemAdministrator}")]
    public class AdminRapporterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TimerPrLeilighet()
        {
            var innstillinger = await _context.Innstillinger.FirstAsync();

            var totaleTimerAlle = await _context.Timeforinger.SumAsync(x => x.AntallTimer);

            var timeverdi = totaleTimerAlle == 0
                ? 0
                : (decimal)innstillinger.Dugnadsbudsjett / totaleTimerAlle;

            var rapport = await _context.Timeforinger
    .Include(d => d.Beboer)
        .ThenInclude(b => b.Leilighet)
   .GroupBy(d => new
   {
       d.Beboer.Leilighet.Id,
       d.Beboer.Leilighet.Leilighetsnummer
   })
    .Select(g => new RapportTimerPrLeilighetViewModel
    {
        LeilighetId = g.Key.Id,

        Visningsnavn = g.Key.Leilighetsnummer,

        Leilighetsnummer = g.Key.Leilighetsnummer,

        AntallRegistreringer = g.Count(),

        TotaleTimer = g.Sum(x => x.AntallTimer)
    })
.OrderBy(x => x.Leilighetsnummer)
.ToListAsync();

            foreach (var rad in rapport)
            {
                rad.TotalVerdi = rad.TotaleTimer * timeverdi;
            }

            return View(rapport);
        }  

        private readonly ApplicationDbContext _context;

        public AdminRapporterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> LeilighetDetaljer(int id)
        {
            var innstillinger = await _context.Innstillinger.FirstAsync();

            var totaleTimerAlle = await _context.Timeforinger.SumAsync(x => x.AntallTimer);

            var timeverdi = totaleTimerAlle == 0
                ? 0
                : (decimal)innstillinger.Dugnadsbudsjett / totaleTimerAlle;

            var dugnadstimer = await _context.Timeforinger
                .Include(d => d.Dugnad)
                .Include(d => d.Oppgave)
                .Include(d => d.Beboer)
                    .ThenInclude(b => b.Leilighet)
                .Where(d => d.Beboer.LeilighetId == id)
                .OrderByDescending(d => d.RegistrertDato)
                .ToListAsync();

            var model = new LeilighetDetaljerViewModel
            {
                LeilighetId = id,
                Leilighetsnummer = dugnadstimer.FirstOrDefault()?.Beboer.Leilighet.Leilighetsnummer ?? string.Empty,
                AntallRegistreringer = dugnadstimer.Count,
                TotaleTimer = dugnadstimer.Sum(x => x.AntallTimer),
                TotalVerdi = dugnadstimer.Sum(x => x.AntallTimer) * timeverdi,
                Dugnadstimer = dugnadstimer
            };

            return View(model);
        }

        public async Task<IActionResult> TimerPrBeboer()
        {
            var innstillinger = await _context.Innstillinger.FirstAsync();

            var totaleTimerAlle = await _context.Timeforinger.SumAsync(x => x.AntallTimer);

            var timeverdi = totaleTimerAlle == 0
                ? 0
                : (decimal)innstillinger.Dugnadsbudsjett / totaleTimerAlle;

            var model = await _context.Timeforinger
                .Include(d => d.Beboer)
                    .ThenInclude(b => b.Leilighet)
                .GroupBy(d => new
                {
                    d.BeboerId,
                    d.Beboer.Fornavn,
                    d.Beboer.Etternavn,
                    Leilighetsnummer = d.Beboer.Leilighet.Leilighetsnummer
                })
                .Select(g => new RapportTimerPrBeboerViewModel
                {
                    BeboerId = g.Key.BeboerId,
                    Navn = g.Key.Fornavn + " " + g.Key.Etternavn,
                    Leilighetsnummer = g.Key.Leilighetsnummer,
                    AntallRegistreringer = g.Count(),
                    TotaleTimer = g.Sum(x => x.AntallTimer),
                })
                .OrderBy(x => x.Navn)
                .ToListAsync();

            foreach (var rad in model)
            {
                rad.TotalVerdi = rad.TotaleTimer * timeverdi;
            }

            return View(model);
        }

        public async Task<IActionResult> BeboerDetaljer(int id)
        {
            var innstillinger = await _context.Innstillinger.FirstAsync();

            var totaleTimerAlle = await _context.Timeforinger.SumAsync(x => x.AntallTimer);

            var timeverdi = totaleTimerAlle == 0
                ? 0
                : (decimal)innstillinger.Dugnadsbudsjett / totaleTimerAlle;

            var dugnadstimer = await _context.Timeforinger
            .Include(d => d.Dugnad)
            .Include(d => d.Oppgave)
            .Include(d => d.Beboer)
                    .ThenInclude(b => b.Leilighet)
                .Where(d => d.BeboerId == id)
                .OrderByDescending(d => d.RegistrertDato)
                .ToListAsync();

            var model = new BeboerDetaljerViewModel
            {
                BeboerId = id,
                Navn = dugnadstimer.Any()
                ? $"{dugnadstimer.First().Beboer.Fornavn} {dugnadstimer.First().Beboer.Etternavn}"
                : string.Empty,
                Leilighetsnummer = dugnadstimer.FirstOrDefault()?.Beboer.Leilighet.Leilighetsnummer ?? string.Empty,
                AntallRegistreringer = dugnadstimer.Count,
                TotaleTimer = dugnadstimer.Sum(x => x.AntallTimer),
                Dugnadstimer = dugnadstimer
            };

            model.TotalVerdi = model.TotaleTimer * timeverdi;

            return View(model);
        }

        public async Task<IActionResult> TimerPrDugnad()
        {
            var dugnader = await _context.Timeforinger
                .Where(t => t.DugnadId != null)
                .GroupBy(t => new
                {
                    t.Dugnad!.StartDato,
                    Navn = t.Dugnad.Tittel
                })
                .Select(g => new RapportTimerPrDugnadViewModel
                {
                    Dato = g.Min(x => x.RegistrertDato),
                    Dugnad = g.Key.Navn,
                    Type = "Dugnad",
                    Registreringer = g.Count(),
                    Timer = g.Sum(x => x.AntallTimer),
                    Verdi = 0m
                })
                .ToListAsync();

            var oppgaver = await _context.Timeforinger
                .Where(t => t.OppgaveId != null)
                .GroupBy(t => t.Oppgave!.Navn)
                .Select(g => new RapportTimerPrDugnadViewModel
                {
                    Dato = g.Min(x => x.RegistrertDato),
                    Dugnad = g.Key,
                    Type = "Oppgave",
                    Registreringer = g.Count(),
                    Timer = g.Sum(x => x.AntallTimer),
                    Verdi = 0m
                })
                .ToListAsync();

            var model = dugnader
                .Concat(oppgaver)
                .OrderBy(x => x.Dato)
                .ThenBy(x => x.Dugnad)
                .ToList();

            var innstillinger = await _context.Innstillinger.FirstAsync();

            var totaltRegistrerteTimer = model.Sum(x => x.Timer);

            decimal timesats = totaltRegistrerteTimer > 0
                ? Math.Round((decimal)innstillinger.Dugnadsbudsjett / totaltRegistrerteTimer, 2)
                : 0m;

            foreach (var rad in model)
            {
                rad.Verdi = Math.Round(rad.Timer * timesats, 2);
            }

            return View(model);
        }

        public async Task<IActionResult> DugnadDetaljer(int id)
        {
            var innstillinger = await _context.Innstillinger.FirstAsync();

            var totaleTimerAlle = await _context.Timeforinger.SumAsync(x => x.AntallTimer);

            var timeverdi = totaleTimerAlle == 0
                ? 0
                : (decimal)innstillinger.Dugnadsbudsjett / totaleTimerAlle;

            var dugnadstimer = await _context.Timeforinger
                .Include(d => d.Dugnad)
                .Include(d => d.Beboer)
                    .ThenInclude(b => b.Leilighet)
                .Where(d => d.DugnadId == id)
                .OrderByDescending(d => d.RegistrertDato)
                .ToListAsync();

            var model = new DugnadDetaljerViewModel
            {
                DugnadId = id,
                Tittel = dugnadstimer.FirstOrDefault()?.Dugnad.Tittel ?? string.Empty,
                Dato = dugnadstimer.FirstOrDefault()?.Dugnad.StartDato,
                AntallRegistreringer = dugnadstimer.Count,
                TotaleTimer = dugnadstimer.Sum(x => x.AntallTimer),
                TotalVerdi = dugnadstimer.Sum(x => x.AntallTimer) * timeverdi,
                Dugnadstimer = dugnadstimer
            };

            return View(model);
        }

        public async Task<IActionResult> TimerPrBeboerPdf()
        {
            var innstillinger = await _context.Innstillinger.FirstAsync();

            var totaleTimerAlle = await _context.Timeforinger
                .SumAsync(x => x.AntallTimer);

            var timeverdi = totaleTimerAlle == 0
                ? 0
                : (decimal)innstillinger.Dugnadsbudsjett / totaleTimerAlle;

            var model = await _context.Timeforinger
                .Include(d => d.Beboer)
                    .ThenInclude(b => b.Leilighet)
                .GroupBy(d => new
                {
                    d.BeboerId,
                    d.Beboer.Fornavn,
                    d.Beboer.Etternavn,
                    Leilighetsnummer = d.Beboer.Leilighet.Leilighetsnummer
                })
                .Select(g => new RapportTimerPrBeboerViewModel
                {
                    BeboerId = g.Key.BeboerId,
                    Navn = g.Key.Fornavn + " " + g.Key.Etternavn,
                    Leilighetsnummer = g.Key.Leilighetsnummer,
                    AntallRegistreringer = g.Count(),
                    TotaleTimer = g.Sum(x => x.AntallTimer)
                })
                .OrderBy(x => x.Navn)
                .ToListAsync();

            foreach (var rad in model)
            {
                rad.TotalVerdi = rad.TotaleTimer * timeverdi;
            }

            var totaltAntallRegistreringer =
                model.Sum(x => x.AntallRegistreringer);

            var totaleTimer =
                model.Sum(x => x.TotaleTimer);

            var totalVerdi =
                model.Sum(x => x.TotalVerdi);

            var rapportDato = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);

                    page.DefaultTextStyle(x =>
                        x.FontSize(9));

                    // HEADER
                    page.Header()
                        .Column(header =>
                        {
                            header.Item()
                                .Text("Timer pr. beboer")
                                .FontSize(18)
                                .Bold();

                            header.Item()
                                .Text($"Rapport generert: {rapportDato}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);

                            header.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1);
                        });

                    // INNHOLD
                    page.Content()
                        .PaddingTop(15)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1.3f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.4f);
                            });

                            // HEADER
                            table.Header(header =>
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(6)
                                    .Text("Beboer")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(6)
                                    .Text("Leilighet")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(6)
                                    .AlignRight()
                                    .Text("Registreringer")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(6)
                                    .AlignRight()
                                    .Text("Timer")
                                    .Bold();

                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(6)
                                    .AlignRight()
                                    .Text("Verdi")
                                    .Bold();
                            });

                            // RADER
                            foreach (var rad in model)
                            {
                                table.Cell()
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6)
                                    .Text(rad.Navn);

                                table.Cell()
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6)
                                    .Text(rad.Leilighetsnummer);

                                table.Cell()
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6)
                                    .AlignRight()
                                    .Text(rad.AntallRegistreringer.ToString());

                                table.Cell()
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6)
                                    .AlignRight()
                                    .Text(rad.TotaleTimer.ToString("N1"));

                                table.Cell()
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6)
                                    .AlignRight()
                                    .Text($"{rad.TotalVerdi:N0} kr");
                            }

                            // TOTAL
                            table.Cell()
                                .Background(Colors.Grey.Lighten3)
                                .Padding(6)
                                .Text("Totalt")
                                .Bold();

                            table.Cell()
                                .Background(Colors.Grey.Lighten3)
                                .Padding(6)
                                .Text("");

                            table.Cell()
                                .Background(Colors.Grey.Lighten3)
                                .Padding(6)
                                .AlignRight()
                                .Text(totaltAntallRegistreringer.ToString())
                                .Bold();

                            table.Cell()
                                .Background(Colors.Grey.Lighten3)
                                .Padding(6)
                                .AlignRight()
                                .Text(totaleTimer.ToString("N1"))
                                .Bold();

                            table.Cell()
                                .Background(Colors.Grey.Lighten3)
                                .Padding(6)
                                .AlignRight()
                                .Text($"{totalVerdi:N0} kr")
                                .Bold();
                        });

                    // FOOTER
                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("DugnadApp  •  Side ");
                            text.CurrentPageNumber();
                            text.Span(" av ");
                            text.TotalPages();
                        });
                });
            });

            var pdf = document.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                "Timer-pr-beboer.pdf");
        }
    }
}