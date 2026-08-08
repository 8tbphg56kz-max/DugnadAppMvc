using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class AddAdministratorViewModel
{
    [Display(Name = "Beboer")]
    public string SelectedUserId { get; set; } = string.Empty;

    [Display(Name = "Rolle")]
    public string SelectedRole { get; set; } = string.Empty;

    public List<SelectListItem> Users { get; set; } = [];

    public List<SelectListItem> Roles { get; set; } = [];
}