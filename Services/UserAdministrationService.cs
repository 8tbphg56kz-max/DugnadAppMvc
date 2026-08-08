using DugnadAppMvc.Models;
using DugnadAppMvc.Services.Interfaces;
using DugnadAppMvc.ViewModels.AdminUsers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Services;

public class UserAdministrationService : IUserAdministrationService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAdministrationService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<List<UserListViewModel>> GetUsersAsync()
    {
        var users = _userManager.Users.ToList();

        var model = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roller = await _userManager.GetRolesAsync(user);

            // Finn den administrative rollen
            var rolle = roller.FirstOrDefault(r => r != IdentityRoles.Beboer);

            if (rolle is null)
                continue;

            model.Add(new UserListViewModel
            {
                Id = user.Id,
                Navn = $"{user.FirstName} {user.LastName}",
                Epost = user.Email ?? "",
                Rolle = rolle
            });
        }

        return model;
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateRoleAsync(EditUserRoleViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);

        if (user == null)
            return (false, "Brukeren ble ikke funnet.");

        var currentRoles = await _userManager.GetRolesAsync(user);

        // Beskytt siste SystemAdministrator
        if (currentRoles.Contains(IdentityRoles.SystemAdministrator) &&
            model.SelectedRole != IdentityRoles.SystemAdministrator)
        {
            var admins = await _userManager.GetUsersInRoleAsync(
                IdentityRoles.SystemAdministrator);

            if (admins.Count == 1)
            {
                return (false, "Det må alltid finnes minst én SystemAdministrator.");
            }
        }

        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!removeResult.Succeeded)
            return (false, "Kunne ikke fjerne eksisterende rolle.");

        var addResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);

        if (!addResult.Succeeded)
            return (false, "Kunne ikke legge til ny rolle.");

        return (true, null);
    }

    public async Task<EditUserRoleViewModel?> GetUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new EditUserRoleViewModel
        {
            Id = user.Id,
            Navn = $"{user.FirstName} {user.LastName}",
            Epost = user.Email ?? "",
            SelectedRole = roles.FirstOrDefault() ?? IdentityRoles.Beboer,

            Roles = new List<SelectListItem>
    {
        new() { Value = IdentityRoles.Styremedlem, Text = "Styremedlem" },
        new() { Value = IdentityRoles.Administrator, Text = "Administrator" },
        new() { Value = IdentityRoles.SystemAdministrator, Text = "Systemadministrator" }
    }
        };
    }
}