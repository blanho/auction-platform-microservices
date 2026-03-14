using AuctionService.Contracts.Commands;
using BuildingBlocks.Application.CQRS;
using MassTransit;

namespace Auctions.Application.Features.Auctions.QueueAuctionImport;

public class QueueAuctionImportCommandHandler : ICommandHandler<QueueAuctionImportCommand, BackgroundJobResult>
{
    private const int MaxRowsPerBatch = 1000;

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

        var payloadRows = request.Rows.Select((row, index) => new ImportAuctionItemPayload
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
        }).ToList();

        var totalRows = payloadRows.Count;
        var batches = ChunkList(payloadRows, MaxRowsPerBatch);
        var totalBatches = batches.Count;

        for (var i = 0; i < totalBatches; i++)
        {
            var batchCommand = new ProcessAuctionImportBatchCommand
            {
                CorrelationId = correlationId,
                SellerId = request.SellerId,
                SellerUsername = request.SellerUsername,
                Currency = request.Currency,
                BatchNumber = i + 1,
                TotalBatches = totalBatches,
                TotalRows = totalRows,
                Rows = batches[i]
            };

            await _publishEndpoint.Publish(batchCommand, cancellationToken);
        }

        _logger.LogInformation(
            "Queued auction import {CorrelationId} for seller {SellerId}: {RowCount} rows in {BatchCount} batches",
            correlationId, request.SellerId, totalRows, totalBatches);

        return Result<BackgroundJobResult>.Success(new BackgroundJobResult(
            JobId: correlationId,
            CorrelationId: correlationId.ToString(),
            Status: "Queued",
            Message: $"Import of {totalRows} auctions has been queued for background processing ({totalBatches} batches)."));
    }

    private static List<List<T>> ChunkList<T>(List<T> source, int chunkSize)
    {
        var chunks = new List<List<T>>();
        for (var i = 0; i < source.Count; i += chunkSize)
        {
            chunks.Add(source.GetRange(i, Math.Min(chunkSize, source.Count - i)));
        }
        return chunks;
    }
}
