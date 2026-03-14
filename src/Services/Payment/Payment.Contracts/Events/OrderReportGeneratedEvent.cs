namespace PaymentService.Contracts.Events;

public record OrderReportGeneratedEvent
{
    public Guid CorrelationId { get; init; }
    public Guid RequestedBy { get; init; }
    public string ReportType { get; init; } = string.Empty;
    public string Format { get; init; } = string.Empty;
    public int TotalRecords { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string DownloadUrl { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
}
