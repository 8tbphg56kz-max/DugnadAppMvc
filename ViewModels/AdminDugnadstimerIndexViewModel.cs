using Microsoft.AspNetCore.Mvc.Rendering;

namespace DugnadAppMvc.ViewModels
{
    public class AdminDugnadstimerIndexViewModel
    {
        public int? LeilighetId { get; set; }

        public List<SelectListItem> Leiligheter { get; set; } = new();

        public int? DugnadId { get; set; }

        public List<SelectListItem> Dugnader { get; set; } = new();

        public List<AdminDugnadstimeViewModel> Dugnadstimer { get; set; } = new();

        public int? BeboerId { get; set; }

        public List<SelectListItem> Beboere { get; set; } = new();
    }
}