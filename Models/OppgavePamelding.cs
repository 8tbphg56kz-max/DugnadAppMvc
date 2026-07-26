using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.Models
{
    public class OppgavePamelding
    {
        public int Id { get; set; }

        [Required]
        public int OppgaveId { get; set; }
        public Oppgave Oppgave { get; set; } = null!;

        [Required]
        public int BeboerId { get; set; }
        public Beboer Beboer { get; set; } = null!;

        public DateTime PameldtDato { get; set; } = DateTime.UtcNow;

        public bool ErAktiv { get; set; } = true;
    }
}