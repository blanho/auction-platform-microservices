namespace PaymentService.Contracts.Commands;

public record GenerateOrderReportCommand
{
    public Guid? ParentJobId { get; init; }
    public Guid? ParentJobItemId { get; init; }
    public int Attempt { get; init; }
    public string? Culture { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid RequestedBy { get; init; }
    public string ReportType { get; init; } = string.Empty;
    public string Format { get; init; } = string.Empty;
    public string? StatusFilter { get; init; }
    public Guid? BuyerIdFilter { get; init; }
    public Guid? SellerIdFilter { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public DateTimeOffset RequestedAt { get; init; }
}
