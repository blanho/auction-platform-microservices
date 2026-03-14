using Common.Contracts.Events;

namespace AuctionService.Contracts.Events;

public record AuctionExportCompletedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid CorrelationId { get; init; }
    public Guid RequestedBy { get; init; }
    public string Format { get; init; } = string.Empty;
    public int TotalRecords { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string? DownloadUrl { get; init; }
    public TimeSpan Duration { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
}
