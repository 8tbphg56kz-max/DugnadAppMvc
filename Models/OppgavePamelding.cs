using DugnadAppMvc.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.Models
{
    public class OppgavePamelding
    {
        public int Id { get; set; }

        public int OppgaveId { get; set; }
        public Oppgave Oppgave { get; set; } = null!;

        public int BeboerId { get; set; }
        public Beboer Beboer { get; set; } = null!;

        public DateTime PameldtDato { get; set; } = DateTime.UtcNow;

        public OppgaveStatus Status { get; set; } = OppgaveStatus.Pameldt;

        public DateTime? UtfortDato { get; set; }
    }
}