using System.ComponentModel.DataAnnotations.Schema;
using BuildingBlocks.Domain.Events;

namespace BuildingBlocks.Domain.Entities;

public abstract class BaseEntity : IAuditableEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; }
    public Guid CreatedBy { get; protected set; }
    public DateTimeOffset? UpdatedAt { get; protected set; }
    public Guid UpdatedBy { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public DateTimeOffset? DeletedAt { get; protected set; }
    public Guid DeletedBy { get; protected set; }

    public void SetCreatedAudit(Guid userId, DateTimeOffset timestamp)
    {
        CreatedBy = userId;
        CreatedAt = timestamp;
        IsDeleted = false;
    }

    public void SetUpdatedAudit(Guid userId, DateTimeOffset timestamp)
    {
        UpdatedBy = userId;
        UpdatedAt = timestamp;
    }

    public void MarkAsDeleted(Guid userId, DateTimeOffset timestamp)
    {
        IsDeleted = true;
        DeletedBy = userId;
        DeletedAt = timestamp;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = Guid.Empty;
    }

    private readonly List<IDomainEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return Id != Guid.Empty && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !Equals(left, right);
    }
}
