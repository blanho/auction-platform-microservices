#nullable enable

namespace Storage.Domain.Events;

public record FileUploadFailedEvent
{
    public Guid FileId { get; init; }
    public string OwnerService { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset FailedAt { get; init; }
}
