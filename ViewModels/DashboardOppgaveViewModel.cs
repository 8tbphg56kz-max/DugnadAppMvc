using DugnadAppMvc.Models.Enums;

public class DashboardOppgaveViewModel
{
    public int Id { get; set; }

    public string Tittel { get; set; } = string.Empty;

    public DateTime? Frist { get; set; }

    public int AntallPersoner { get; set; }

    public int AntallPameldte { get; set; }

    public bool ErPameldt { get; set; }

    public bool ErFulltegnet => AntallPameldte >= AntallPersoner;
    public OppgaveStatus? Status { get; set; }
}