namespace Storage.Domain.Events;

public record FileDeletedDomainEvent(
    Guid FileId,
    string FileName,
    Guid? OwnerId) : DomainEvent;
