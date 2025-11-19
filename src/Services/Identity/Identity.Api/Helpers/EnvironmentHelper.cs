namespace Identity.Api.Helpers;

public static class EnvironmentHelper
{
    public static bool IsProduction(IConfiguration configuration) =>
        !string.Equals(
            configuration["ASPNETCORE_ENVIRONMENT"],
            "Development",
            StringComparison.OrdinalIgnoreCase);

    public static bool IsDevelopment(IConfiguration configuration) =>
        string.Equals(
            configuration["ASPNETCORE_ENVIRONMENT"],
            "Development",
            StringComparison.OrdinalIgnoreCase);

    public static string GetEnvironment(IConfiguration configuration) =>
        configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
}
