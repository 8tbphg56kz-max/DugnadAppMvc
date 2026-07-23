using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.Models.ViewModels
{
    public class SetupViewModel
    {
        [Required]
        public string Fornavn { get; set; } = "";

        [Required]
        public string Etternavn { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Epost { get; set; } = "";

        [Required]
        public string Leilighet { get; set; } = "";
    }
}