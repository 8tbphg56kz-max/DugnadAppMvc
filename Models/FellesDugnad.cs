using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.Models
{
    public class FellesDugnad
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Tittel { get; set; } = "";

        [StringLength(1000)]
        public string? Beskrivelse { get; set; }

        [Required]
        public DateTime StartTid { get; set; }

        public DateTime? SluttTid { get; set; }

        [StringLength(100)]
        public string? Oppmotested { get; set; }

        public bool ErAvlyst { get; set; }
    }
}