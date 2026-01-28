namespace BuildingBlocks.Application.Abstractions.Auditing;

public enum AuditAction
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    SoftDeleted = 4,
    Restored = 5
}

public interface IAuditPublisher
{
    Task PublishAsync<T>(
        Guid entityId,
        T entity,
        AuditAction action,
        T? oldEntity = default,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default) where T : class;

    Task PublishBatchAsync<T>(
        IEnumerable<(Guid EntityId, T Entity)> entities,
        AuditAction action,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default) where T : class;

    Task PublishEntriesAsync(
        IEnumerable<AuditEntry> entries,
        CancellationToken cancellationToken = default);
}

public interface IAuditContext
{
    Guid UserId { get; }
    string? Username { get; }
    string? CorrelationId { get; }
    string? IpAddress { get; }
}

