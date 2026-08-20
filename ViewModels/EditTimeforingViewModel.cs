using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class EditTimeforingViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Aktivitet")]
        public string Aktivitet { get; set; } = string.Empty;

        [Required(ErrorMessage = "Velg beboer.")]
        [Display(Name = "Beboer")]
        public int? BeboerId { get; set; }

        [Required(ErrorMessage = "Velg antall timer.")]
        [Display(Name = "Timer")]

        public string? Timer { get; set; }
        [Display(Name = "Kommentar")]
        public string? Kommentar { get; set; }

        [Required(ErrorMessage = "Du må oppgi en begrunnelse for endringen.")]
        [Display(Name = "Begrunnelse")]
        public string? Begrunnelse { get; set; }

        public List<SelectListItem> Beboere { get; set; } = [];

        public List<SelectListItem> TimerAlternativer { get; set; } = [];
    }
}