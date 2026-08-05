namespace DugnadAppMvc.ViewModels;

using DugnadAppMvc.Models;
using DugnadAppMvc.Models.Enums;

public class OppgaveIndexViewModel
{
    public IEnumerable<Oppgave> Oppgaver { get; set; } = [];

    public string? Sok { get; set; }

    public OppgavePrioritet? Prioritet { get; set; }

    public bool? ErUtfort { get; set; }
}