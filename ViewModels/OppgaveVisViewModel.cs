using DugnadAppMvc.Models;

public class OppgaveVisViewModel
{
    public Oppgave Oppgave { get; set; } = null!;

    public bool ErPameldt { get; set; }

    public bool ErFulltegnet =>
        Oppgave.Pameldinger.Count >= Oppgave.AntallPersoner;
}