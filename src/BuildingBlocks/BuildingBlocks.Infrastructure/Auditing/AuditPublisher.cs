using System.Text.Json;
using BuildingBlocks.Application.Abstractions.Auditing;
using Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

using AppAuditAction = BuildingBlocks.Application.Abstractions.Auditing.AuditAction;
using ContractAuditAction = Common.Contracts.Events.AuditAction;

namespace BuildingBlocks.Infrastructure.Auditing;

public class AuditPublisher : IAuditPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IAuditContext _auditContext;
    private readonly ILogger<AuditPublisher> _logger;
    private readonly string _serviceName;

    public AuditPublisher(
        IPublishEndpoint publishEndpoint,
        IAuditContext auditContext,
        ILogger<AuditPublisher> logger,
        string serviceName)
    {
        _publishEndpoint = publishEndpoint;
        _auditContext = auditContext;
        _logger = logger;
        _serviceName = serviceName;
    }

    public async Task PublishAsync<T>(
        Guid entityId,
        T entity,
        AppAuditAction action,
        T? oldEntity = default,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var changedProperties = GetChangedProperties(entity, oldEntity);

        var auditEvent = new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityId = entityId,
            EntityType = typeof(T).Name,
            Action = MapToContractAction(action),
            OldValues = oldEntity != null ? SerializeEntity(oldEntity) : null,
            NewValues = SerializeEntity(entity),
            ChangedProperties = changedProperties,
            UserId = _auditContext.UserId,
            Username = _auditContext.Username,
            ServiceName = _serviceName,
            CorrelationId = _auditContext.CorrelationId,
            IpAddress = _auditContext.IpAddress,
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = metadata
        };

        await _publishEndpoint.Publish(auditEvent, cancellationToken);

        _logger.LogDebug(
            "Published audit event: {Action} on {EntityType} ({EntityId}) by user {UserId}",
            action, typeof(T).Name, entityId, _auditContext.UserId);
    }

    public async Task PublishBatchAsync<T>(
        IEnumerable<(Guid EntityId, T Entity)> entities,
        AppAuditAction action,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var auditEvents = entities.Select(e => new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityId = e.EntityId,
            EntityType = typeof(T).Name,
            Action = MapToContractAction(action),
            OldValues = null,
            NewValues = SerializeEntity(e.Entity),
            ChangedProperties = [],
            UserId = _auditContext.UserId,
            Username = _auditContext.Username,
            ServiceName = _serviceName,
            CorrelationId = _auditContext.CorrelationId,
            IpAddress = _auditContext.IpAddress,
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = metadata
        }).ToList();

        await _publishEndpoint.PublishBatch(auditEvents, cancellationToken);

        _logger.LogDebug(
            "Published {Count} audit events for {EntityType} by user {UserId}",
            auditEvents.Count, typeof(T).Name, _auditContext.UserId);
    }

    private static ContractAuditAction MapToContractAction(AppAuditAction action)
    {
        return action switch
        {
            AppAuditAction.Created => ContractAuditAction.Created,
            AppAuditAction.Updated => ContractAuditAction.Updated,
            AppAuditAction.Deleted => ContractAuditAction.Deleted,
            AppAuditAction.SoftDeleted => ContractAuditAction.SoftDeleted,
            AppAuditAction.Restored => ContractAuditAction.Restored,
            _ => ContractAuditAction.Updated
        };
    }

    private static string? SerializeEntity<T>(T entity) where T : class
    {
        try
        {
            return JsonSerializer.Serialize(entity, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            return null;
        }
    }

    private static List<string> GetChangedProperties<T>(T newEntity, T? oldEntity) where T : class
    {
        if (oldEntity == null)
            return [];

        var changedProperties = new List<string>();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            try
            {
                var oldValue = property.GetValue(oldEntity);
                var newValue = property.GetValue(newEntity);

                if (!Equals(oldValue, newValue))
                    changedProperties.Add(property.Name);
            }
            catch
            {
            }
        }

        return changedProperties;
    }
}
