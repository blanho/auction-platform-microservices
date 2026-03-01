using AuctionService.Contracts.Commands;
using BuildingBlocks.Application.CQRS;
using MassTransit;

namespace Auctions.Application.Commands.QueueBulkUpdateAuctions;

public class QueueBulkUpdateAuctionsCommandHandler : ICommandHandler<QueueBulkUpdateAuctionsCommand, BackgroundJobResult>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<QueueBulkUpdateAuctionsCommandHandler> _logger;

    public QueueBulkUpdateAuctionsCommandHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<QueueBulkUpdateAuctionsCommandHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<BackgroundJobResult>> Handle(
        QueueBulkUpdateAuctionsCommand request,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();

        var command = new ProcessBulkAuctionUpdateCommand
        {
            CorrelationId = correlationId,
            RequestedBy = request.RequestedBy,
            AuctionIds = request.AuctionIds,
            Activate = request.Activate,
            Reason = request.Reason,
            RequestedAt = DateTimeOffset.UtcNow
        };

        await _publishEndpoint.Publish(command, cancellationToken);

        _logger.LogInformation(
            "Queued bulk auction update job {CorrelationId} for {Count} auctions (Activate={Activate})",
            correlationId, request.AuctionIds.Count, request.Activate);

        return Result<BackgroundJobResult>.Success(new BackgroundJobResult(
            JobId: correlationId,
            CorrelationId: correlationId.ToString(),
            Status: "Queued",
            Message: $"Bulk update of {request.AuctionIds.Count} auctions has been queued for background processing."));
    }
}
