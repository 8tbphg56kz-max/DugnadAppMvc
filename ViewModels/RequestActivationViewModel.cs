using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels;

public class RequestActivationViewModel
{
    [Required(ErrorMessage = "E-post må fylles ut.")]
    [EmailAddress(ErrorMessage = "Ugyldig e-postadresse.")]
    public string Email { get; set; } = "";
}