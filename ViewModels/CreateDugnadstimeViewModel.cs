using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class CreateTimeforingViewModel
    {
        [Display(Name = "Dato")]
        [DataType(DataType.Date)]
        public DateTime Dato { get; set; } = DateTime.Today;

        [Display(Name = "Timer")]
        [Range(0.5, 24)]
        public decimal Timer { get; set; }

        [Display(Name = "Kommentar")]
        public string? Kommentar { get; set; }

        public List<SelectListItem> TimerAlternativer { get; set; } = [];
    }
}