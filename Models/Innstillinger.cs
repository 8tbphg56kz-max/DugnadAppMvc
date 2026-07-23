using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.Models
{
    public class Innstillinger
    {
        public int Id { get; set; }

        [Display(Name = "Sameiets navn")]
        [StringLength(100)]
        public string SameieNavn { get; set; } = string.Empty;

        [Display(Name = "Dugnadsbudsjett")]
        [Range(0, int.MaxValue)]
        public int Dugnadsbudsjett { get; set; }
    }
}