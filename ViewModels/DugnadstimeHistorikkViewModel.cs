using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class DugnadstimeHistorikkViewModel
    {
        public int Id { get; set; }

        public bool KanRedigeres { get; set; }
        public bool KanSlettes { get; set; }

        [Display(Name = "Registrert")]
        public DateTime Registrert { get; set; }

        // Midlertidig - beholdes til viewet er oppdatert
        [Display(Name = "Dugnad")]
        public string Dugnad { get; set; } = "";

        [Display(Name = "Type")]
        public string Type { get; set; } = "";

        [Display(Name = "Aktivitet")]
        public string Aktivitet { get; set; } = "";

        [Display(Name = "Timer")]
        public decimal Timer { get; set; }

        [Display(Name = "Kommentar")]
        public string? Kommentar { get; set; }
    }
}