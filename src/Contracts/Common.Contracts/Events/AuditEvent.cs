namespace Common.Contracts.Events;

public enum AuditAction
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    SoftDeleted = 4,
    Restored = 5
}

public record AuditEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid EntityId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public AuditAction Action { get; init; }
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public List<string> ChangedProperties { get; init; } = [];
    public Guid UserId { get; init; }
    public string? Username { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public string? CorrelationId { get; init; }
    public string? IpAddress { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }
}
