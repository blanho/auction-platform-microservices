namespace Analytics.Api.Entities;

public class FactUser
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset EventTime { get; set; }
    public DateTimeOffset IngestedAt { get; set; }
    public DateOnly DateKey { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string? FullName { get; set; }

    public string EventType { get; set; } = string.Empty;
    public short EventVersion { get; set; }
}
