using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace DugnadAppMvc.Models
{
    public class Beboer
    {
        public int Id { get; set; }

        public string Fornavn { get; set; } = "";

        public string Etternavn { get; set; } = "";

        public string Epost { get; set; } = "";

        public bool ErAdmin { get; set; }

        public bool Aktiv { get; set; } = true;

        // Kobling til leilighet
        public int LeilighetId { get; set; }

        [ValidateNever]
        public Leilighet Leilighet { get; set; } = null!;

        // Kobling til Identity
        public string? ApplicationUserId { get; set; }

        public ApplicationUser? ApplicationUser { get; set; }

        public ICollection<Dugnadstime> Dugnadstimer { get; set; } = new List<Dugnadstime>();
    }
}