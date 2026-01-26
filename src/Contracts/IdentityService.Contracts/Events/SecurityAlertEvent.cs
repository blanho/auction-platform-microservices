namespace IdentityService.Contracts.Events;

public record SecurityAlertEvent : IVersionedEvent
{
    public int Version => 1;

    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string AlertType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}

public static class SecurityAlertTypes
{
    public const string TokenTheftDetected = "token_theft_detected";
    public const string SuspiciousLogin = "suspicious_login";
    public const string MultipleFailedLogins = "multiple_failed_logins";
    public const string PasswordChanged = "password_changed";
    public const string SessionsRevoked = "sessions_revoked";
}
