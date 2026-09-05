using DugnadAppMvc.Models;
using Microsoft.AspNetCore.Http;

namespace DugnadAppMvc.Services;

public class OppgaveBildeService
{
    private readonly string _mappe;

    private const long MaksFilstorrelse = 5 * 1024 * 1024;
    private const int MaksAntallBilder = 5;

    private static readonly string[] TillatteUtvidelser =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    public OppgaveBildeService(IWebHostEnvironment environment)
    {
        _mappe = Path.Combine(environment.ContentRootPath, "oppgavebilder");

        Directory.CreateDirectory(_mappe);
    }

    public async Task<List<OppgaveBilde>> LagreBilderAsync(
        int oppgaveId,
        IEnumerable<IFormFile>? filer)
    {
        var bilder = filer?
            .Where(f => f != null && f.Length > 0)
            .ToList()
            ?? new List<IFormFile>();

        if (bilder.Count == 0)
            return new List<OppgaveBilde>();

        if (bilder.Count > MaksAntallBilder)
        {
            throw new InvalidOperationException(
                $"Du kan laste opp maksimalt {MaksAntallBilder} bilder.");
        }

        var resultat = new List<OppgaveBilde>();

        foreach (var fil in bilder)
        {
            if (fil.Length > MaksFilstorrelse)
            {
                throw new InvalidOperationException(
                    $"Bildet «{fil.FileName}» er større enn 5 MB.");
            }

            var utvidelse =
                Path.GetExtension(fil.FileName).ToLowerInvariant();

            if (!TillatteUtvidelser.Contains(utvidelse))
            {
                throw new InvalidOperationException(
                    $"Bildet «{fil.FileName}» har en filtype som ikke er tillatt.");
            }

            if (!await ErGyldigBildefilAsync(fil, utvidelse))
            {
                throw new InvalidOperationException(
                    $"Filen «{fil.FileName}» ser ikke ut til å være et gyldig bilde.");
            }

            var filnavn =
                $"{Guid.NewGuid():N}{utvidelse}";

            var filbane =
                Path.Combine(_mappe, filnavn);

            await using var stream =
                new FileStream(
                    filbane,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None);

            await fil.CopyToAsync(stream);

            resultat.Add(new OppgaveBilde
            {
                OppgaveId = oppgaveId,
                Filnavn = filnavn,
                OriginaltFilnavn = Path.GetFileName(fil.FileName),
                LastetOpp = DateTime.UtcNow
            });
        }

        return resultat;
    }

    public string HentFilbane(string filnavn)
    {
        return Path.Combine(
            _mappe,
            Path.GetFileName(filnavn));
    }

    public void SlettBilde(OppgaveBilde bilde)
    {
        var filbane = HentFilbane(bilde.Filnavn);

        if (File.Exists(filbane))
        {
            File.Delete(filbane);
        }
    }

    private static async Task<bool> ErGyldigBildefilAsync(
        IFormFile fil,
        string utvidelse)
    {
        await using var stream = fil.OpenReadStream();

        var buffer = new byte[12];
        var antall = await stream.ReadAsync(buffer);

        if (utvidelse == ".jpg" || utvidelse == ".jpeg")
        {
            return antall >= 3 &&
                   buffer[0] == 0xFF &&
                   buffer[1] == 0xD8 &&
                   buffer[2] == 0xFF;
        }

        if (utvidelse == ".png")
        {
            return antall >= 8 &&
                   buffer[0] == 0x89 &&
                   buffer[1] == 0x50 &&
                   buffer[2] == 0x4E &&
                   buffer[3] == 0x47 &&
                   buffer[4] == 0x0D &&
                   buffer[5] == 0x0A &&
                   buffer[6] == 0x1A &&
                   buffer[7] == 0x0A;
        }

        if (utvidelse == ".webp")
        {
            return antall >= 12 &&
                   buffer[0] == 0x52 &&
                   buffer[1] == 0x49 &&
                   buffer[2] == 0x46 &&
                   buffer[3] == 0x46 &&
                   buffer[8] == 0x57 &&
                   buffer[9] == 0x45 &&
                   buffer[10] == 0x42 &&
                   buffer[11] == 0x50;
        }

        return false;
    }
}