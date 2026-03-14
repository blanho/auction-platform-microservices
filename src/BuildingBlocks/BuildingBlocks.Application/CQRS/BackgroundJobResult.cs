namespace BuildingBlocks.Application.CQRS;

public record BackgroundJobResult(
    Guid JobId,
    string CorrelationId,
    string Status,
    string Message);
