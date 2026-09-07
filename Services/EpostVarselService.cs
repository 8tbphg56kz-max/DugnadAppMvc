using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Services;

public class EpostVarselService
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;

    public EpostVarselService(
        ApplicationDbContext context,
        EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task SendOppgaveVarselAsync(
        int oppgaveId,
        string sendtAvBrukerId)
    {
        // Kontroller at oppgaven finnes
        var oppgave = await _context.Oppgaver
            .FirstOrDefaultAsync(o => o.Id == oppgaveId);

        if (oppgave == null)
        {
            throw new InvalidOperationException(
                "Oppgaven ble ikke funnet.");
        }

        // Kontroller at det ikke allerede finnes et varsel
        var eksisterendeVarsel = await _context.EpostVarsler
            .FirstOrDefaultAsync(e => e.OppgaveId == oppgaveId);

        if (eksisterendeVarsel != null)
        {
            throw new InvalidOperationException(
                "Det er allerede opprettet et e-postvarsel for denne oppgaven.");
        }

        // Finn alle beboere med registrert e-postadresse
        var beboere = await _context.Beboere
     .Where(b =>
         !string.IsNullOrWhiteSpace(b.Epost) &&
         !b.IkkeMottaEpost)
     .OrderBy(b => b.Etternavn)
     .ThenBy(b => b.Fornavn)
     .ToListAsync();

        if (beboere.Count == 0)
        {
            throw new InvalidOperationException(
                "Det finnes ingen beboere med registrert e-postadresse.");
        }

        // Opprett selve varslet
        var varsel = new EpostVarsel
        {
            OppgaveId = oppgave.Id,
            SendtAvBrukerId = sendtAvBrukerId,
            Status = EpostVarselStatus.Sender,
            SendtDato = null
        };

        _context.EpostVarsler.Add(varsel);

        await _context.SaveChangesAsync();

        // Opprett mottakerliste
        var mottakere = beboere
            .Select(b => new EpostVarselMottaker
            {
                EpostVarselId = varsel.Id,
                BeboerId = b.Id,
                Epostadresse = b.Epost,
                Sendt = false
            })
            .ToList();

        _context.EpostVarselMottakere.AddRange(mottakere);

        await _context.SaveChangesAsync();

        // Lenke til oppgaven
        var link =
            $"https://dugnad.owet.no/Oppgaver/Vis/{oppgave.Id}";

        var datoTekst =
            $"Periode: {oppgave.FraDato:dd.MM.yyyy} – " +
            $"{oppgave.Frist:dd.MM.yyyy}";

        // Send til hver mottaker
        foreach (var mottaker in mottakere)
        {
            try
            {
                await _emailService.SendNewActivityEmailAsync(
                    mottaker.Epostadresse,
                    "Oppgave",
                    oppgave.Navn,
                    datoTekst,
                    link);

                mottaker.Sendt = true;
                mottaker.SendtDato = DateTime.UtcNow;
                mottaker.Feilmelding = null;
            }
            catch (Exception ex)
            {
                mottaker.Sendt = false;
                mottaker.SendtDato = null;
                mottaker.Feilmelding = ex.Message;
            }

            await _context.SaveChangesAsync();
        }

        // Finn resultatet av utsendelsen
        var antallSendt = mottakere.Count(m => m.Sendt);
        var antallFeilet = mottakere.Count(m => !m.Sendt);

        varsel.Status =
            antallFeilet == 0
                ? EpostVarselStatus.Sendt
                : EpostVarselStatus.DelvisSendt;

        varsel.SendtDato = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}