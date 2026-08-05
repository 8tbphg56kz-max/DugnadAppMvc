namespace DugnadAppMvc.ViewModels;

using DugnadAppMvc.Models;
using DugnadAppMvc.Models.Enums;

public class OppgaveIndexViewModel
{
    public IEnumerable<Oppgave> Oppgaver { get; set; } = [];

    public bool? ErUtfort { get; set; } = null;
}