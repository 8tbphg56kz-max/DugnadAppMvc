using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class SetPasswordViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Passord")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passordene er ikke like.")]
        [Display(Name = "Bekreft passord")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}