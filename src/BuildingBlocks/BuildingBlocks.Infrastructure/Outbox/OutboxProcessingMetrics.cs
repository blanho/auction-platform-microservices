using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Outbox;

public sealed class OutboxProcessingMetrics
{
    private long _totalProcessed;
    private long _totalFailed;
    private long _totalRetried;
    private readonly ILogger<OutboxProcessingMetrics> _logger;

    public OutboxProcessingMetrics(ILogger<OutboxProcessingMetrics> logger)
    {
        _logger = logger;
    }

    public long TotalProcessed => Interlocked.Read(ref _totalProcessed);
    public long TotalFailed => Interlocked.Read(ref _totalFailed);
    public long TotalRetried => Interlocked.Read(ref _totalRetried);

    public void RecordProcessed()
    {
        Interlocked.Increment(ref _totalProcessed);
    }

    public void RecordFailed()
    {
        Interlocked.Increment(ref _totalFailed);
    }

    public void RecordRetried()
    {
        Interlocked.Increment(ref _totalRetried);
    }

    public void LogSummary()
    {
        _logger.LogInformation(
            "Outbox metrics - Processed: {Processed}, Failed: {Failed}, Retried: {Retried}",
            TotalProcessed,
            TotalFailed,
            TotalRetried);
    }
}
