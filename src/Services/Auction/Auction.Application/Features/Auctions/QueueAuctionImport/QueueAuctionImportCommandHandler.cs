using AuctionService.Contracts.Commands;
using BuildingBlocks.Application.CQRS;
using MassTransit;

namespace Auctions.Application.Commands.QueueAuctionImport;

public class QueueAuctionImportCommandHandler : ICommandHandler<QueueAuctionImportCommand, BackgroundJobResult>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<QueueAuctionImportCommandHandler> _logger;

    public QueueAuctionImportCommandHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<QueueAuctionImportCommandHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<BackgroundJobResult>> Handle(
        QueueAuctionImportCommand request,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();

        var command = new ProcessAuctionImportCommand
        {
            CorrelationId = correlationId,
            SellerId = request.SellerId,
            SellerUsername = request.SellerUsername,
            Currency = request.Currency,
            RequestedAt = DateTimeOffset.UtcNow,
            Rows = request.Rows.Select((row, index) => new ImportAuctionItemPayload
            {
                RowNumber = index + 1,
                Title = row.Title,
                Description = row.Description,
                Condition = row.Condition,
                YearManufactured = row.YearManufactured,
                ReservePrice = row.ReservePrice,
                BuyNowPrice = row.BuyNowPrice,
                AuctionEnd = row.AuctionEnd,
                CategoryId = row.CategoryId,
                BrandId = row.BrandId,
                Attributes = row.Attributes
            }).ToList()
        };

        await _publishEndpoint.Publish(command, cancellationToken);

        _logger.LogInformation(
            "Queued auction import job {CorrelationId} for seller {SellerId} with {RowCount} rows",
            correlationId, request.SellerId, request.Rows.Count);

        return Result<BackgroundJobResult>.Success(new BackgroundJobResult(
            JobId: correlationId,
            CorrelationId: correlationId.ToString(),
            Status: "Queued",
            Message: $"Import of {request.Rows.Count} auctions has been queued for background processing."));
    }
}
