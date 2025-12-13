namespace Common.Security.Models;

public sealed record CurrentUser
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string[] Roles { get; init; } = Array.Empty<string>();
    public Dictionary<string, string> Claims { get; init; } = new();
}
