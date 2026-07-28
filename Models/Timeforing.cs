using DugnadAppMvc.Models;

public class Timeforing
{
    public int Id { get; set; }

    public int OppgaveId { get; set; }
    public Oppgave Oppgave { get; set; } = null!;

    public int BeboerId { get; set; }
    public Beboer Beboer { get; set; } = null!;

    public decimal AntallTimer { get; set; }

    public string? Kommentar { get; set; }

    public DateTime RegistrertDato { get; set; }
}