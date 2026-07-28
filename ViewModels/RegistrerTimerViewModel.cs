using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class RegistrerTimerViewModel
    {
        public int OppgaveId { get; set; }

        public string OppgaveNavn { get; set; } = string.Empty;

        [Display(Name = "Antall timer")]
        [Required(ErrorMessage = "Velg antall timer.")]
        public string AntallTimer { get; set; } = string.Empty;

        public List<SelectListItem> TimerAlternativer { get; set; } = new();

        [Display(Name = "Kommentar")]
        [StringLength(500)]
        public string? Kommentar { get; set; }
    }
}