using Microsoft.AspNetCore.Mvc.Rendering;

namespace DugnadAppMvc.ViewModels
{
    public class AdminTimeforingerIndexViewModel
    {
        public int? LeilighetId { get; set; }

        public List<SelectListItem> Leiligheter { get; set; } = new();

        public string? Aktivitet { get; set; }

        public List<SelectListItem> Aktiviteter { get; set; } = new();

        public List<AdminTimeforingViewModel> Dugnadstimer { get; set; } = new();

        public int? BeboerId { get; set; }

        public List<SelectListItem> Beboere { get; set; } = new();
    }
}