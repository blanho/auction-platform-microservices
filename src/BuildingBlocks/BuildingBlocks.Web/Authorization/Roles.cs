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
