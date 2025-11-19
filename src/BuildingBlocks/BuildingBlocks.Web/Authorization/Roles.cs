namespace BuildingBlocks.Web.Authorization;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Seller = "Seller";
    public const string User = "User";

    public const string AdminOrSeller = $"{Admin},{Seller}";
    public const string All = $"{Admin},{Seller},{User}";

    public static readonly IReadOnlyList<string> AllRoles = [Admin, Seller, User];
}

public static class AppRoles
{
    public const string Admin = Roles.Admin;
    public const string Seller = Roles.Seller;
    public const string User = Roles.User;
    public const string AdminOrSeller = Roles.AdminOrSeller;
    public const string AllAuthenticated = Roles.All;
    public static readonly IReadOnlyList<string> AllRoles = Roles.AllRoles;
}
