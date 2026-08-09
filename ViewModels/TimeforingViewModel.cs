using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class TimeforingViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Velg dugnad.")]
        [Display(Name = "Dugnad")]
        public int DugnadId { get; set; }

        public string? DugnadNavn { get; set; }

        [Required(ErrorMessage = "Velg antall timer.")]
        [Display(Name = "Timer")]
        public string? Timer { get; set; }

        [Display(Name = "Kommentar")]
        public string? Kommentar { get; set; }

        public List<SelectListItem> Dugnader { get; set; } = [];

        public List<SelectListItem> TimerAlternativer { get; set; } = [];

        [Required(ErrorMessage = "Velg beboer.")]
        [Display(Name = "Beboer")]
        public int? BeboerId { get; set; }

        public List<SelectListItem> Beboere { get; set; } = [];
    }
}