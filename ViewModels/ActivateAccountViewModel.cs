using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class ActivateAccountViewModel
    {
        [Required(ErrorMessage = "E-post er påkrevd.")]
        [EmailAddress(ErrorMessage = "Ugyldig e-postadresse.")]
        public string Email { get; set; } = string.Empty;
    }
}