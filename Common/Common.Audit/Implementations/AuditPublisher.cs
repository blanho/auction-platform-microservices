using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Audit.Abstractions;
using Common.Audit.Configuration;
using Common.Audit.Enums;
using Common.Audit.Events;
using Common.Messaging.Abstractions;
using Microsoft.Extensions.Options;

namespace Common.Audit.Implementations;


public class AuditPublisher : IAuditPublisher
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IAuditContext _auditContext;
    private readonly AuditOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuditPublisher(
        IEventPublisher eventPublisher,
        IAuditContext auditContext,
        IOptions<AuditOptions> options)
    {
        _eventPublisher = eventPublisher;
        _auditContext = auditContext;
        _options = options.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 32
        };
    }

    public async Task PublishAsync<T>(
        Guid entityId,
        T entity,
        AuditAction action,
        T? oldEntity = default,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (!_options.Enabled) return;

        var entityType = typeof(T).Name;
        if (_options.ExcludedEntities.Contains(entityType)) return;

        var changedProperties = GetChangedProperties(oldEntity, entity);
        var newValues = SerializeEntity(entity);
        var oldValues = oldEntity != null ? SerializeEntity(oldEntity) : null;

        var auditEvent = new AuditEvent
        {
            EntityId = entityId,
            EntityType = entityType,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            ChangedProperties = changedProperties,
            UserId = _auditContext.UserId,
            Username = _auditContext.Username,
            ServiceName = _options.ServiceName,
            CorrelationId = _auditContext.CorrelationId,
            IpAddress = _auditContext.IpAddress,
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = metadata
        };

        await _eventPublisher.PublishAsync(auditEvent, cancellationToken);
    }

    private string SerializeEntity<T>(T entity) where T : class
    {
        var dict = new Dictionary<string, object?>();
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            if (_options.ExcludedProperties.Contains(prop.Name)) continue;
            
            var value = prop.GetValue(entity);
            dict[prop.Name] = value;
        }

        return JsonSerializer.Serialize(dict, _jsonOptions);
    }

    private List<string> GetChangedProperties<T>(T? oldEntity, T newEntity) where T : class
    {
        var changedProperties = new List<string>();
        if (oldEntity == null) return changedProperties;

        var properties = typeof(T).GetProperties();
        foreach (var prop in properties)
        {
            if (_options.ExcludedProperties.Contains(prop.Name)) continue;

            var oldValue = prop.GetValue(oldEntity);
            var newValue = prop.GetValue(newEntity);

            if (!Equals(oldValue, newValue))
            {
                changedProperties.Add(prop.Name);
            }
        }

        return changedProperties;
    }

    public async Task PublishBatchAsync<T>(
        IEnumerable<(Guid EntityId, T Entity)> entities,
        AuditAction action,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (!_options.Enabled) return;

        var entityType = typeof(T).Name;
        if (_options.ExcludedEntities.Contains(entityType)) return;

        var auditEvents = entities.Select(e => new AuditEvent
        {
            EntityId = e.EntityId,
            EntityType = entityType,
            Action = action,
            OldValues = null,
            NewValues = SerializeEntity(e.Entity),
            ChangedProperties = new List<string>(),
            UserId = _auditContext.UserId,
            Username = _auditContext.Username,
            ServiceName = _options.ServiceName,
            CorrelationId = _auditContext.CorrelationId,
            IpAddress = _auditContext.IpAddress,
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = metadata
        }).ToList();

        await _eventPublisher.PublishBatchAsync(auditEvents, cancellationToken);
    }
}
