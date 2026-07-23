namespace DugnadAppMvc.Infrastructure.Identity;

public static class IdentityRoles
{
    public const string SystemAdministrator = nameof(SystemAdministrator);
    public const string Administrator = nameof(Administrator);
    public const string Styremedlem = nameof(Styremedlem);
    public const string Beboer = nameof(Beboer);
    public const string BoardAccess = $"{Styremedlem},{Administrator},{SystemAdministrator}";
    public const string AdminAccess = $"{Administrator},{SystemAdministrator}";
    public static readonly string[] All =
    [
        SystemAdministrator,
        Administrator,
        Styremedlem,
        Beboer
    ];
}