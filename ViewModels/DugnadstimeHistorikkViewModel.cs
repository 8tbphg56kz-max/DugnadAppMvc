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

        [Display(Name = "Dugnad")]
        public string Dugnad { get; set; } = "";

        [Display(Name = "Timer")]
        public decimal Timer { get; set; }

        [Display(Name = "Kommentar")]
        public string? Kommentar { get; set; }
    }
}