namespace Jobs.Infrastructure.Persistence;

public class JobProgressEntry
{
    public string CorrelationId { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public int CompletedCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorMessage { get; set; }
    public bool Applied { get; set; }
}
