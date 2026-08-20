using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class AdminTimeforingViewModel
    {
        public int Id { get; set; }

        public DateTime Registrert { get; set; }

        public string Aktivitet { get; set; } = "";

        public string Beboer { get; set; } = "";

        public decimal Timer { get; set; }

        public string? Kommentar { get; set; }

        [Required(ErrorMessage = "Du må oppgi en begrunnelse for slettingen.")]
        [Display(Name = "Begrunnelse")]
        public string? Begrunnelse { get; set; }
    }
}