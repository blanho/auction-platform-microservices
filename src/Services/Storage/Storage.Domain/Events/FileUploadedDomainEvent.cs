namespace Storage.Domain.Events;

public record FileUploadedDomainEvent(
    Guid FileId,
    string FileName,
    string ContentType,
    long FileSize,
    string Url,
    Guid? OwnerId) : DomainEvent;
