using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class AdminCreateTimeforingViewModel
    {
        [Required(ErrorMessage = "Velg aktivitet.")]
        [Display(Name = "Aktivitet")]
        public string Aktivitet { get; set; } = string.Empty;

        public List<SelectListItem> Aktiviteter { get; set; } = [];

        [Required(ErrorMessage = "Velg antall timer.")]
        [Display(Name = "Timer")]
        public string? Timer { get; set; }

        [Display(Name = "Kommentar")]
        public string? Kommentar { get; set; }

        [Required(ErrorMessage = "Begrunnelse er påkrevd.")]
        [Display(Name = "Begrunnelse")]
        public string? Begrunnelse { get; set; }

        public List<SelectListItem> TimerAlternativer { get; set; } = [];

        [Required(ErrorMessage = "Velg beboer.")]
        [Display(Name = "Beboer")]
        public int? BeboerId { get; set; }

        public List<SelectListItem> Beboere { get; set; } = [];
    }
}