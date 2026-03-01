using AuctionService.Contracts.Commands;
using BuildingBlocks.Application.CQRS;
using MassTransit;

namespace Auctions.Application.Commands.QueueAuctionExport;

public class QueueAuctionExportCommandHandler : ICommandHandler<QueueAuctionExportCommand, BackgroundJobResult>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<QueueAuctionExportCommandHandler> _logger;

    public QueueAuctionExportCommandHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<QueueAuctionExportCommandHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<BackgroundJobResult>> Handle(
        QueueAuctionExportCommand request,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();

        var command = new ProcessAuctionExportCommand
        {
            CorrelationId = correlationId,
            RequestedBy = request.RequestedBy,
            Format = request.Format.ToString(),
            StatusFilter = request.StatusFilter?.ToString(),
            SellerFilter = request.SellerFilter,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RequestedAt = DateTimeOffset.UtcNow
        };

        await _publishEndpoint.Publish(command, cancellationToken);

        _logger.LogInformation(
            "Queued auction export job {CorrelationId} in {Format} format for user {RequestedBy}",
            correlationId, request.Format, request.RequestedBy);

        return Result<BackgroundJobResult>.Success(new BackgroundJobResult(
            JobId: correlationId,
            CorrelationId: correlationId.ToString(),
            Status: "Queued",
            Message: $"Export in {request.Format} format has been queued for background processing."));
    }
}
