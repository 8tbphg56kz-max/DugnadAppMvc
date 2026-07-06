using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class ActivateViewModel
    {
        public string UserId { get; set; } = "";

        public string Token { get; set; } = "";

        [Required(ErrorMessage = "Passord må fylles ut.")]
        [DataType(DataType.Password)]
        [Display(Name = "Passord")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Bekreft passord.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passordene er ikke like.")]
        [Display(Name = "Bekreft passord")]
        public string ConfirmPassword { get; set; } = "";
    }
}