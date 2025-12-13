#nullable enable

namespace Common.Security.Interfaces;

public interface IUserContext
{
    string? UserId { get; }
    string? Email { get; }
    string[] Roles { get; }
    bool IsAuthenticated { get; }
}
