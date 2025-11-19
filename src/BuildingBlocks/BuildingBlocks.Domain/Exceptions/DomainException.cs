namespace BuildingBlocks.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class DomainInvariantException : DomainException
{
    public DomainInvariantException(string message) : base(message) { }
}

public sealed class InvalidEntityStateException : DomainException
{
    public string EntityName { get; }
    public string StateName { get; }

    public InvalidEntityStateException(string entityName, string stateName, string message)
        : base(message)
    {
        EntityName = entityName;
        StateName = stateName;
    }
}

public sealed class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string entityName, object? entityId = null)
        : base($"{entityName} not found{(entityId != null ? $" with id '{entityId}'" : "")}")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}

public sealed class DomainConflictException : DomainException
{
    public DomainConflictException(string message) : base(message) { }
}
