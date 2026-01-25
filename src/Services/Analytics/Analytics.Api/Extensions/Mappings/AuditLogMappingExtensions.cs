using System.Text.Json;
using Analytics.Api.Entities;
using Analytics.Api.Models;

namespace Analytics.Api.Extensions.Mappings;

public static class AuditLogMappingExtensions
{
    public static AuditLogDto ToDto(this AuditLog entity)
    {
        return new AuditLogDto
        {
            Id = entity.Id,
            EntityId = entity.EntityId,
            EntityType = entity.EntityType,
            Action = entity.Action,
            OldValues = entity.OldValues,
            NewValues = entity.NewValues,
            ChangedProperties = !string.IsNullOrEmpty(entity.ChangedProperties)
                ? JsonSerializer.Deserialize<List<string>>(entity.ChangedProperties)
                : null,
            UserId = entity.UserId,
            Username = entity.Username,
            ServiceName = entity.ServiceName,
            CorrelationId = entity.CorrelationId,
            IpAddress = entity.IpAddress,
            Timestamp = entity.Timestamp
        };
    }

    public static List<AuditLogDto> ToDtoList(this IEnumerable<AuditLog> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
}
