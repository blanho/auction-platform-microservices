namespace Common.Core.Constants;

public static class AppRoles
{
    public const string Admin = "admin";
    public const string Seller = "seller";
    public const string User = "user";

    public const string AdminOrSeller = $"{Admin},{Seller}";
    public const string AllAuthenticated = $"{Admin},{Seller},{User}";
}
