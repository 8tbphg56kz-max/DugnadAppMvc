using System.Security.Claims;

namespace DugnadAppMvc.Infrastructure.Identity;

public static class ClaimsPrincipalExtensions
{
    public static bool HasBoardAccess(this ClaimsPrincipal user)
    {
        return user.IsInRole(IdentityRoles.Styremedlem)
            || user.IsInRole(IdentityRoles.Administrator)
            || user.IsInRole(IdentityRoles.SystemAdministrator);
    }

    public static bool HasAdminAccess(this ClaimsPrincipal user)
    {
        return user.IsInRole(IdentityRoles.Administrator)
            || user.IsInRole(IdentityRoles.SystemAdministrator);
    }    

    public static bool IsSystemAdministrator(this ClaimsPrincipal user)
    {
        return user.IsInRole(IdentityRoles.SystemAdministrator);
    }
}