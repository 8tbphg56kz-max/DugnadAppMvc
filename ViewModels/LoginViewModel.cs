using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";
}