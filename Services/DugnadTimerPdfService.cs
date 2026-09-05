using DugnadAppMvc.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DugnadAppMvc.Services;

public class DugnadTimerPdfService
{
    private readonly ApplicationDbContext _context;

    public DugnadTimerPdfService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GeneratePdfAsync(string filbane)
    {
        var timeforinger = await _context.Timeforinger
         .AsNoTracking()
         .Include(t => t.Beboer)
         .ThenInclude(b => b.Leilighet)
         .Include(t => t.Oppgave)
         .Include(t => t.Dugnad)
         .OrderBy(t => t.Beboer.Etternavn)
            .ThenBy(t => t.Beboer.Fornavn)
            .ThenBy(t => t.RegistrertDato)
            .ToListAsync();

        if (timeforinger.Count == 0)
        {
            throw new InvalidOperationException(
                "Det finnes ingen registrerte timer å lage rapport av.");
        }

        var mappe = Path.GetDirectoryName(filbane);

        if (!string.IsNullOrWhiteSpace(mappe))
        {
            Directory.CreateDirectory(mappe);
        }

        var generertDato = DateTime.Now;

        var grupper = timeforinger
            .GroupBy(t => t.BeboerId)
            .OrderBy(g => g.First().Beboer.Etternavn)
            .ThenBy(g => g.First().Beboer.Fornavn)
            .ToList();

        var totalTimer = timeforinger.Sum(t => t.AntallTimer);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.DefaultTextStyle(x =>
                    x.FontSize(9));

                page.Header()
                    .Column(header =>
                    {
                        header.Item()
                            .Text("Dugnadstimer")
                            .FontSize(22)
                            .Bold();

                        header.Item()
                            .PaddingTop(4)
                            .Text($"Rapport generert {generertDato:dd.MM.yyyy HH:mm}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken2);
                    });

                page.Content()
                    .PaddingTop(20)
                    .Column(content =>
                    {
                        foreach (var gruppe in grupper)
                        {
                            var beboer = gruppe.First().Beboer;
                            var leilighet =
                                beboer.Leilighet?.Leilighetsnummer
                                ?? "Ukjent";

                            var beboerTotal =
                                gruppe.Sum(t => t.AntallTimer);

                            content.Item()
                                .PaddingTop(10)
                                .Text($"{beboer.Fornavn} {beboer.Etternavn}")
                                .FontSize(13)
                                .Bold();

                            content.Item()
                                .Text($"Leilighet: {leilighet}")
                                .FontSize(9);

                            content.Item()
                                .PaddingTop(6)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(65);
                                        columns.RelativeColumn(2);
                                        columns.ConstantColumn(50);
                                        columns.ConstantColumn(45);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Element(HeaderCellStyle)
                                            .Text("Dato");

                                        header.Cell()
                                            .Element(HeaderCellStyle)
                                            .Text("Aktivitet");

                                        header.Cell()
                                            .Element(HeaderCellStyle)
                                            .AlignRight()
                                            .Text("Timer");

                                        header.Cell()
                                            .Element(HeaderCellStyle)
                                            .Text("Type");

                                        header.Cell()
                                            .Element(HeaderCellStyle)
                                            .Text("Kommentar");
                                    });

                                    foreach (var timeforing in gruppe)
                                    {
                                        table.Cell()
                                            .Element(BodyCellStyle)
                                            .Text(timeforing.RegistrertDato
                                                .ToLocalTime()
                                                .ToString("dd.MM.yyyy"));

                                        table.Cell()
                                            .Element(BodyCellStyle)
                                           .Text(
                                               timeforing.Oppgave?.Navn
                                               ?? timeforing.Dugnad?.Tittel
                                               ?? "Ukjent aktivitet");

                                        table.Cell()
                                            .Element(BodyCellStyle)
                                            .AlignRight()
                                            .Text(timeforing.AntallTimer
                                                .ToString("0.##"));

                                        table.Cell()
                                            .Element(BodyCellStyle)
                                            .Text(
                                                timeforing.Oppgave
                                                    ?.KanUtføresFlereGanger == true
                                                    ? "Gjentakende"
                                                    : "Vanlig");

                                        table.Cell()
                                            .Element(BodyCellStyle)
                                            .Text(timeforing.Kommentar ?? "");
                                    }
                                });

                            content.Item()
                                .PaddingTop(5)
                                .AlignRight()
                                .Text($"Sum {beboer.Fornavn}: {beboerTotal:0.##} timer")
                                .Bold();

                            content.Item()
                                .PaddingBottom(8)
                                .LineHorizontal(1);
                        }

                        content.Item()
                            .PaddingTop(15)
                            .Background(Colors.Grey.Lighten3)
                            .Padding(10)
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .Text("TOTALT ANTALL TIMER")
                                    .Bold()
                                    .FontSize(12);

                                row.ConstantItem(100)
                                    .AlignRight()
                                    .Text($"{totalTimer:0.##} timer")
                                    .Bold()
                                    .FontSize(12);
                            });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("DugnadApp – ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
        })
        .GeneratePdf(filbane);

        return filbane;
    }

    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten2)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Darken1)
            .Padding(4);
    }

    private static IContainer BodyCellStyle(IContainer container)
    {
        return container
            .BorderBottom(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(4);
    }
}