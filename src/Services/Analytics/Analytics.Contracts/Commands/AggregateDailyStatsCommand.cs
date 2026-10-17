namespace AnalyticsService.Contracts.Commands;

public record AggregateDailyStatsCommand
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public Guid RequestedBy { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid? ParentJobId { get; init; }
    public Guid? ParentJobItemId { get; init; }
    public int Attempt { get; init; }
}
