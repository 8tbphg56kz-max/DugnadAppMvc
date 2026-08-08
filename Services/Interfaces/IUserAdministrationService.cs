using DugnadAppMvc.ViewModels.AdminUsers;

namespace DugnadAppMvc.Services.Interfaces;

public interface IUserAdministrationService
{
    Task<List<UserListViewModel>> GetUsersAsync();

    Task<EditUserRoleViewModel?> GetUserAsync(string id);

    Task<(bool Success, string? ErrorMessage)> UpdateRoleAsync(EditUserRoleViewModel model);
   

}