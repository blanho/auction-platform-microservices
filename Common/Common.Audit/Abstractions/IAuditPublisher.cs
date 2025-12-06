using Common.Audit.Enums;

namespace Common.Audit.Abstractions;

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
}

public interface IAuditContext
{
    Guid UserId { get; }
    string? Username { get; }
    string? CorrelationId { get; }
    string? IpAddress { get; }
}
