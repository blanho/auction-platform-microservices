using BuildingBlocks.Application.Paging;

namespace Identity.Api.DTOs.Users;

public class UserFilter
{
    public string? Search { get; init; }
    public string? Role { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsSuspended { get; init; }
}

public class GetUsersQuery : QueryParameters<UserFilter> { }

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
}

public class AdminUserDto : UserDto
{
    public bool IsActive { get; set; }
    public bool IsSuspended { get; set; }
    public string? SuspensionReason { get; set; }
}

public class AdminStatsResponse
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int SuspendedUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
}

public class SuspendUserRequest
{
    public required string Reason { get; set; }
}

public class UpdateUserRolesRequest
{
    public required List<string> Roles { get; set; }
}

