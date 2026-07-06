using DugnadAppMvc.Models;

namespace DugnadAppMvc.Services;

public class UserProvisioningResult
{
    public ApplicationUser User { get; set; } = null!;

    public bool IsNewUser { get; set; }

    public string? ResetPasswordToken { get; set; }
    public string? ActivationLink { get; set; }
}
