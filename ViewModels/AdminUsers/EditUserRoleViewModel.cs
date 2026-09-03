using Microsoft.AspNetCore.Mvc.Rendering;

namespace DugnadAppMvc.ViewModels.AdminUsers;

public class EditUserRoleViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Navn { get; set; } = string.Empty;

    public string Epost { get; set; } = string.Empty;

    public string SelectedRole { get; set; } = string.Empty;

    public List<SelectListItem> Roles { get; set; } = [];

    public bool ErReserveSystemadministrator { get; set; }
}