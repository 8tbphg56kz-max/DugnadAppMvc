namespace DugnadAppMvc.Models;

public enum EpostVarselStatus
{
    IkkeSendt = 0,
    Sender = 1,
    Sendt = 2,
    DelvisSendt = 3
}

public class EpostVarsel
{
    public int Id { get; set; }

    public int? OppgaveId { get; set; }
    public Oppgave? Oppgave { get; set; }

    public int? DugnadId { get; set; }
    public Dugnad? Dugnad { get; set; }

    public DateTime? SendtDato { get; set; }

    public string SendtAvBrukerId { get; set; } = null!;

    public ApplicationUser SendtAvBruker { get; set; } = null!;

    public EpostVarselStatus Status { get; set; }
        = EpostVarselStatus.IkkeSendt;

    public ICollection<EpostVarselMottaker> Mottakere { get; set; }
        = new List<EpostVarselMottaker>();
}