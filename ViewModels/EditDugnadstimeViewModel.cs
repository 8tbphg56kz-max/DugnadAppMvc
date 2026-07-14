using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class EditDugnadstimeViewModel
    {
        public int Id { get; set; }

        public string Dugnad { get; set; } = "";

        [Required(ErrorMessage = "Velg antall timer.")]
        [Display(Name = "Timer")]
        public decimal? Timer { get; set; }

        [Display(Name = "Kommentar")]
        public string? Kommentar { get; set; }

        public List<SelectListItem> TimerAlternativer { get; set; } = [];
    }
}