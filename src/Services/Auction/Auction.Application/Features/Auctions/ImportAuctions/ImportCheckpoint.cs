namespace Auctions.Application.Features.Auctions.ImportAuctions;

public record ImportCheckpoint(
    string CorrelationId,
    int LastProcessedRowIndex,
    int SucceededCount,
    int FailedCount,
    DateTimeOffset LastUpdatedAt);
