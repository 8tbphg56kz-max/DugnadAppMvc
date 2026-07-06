using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-post må fylles ut.")]
    [EmailAddress(ErrorMessage = "Ugyldig e-postadresse.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Passord må fylles ut.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; } = true;
}