using System.Diagnostics;
using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Application.Features.Auctions.ImportAuctions;
using Auctions.Domain.Entities;
using BuildingBlocks.Domain.Constants;
using JobService.Contracts.Commands;
using JobService.Contracts.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class ImportAuctionsBatchConsumer : IConsumer<ProcessAuctionImportBatchCommand>
{
    private const int InsertBatchSize = 500;

    private readonly IAuctionBulkRepository _bulkRepository;
    private readonly ISanitizationService _sanitizationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportAuctionsBatchConsumer> _logger;

    public ImportAuctionsBatchConsumer(
        IAuctionBulkRepository bulkRepository,
        ISanitizationService sanitizationService,
        IUnitOfWork unitOfWork,
        ILogger<ImportAuctionsBatchConsumer> logger)
    {
        _bulkRepository = bulkRepository;
        _sanitizationService = sanitizationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessAuctionImportBatchCommand> context)
    {
        var message = context.Message;
        var correlationId = message.CorrelationId.ToString();
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Processing import batch {BatchNumber}/{TotalBatches} for {CorrelationId}: {RowCount} rows",
            message.BatchNumber, message.TotalBatches, correlationId, message.Rows.Count);

        if (message.BatchNumber == 1)
        {
            await context.Publish(new RequestJobCommand
            {
                JobType = nameof(JobType.AuctionImport),
                CorrelationId = correlationId,
                RequestedBy = message.SellerId,
                PayloadJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    message.SellerId,
                    message.SellerUsername,
                    message.Currency,
                    RowCount = message.TotalRows,
                    TotalBatches = message.TotalBatches
                }),
                TotalItems = message.TotalRows,
                MaxRetryCount = 0
            });
        }

        var validationResult = ValidateRows(message.Rows, message.Currency);

        if (validationResult.Errors.Count > 0)
        {
            await ReportProgress(context, correlationId, 0, validationResult.Errors.Count);
        }

        var insertedCount = 0;

        foreach (var chunk in ChunkRows(validationResult.ValidRows, InsertBatchSize))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var auctions = MapToEntities(chunk, message.SellerId, message.SellerUsername, message.Currency);
            await _bulkRepository.BulkInsertAsync(auctions, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            insertedCount += auctions.Count;

            await ReportProgress(context, correlationId, auctions.Count, 0);
        }

        stopwatch.Stop();

        _logger.LogInformation(
            "Import batch {BatchNumber}/{TotalBatches} completed for {CorrelationId}: {Inserted} inserted, {Failed} failed in {Duration}ms",
            message.BatchNumber, message.TotalBatches, correlationId,
            insertedCount, validationResult.Errors.Count, stopwatch.ElapsedMilliseconds);

        if (message.BatchNumber == message.TotalBatches)
        {
            await context.Publish(new AuctionImportCompletedEvent
            {
                CorrelationId = message.CorrelationId,
                SellerId = message.SellerId,
                TotalRows = message.TotalRows,
                SucceededCount = insertedCount,
                FailedCount = validationResult.Errors.Count,
                SkippedDuplicateCount = 0,
                Duration = stopwatch.Elapsed,
                CompletedAt = DateTimeOffset.UtcNow,
                Errors = validationResult.Errors.Select(e => new ImportRowErrorPayload
                {
                    RowNumber = e.RowNumber,
                    Field = e.Field,
                    ErrorMessage = e.ErrorMessage
                }).ToList()
            });
        }
    }

    private static RowValidationResult ValidateRows(List<ImportAuctionItemPayload> rows, string currency)
    {
        var validRows = new List<ValidatedRow>();
        var errors = new List<RowError>();

        foreach (var row in rows)
        {
            var rowErrors = ValidateRow(row, currency);
            if (rowErrors.Count == 0)
            {
                validRows.Add(new ValidatedRow(row.RowNumber, row));
            }
            else
            {
                errors.AddRange(rowErrors);
            }
        }

        return new RowValidationResult(validRows, errors);
    }

    private static List<RowError> ValidateRow(ImportAuctionItemPayload row, string currency)
    {
        var errors = new List<RowError>();

        if (string.IsNullOrWhiteSpace(row.Title))
            errors.Add(new RowError(row.RowNumber, nameof(row.Title), "Title is required."));
        else if (row.Title.Length > ValidationConstants.StringLength.Medium)
            errors.Add(new RowError(row.RowNumber, nameof(row.Title),
                $"Title must not exceed {ValidationConstants.StringLength.Medium} characters."));

        if (string.IsNullOrWhiteSpace(row.Description))
            errors.Add(new RowError(row.RowNumber, nameof(row.Description), "Description is required."));

        if (row.ReservePrice < 0)
            errors.Add(new RowError(row.RowNumber, nameof(row.ReservePrice), "Reserve price must be non-negative."));

        if (row.BuyNowPrice.HasValue && row.BuyNowPrice.Value <= row.ReservePrice)
            errors.Add(new RowError(row.RowNumber, nameof(row.BuyNowPrice), "Buy now price must exceed reserve price."));

        if (row.AuctionEnd <= DateTimeOffset.UtcNow)
            errors.Add(new RowError(row.RowNumber, nameof(row.AuctionEnd), "Auction end date must be in the future."));

        if (row.YearManufactured.HasValue && (row.YearManufactured < 1900 || row.YearManufactured > DateTime.UtcNow.Year + 1))
            errors.Add(new RowError(row.RowNumber, nameof(row.YearManufactured),
                $"Year must be between 1900 and {DateTime.UtcNow.Year + 1}."));

        return errors;
    }

    private List<Auction> MapToEntities(
        IReadOnlyList<ValidatedRow> rows,
        Guid sellerId,
        string sellerUsername,
        string currency)
    {
        var auctions = new List<Auction>(rows.Count);

        foreach (var validatedRow in rows)
        {
            var row = validatedRow.Payload;

            var item = Item.Create(
                title: _sanitizationService.SanitizeText(row.Title),
                description: _sanitizationService.SanitizeHtml(row.Description),
                condition: row.Condition,
                yearManufactured: row.YearManufactured,
                categoryId: row.CategoryId,
                brandId: row.BrandId);

            if (row.Attributes != null)
            {
                foreach (var attr in row.Attributes)
                {
                    item.SetAttribute(attr.Key, attr.Value);
                }
            }

            var auction = Auction.Create(new CreateAuctionParams(
                SellerId: sellerId,
                SellerUsername: sellerUsername,
                Item: item,
                ReservePrice: row.ReservePrice,
                AuctionEnd: row.AuctionEnd,
                Currency: currency,
                BuyNowPrice: row.BuyNowPrice,
                IsFeatured: false));

            auctions.Add(auction);
        }

        return auctions;
    }

    private static IEnumerable<IReadOnlyList<ValidatedRow>> ChunkRows(
        IReadOnlyList<ValidatedRow> rows, int chunkSize)
    {
        for (var i = 0; i < rows.Count; i += chunkSize)
        {
            yield return rows.Skip(i).Take(Math.Min(chunkSize, rows.Count - i)).ToList();
        }
    }

    private static async Task ReportProgress(
        ConsumeContext<ProcessAuctionImportBatchCommand> context,
        string correlationId,
        int completedCount,
        int failedCount)
    {
        await context.Publish(new ReportJobBatchProgressCommand
        {
            CorrelationId = correlationId,
            CompletedCount = completedCount,
            FailedCount = failedCount
        });
    }

    private sealed record ValidatedRow(int RowNumber, ImportAuctionItemPayload Payload);
    private sealed record RowError(int RowNumber, string Field, string ErrorMessage);
    private sealed record RowValidationResult(
        IReadOnlyList<ValidatedRow> ValidRows,
        IReadOnlyList<RowError> Errors);
}
