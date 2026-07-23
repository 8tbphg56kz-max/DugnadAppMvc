namespace DugnadAppMvc.ViewModels.AdminUsers;

public class UserListViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Navn { get; set; } = string.Empty;

    public string Epost { get; set; } = string.Empty;

    public bool ErAktiv { get; set; }

    public string Rolle { get; set; } = string.Empty;
}