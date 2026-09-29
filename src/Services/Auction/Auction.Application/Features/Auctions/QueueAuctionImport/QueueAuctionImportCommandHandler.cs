using JobService.Contracts.Commands;
using AuctionService.Contracts.Commands;
using Auctions.Domain.Constants;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Features.Auctions.QueueAuctionImport;

public class QueueAuctionImportCommandHandler : ICommandHandler<QueueAuctionImportCommand, BackgroundJobResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<QueueAuctionImportCommandHandler> _logger;

    public QueueAuctionImportCommandHandler(
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<QueueAuctionImportCommandHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BackgroundJobResult>> Handle(
        QueueAuctionImportCommand request,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        await _eventPublisher.PublishAsync(new RequestJobCommand
        {
            CorrelationId = correlationId.ToString(),
            JobType = "AuctionImport",
            RequestedBy = request.SellerId,
            TotalItems = request.Rows.Count,
            MaxRetryCount = 0,
            PayloadJson = "{}"
        }, cancellationToken);

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
        var batches = ChunkList(payloadRows, AuctionDefaults.Batch.RowsPerImportBatch);
        var totalBatches = batches.Count;

        for (var i = 0; i < totalBatches; i++)
        {
            var batchCommand = new ProcessAuctionImportBatchCommand
            {
                CorrelationId = correlationId,
                Culture = System.Globalization.CultureInfo.CurrentUICulture.Name,
                SellerId = request.SellerId,
                SellerUsername = request.SellerUsername,
                Currency = request.Currency,
                BatchNumber = i + 1,
                TotalBatches = totalBatches,
                TotalRows = totalRows,
                Rows = batches[i]
            };

            await _eventPublisher.PublishAsync(batchCommand, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Queued auction import {CorrelationId} for seller {SellerId}: {RowCount} rows in {BatchCount} batches",
            correlationId, request.SellerId, totalRows, totalBatches);

        return Result<BackgroundJobResult>.Success(new BackgroundJobResult(
            JobId: correlationId,
            CorrelationId: correlationId.ToString(),
            Status: BackgroundJobStatuses.Queued,
            Message: BuildingBlocks.Application.Localization.UserMessageLocalizer.Translate($"Import of {totalRows} auctions has been queued for background processing ({totalBatches} batches).")));
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
