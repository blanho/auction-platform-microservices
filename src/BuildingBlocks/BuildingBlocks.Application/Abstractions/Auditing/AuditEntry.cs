namespace BuildingBlocks.Application.Abstractions.Auditing;

public class AuditEntry
{
    public Guid EntityId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public AuditAction Action { get; init; }
    public object? OldValues { get; init; }
    public object? NewValues { get; init; }
    public List<string> ChangedProperties { get; init; } = [];
    public Dictionary<string, object>? Metadata { get; init; }
}
