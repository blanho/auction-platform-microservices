namespace Identity.Api.DTOs.Audit;

public record UserAuditData
{
    public required string UserId { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? FullName { get; init; }
    public bool IsActive { get; init; }
    public bool IsSuspended { get; init; }
    public string? SuspensionReason { get; init; }
    public DateTimeOffset? SuspendedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public List<string>? Roles { get; init; }

    public static UserAuditData FromUser(Models.ApplicationUser user, IList<string>? roles = null)
    {
        return new UserAuditData
        {
            UserId = user.Id,
            Username = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            IsSuspended = user.IsSuspended,
            SuspensionReason = user.SuspensionReason,
            SuspendedAt = user.SuspendedAt,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles?.ToList()
        };
    }
}

public record TwoFactorAuditData
{
    public required string UserId { get; init; }
    public string? Username { get; init; }
    public required string Action { get; init; }
    public bool IsEnabled { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

public record AuthAuditData
{
    public required string UserId { get; init; }
    public string? Username { get; init; }
    public required string Action { get; init; }
    public string? IpAddress { get; init; }
    public bool Success { get; init; }
    public string? FailureReason { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
