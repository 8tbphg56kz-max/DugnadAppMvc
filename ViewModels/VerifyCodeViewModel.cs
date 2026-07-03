using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class VerifyCodeViewModel
    {
        public string Epost { get; set; } = "";

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Kode { get; set; } = "";
    }
}