namespace Common.Core.Authorization;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Seller = "Seller";
    public const string User = "User";

    public const string AdminOrSeller = "Admin,Seller";
    public const string AllAuthenticated = "Admin,Seller,User";

    public static readonly IReadOnlyList<string> AllRoles = new[] { Admin, Seller, User };
}
