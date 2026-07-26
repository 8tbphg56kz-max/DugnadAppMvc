using DugnadAppMvc.Models;

namespace DugnadAppMvc.ViewModels
{
    public class OppgaveMineViewModel
    {
        public Oppgave Oppgave { get; set; } = null!;

        public bool ErPameldt { get; set; }

        public int AntallPameldte { get; set; }
    }
}