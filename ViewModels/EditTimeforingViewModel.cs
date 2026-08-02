using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class EditTimeforingViewModel
    {
        public int Id { get; set; }

        public string Dugnad { get; set; } = string.Empty;

        public decimal? Timer { get; set; }

        public string? Kommentar { get; set; }

        public List<SelectListItem> TimerAlternativer { get; set; } = new();
    }
}