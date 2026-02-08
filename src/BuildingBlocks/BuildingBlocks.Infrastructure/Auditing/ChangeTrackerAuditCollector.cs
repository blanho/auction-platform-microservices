using System.Text.Json;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BuildingBlocks.Infrastructure.Auditing;

public class ChangeTrackerAuditCollector : IAuditCollector
{
    private readonly List<AuditEntry> _pendingAudits = [];

    public IReadOnlyList<AuditEntry> GetPendingAudits() => _pendingAudits.AsReadOnly();

    public void Clear() => _pendingAudits.Clear();

    public void CollectAudits(DbContext context)
    {
        var entries = context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IAuditableEntity &&
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var auditEntry = CreateAuditEntry(entry);
            if (auditEntry != null)
            {
                _pendingAudits.Add(auditEntry);
            }
        }
    }

    private static AuditEntry? CreateAuditEntry(EntityEntry entry)
    {
        if (entry.Entity is not IAuditableEntity auditableEntity)
            return null;

        var entityType = entry.Entity.GetType();
        var action = MapStateToAction(entry.State, entry.Entity);

        return entry.State switch
        {
            EntityState.Added => new AuditEntry
            {
                EntityId = auditableEntity.Id,
                EntityType = entityType.Name,
                Action = action,
                OldValues = null,
                NewValues = GetCurrentValues(entry),
                ChangedProperties = GetAllPropertyNames(entry)
            },
            EntityState.Modified => new AuditEntry
            {
                EntityId = auditableEntity.Id,
                EntityType = entityType.Name,
                Action = action,
                OldValues = GetOriginalValues(entry),
                NewValues = GetCurrentValues(entry),
                ChangedProperties = GetChangedPropertyNames(entry)
            },
            EntityState.Deleted => new AuditEntry
            {
                EntityId = auditableEntity.Id,
                EntityType = entityType.Name,
                Action = AuditAction.Deleted,
                OldValues = GetOriginalValues(entry),
                NewValues = null,
                ChangedProperties = []
            },
            _ => null
        };
    }

    private static AuditAction MapStateToAction(EntityState state, object entity)
    {
        if (state == EntityState.Modified && entity is BaseEntity baseEntity && baseEntity.IsDeleted)
            return AuditAction.SoftDeleted;

        return state switch
        {
            EntityState.Added => AuditAction.Created,
            EntityState.Modified => AuditAction.Updated,
            EntityState.Deleted => AuditAction.Deleted,
            _ => AuditAction.Updated
        };
    }

    private static Dictionary<string, object?> GetCurrentValues(EntityEntry entry)
    {
        return entry.CurrentValues.Properties
            .Where(property => ShouldAuditProperty(property.Name))
            .ToDictionary(property => property.Name, property => entry.CurrentValues[property]);
    }

    private static Dictionary<string, object?> GetOriginalValues(EntityEntry entry)
    {
        return entry.OriginalValues.Properties
            .Where(property => ShouldAuditProperty(property.Name))
            .ToDictionary(property => property.Name, property => entry.OriginalValues[property]);
    }

    private static List<string> GetAllPropertyNames(EntityEntry entry)
    {
        return entry.CurrentValues.Properties
            .Where(p => ShouldAuditProperty(p.Name))
            .Select(p => p.Name)
            .ToList();
    }

    private static List<string> GetChangedPropertyNames(EntityEntry entry)
    {
        var changedProperties = new List<string>();

        foreach (var property in entry.OriginalValues.Properties)
        {
            if (!ShouldAuditProperty(property.Name))
                continue;

            var originalValue = entry.OriginalValues[property];
            var currentValue = entry.CurrentValues[property];

            if (!Equals(originalValue, currentValue))
            {
                changedProperties.Add(property.Name);
            }
        }

        return changedProperties;
    }

    private static readonly HashSet<string> ExcludedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "DomainEvents",
        "RowVersion",
        "ConcurrencyStamp"
    };

    private static bool ShouldAuditProperty(string propertyName)
        => !ExcludedProperties.Contains(propertyName);
}
