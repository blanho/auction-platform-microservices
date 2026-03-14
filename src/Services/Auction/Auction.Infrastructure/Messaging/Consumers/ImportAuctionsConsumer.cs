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

public class ImportAuctionsConsumer : IConsumer<ProcessAuctionImportCommand>
{
    private const int BatchSize = 500;

    private readonly IAuctionBulkRepository _bulkRepository;
    private readonly IImportCheckpointRepository _checkpointRepository;
    private readonly ISanitizationService _sanitizationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportAuctionsConsumer> _logger;

    public ImportAuctionsConsumer(
        IAuctionBulkRepository bulkRepository,
        IImportCheckpointRepository checkpointRepository,
        ISanitizationService sanitizationService,
        IUnitOfWork unitOfWork,
        ILogger<ImportAuctionsConsumer> logger)
    {
        _bulkRepository = bulkRepository;
        _checkpointRepository = checkpointRepository;
        _sanitizationService = sanitizationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessAuctionImportCommand> context)
    {
        var message = context.Message;
        var stopwatch = Stopwatch.StartNew();
        var correlationId = message.CorrelationId.ToString();

        _logger.LogInformation(
            "Processing auction import {CorrelationId} for seller {SellerId} with {RowCount} rows",
            correlationId, message.SellerId, message.Rows.Count);

        await PublishJobRequest(context, message, correlationId);

        var checkpoint = await _checkpointRepository.GetCheckpointAsync(
            correlationId, context.CancellationToken);

        var validationResult = ValidateAllRows(message.Rows, message.Currency);

        if (validationResult.ValidRows.Count == 0)
        {
            await ReportJobFailure(context, correlationId,
                $"All {validationResult.Errors.Count} rows failed validation.");
            await PublishCompletionEvent(context, message, stopwatch.Elapsed, 0,
                validationResult.Errors.Count, 0, validationResult.Errors);
            return;
        }

        var rowsToProcess = ResumeFromCheckpoint(validationResult.ValidRows, checkpoint);
        var priorSucceeded = checkpoint?.SucceededCount ?? 0;

        if (validationResult.Errors.Count > 0)
        {
            await ReportJobBatchProgress(context, correlationId, 0, validationResult.Errors.Count);
        }

        var batchSucceeded = await ProcessBatchesAsync(
            rowsToProcess, message, correlationId,
            priorSucceeded, validationResult.Errors.Count,
            context);

        var totalSucceeded = priorSucceeded + batchSucceeded;

        await _checkpointRepository.DeleteCheckpointAsync(correlationId, context.CancellationToken);

        stopwatch.Stop();

        await PublishCompletionEvent(context, message, stopwatch.Elapsed,
            totalSucceeded, validationResult.Errors.Count, 0, validationResult.Errors);

        _logger.LogInformation(
            "Import {CorrelationId} completed: {Succeeded}/{Total} succeeded in {Duration}ms",
            correlationId, totalSucceeded, message.Rows.Count, stopwatch.ElapsedMilliseconds);
    }

    private static IReadOnlyList<ValidatedImportRow> ResumeFromCheckpoint(
        IReadOnlyList<ValidatedImportRow> validRows,
        ImportCheckpoint? checkpoint)
    {
        if (checkpoint is null || checkpoint.LastProcessedRowIndex <= 0)
        {
            return validRows;
        }

        return validRows.Where(r => r.RowNumber > checkpoint.LastProcessedRowIndex).ToList();
    }

    private async Task<int> ProcessBatchesAsync(
        IReadOnlyList<ValidatedImportRow> validRows,
        ProcessAuctionImportCommand message,
        string correlationId,
        int priorSucceeded,
        int failedCount,
        ConsumeContext<ProcessAuctionImportCommand> context)
    {
        var totalInserted = 0;

        foreach (var batch in ChunkRows(validRows, BatchSize))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var auctions = MapToEntities(batch, message.SellerId, message.SellerUsername, message.Currency);
            await _bulkRepository.BulkInsertAsync(auctions, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            totalInserted += auctions.Count;

            await _checkpointRepository.SaveCheckpointAsync(
                new ImportCheckpoint(
                    correlationId,
                    batch[^1].RowNumber,
                    priorSucceeded + totalInserted,
                    failedCount,
                    DateTimeOffset.UtcNow),
                context.CancellationToken);

            await ReportJobBatchProgress(context, correlationId, auctions.Count, 0);

            _logger.LogInformation(
                "Import batch processed: {Inserted}/{Total} for {CorrelationId}",
                totalInserted, validRows.Count, correlationId);
        }

        return totalInserted;
    }

    private static async Task PublishCompletionEvent(
        ConsumeContext<ProcessAuctionImportCommand> context,
        ProcessAuctionImportCommand message,
        TimeSpan duration,
        int succeeded,
        int failed,
        int skipped,
        IReadOnlyList<ImportRowValidationError> errors)
    {
        await context.Publish(new AuctionImportCompletedEvent
        {
            CorrelationId = message.CorrelationId,
            SellerId = message.SellerId,
            TotalRows = message.Rows.Count,
            SucceededCount = succeeded,
            FailedCount = failed,
            SkippedDuplicateCount = skipped,
            Duration = duration,
            CompletedAt = DateTimeOffset.UtcNow,
            Errors = errors.Select(e => new ImportRowErrorPayload
            {
                RowNumber = e.RowNumber,
                Field = e.Field,
                ErrorMessage = e.ErrorMessage
            }).ToList()
        });
    }

    private static RowValidationResult ValidateAllRows(
        List<ImportAuctionItemPayload> rows, string currency)
    {
        var validRows = new List<ValidatedImportRow>();
        var errors = new List<ImportRowValidationError>();

        foreach (var row in rows)
        {
            var rowErrors = ValidateSingleRow(row, currency);

            if (rowErrors.Count == 0)
            {
                validRows.Add(new ValidatedImportRow(row.RowNumber, row));
            }
            else
            {
                errors.AddRange(rowErrors);
            }
        }

        return new RowValidationResult(validRows, errors);
    }

    private static List<ImportRowValidationError> ValidateSingleRow(ImportAuctionItemPayload row, string currency)
    {
        var errors = new List<ImportRowValidationError>();

        if (string.IsNullOrWhiteSpace(row.Title))
            errors.Add(new ImportRowValidationError(row.RowNumber, nameof(row.Title), "Title is required."));
        else if (row.Title.Length > ValidationConstants.StringLength.Medium)
            errors.Add(new ImportRowValidationError(row.RowNumber, nameof(row.Title),
                $"Title must not exceed {ValidationConstants.StringLength.Medium} characters."));

        if (string.IsNullOrWhiteSpace(row.Description))
            errors.Add(new ImportRowValidationError(row.RowNumber, nameof(row.Description), "Description is required."));

        if (row.ReservePrice < 0)
            errors.Add(new ImportRowValidationError(row.RowNumber, nameof(row.ReservePrice), "Reserve price must be non-negative."));

        if (row.BuyNowPrice.HasValue && row.BuyNowPrice.Value <= row.ReservePrice)
            errors.Add(new ImportRowValidationError(row.RowNumber, nameof(row.BuyNowPrice), "Buy now price must exceed reserve price."));

        if (row.AuctionEnd <= DateTimeOffset.UtcNow)
            errors.Add(new ImportRowValidationError(row.RowNumber, nameof(row.AuctionEnd), "Auction end date must be in the future."));

        if (row.YearManufactured.HasValue && (row.YearManufactured < 1900 || row.YearManufactured > DateTime.UtcNow.Year + 1))
            errors.Add(new ImportRowValidationError(row.RowNumber, nameof(row.YearManufactured),
                $"Year must be between 1900 and {DateTime.UtcNow.Year + 1}."));

        return errors;
    }

    private List<Auction> MapToEntities(
        IReadOnlyList<ValidatedImportRow> rows,
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

    private static IEnumerable<IReadOnlyList<ValidatedImportRow>> ChunkRows(
        IReadOnlyList<ValidatedImportRow> rows, int chunkSize)
    {
        for (var i = 0; i < rows.Count; i += chunkSize)
        {
            var remaining = Math.Min(chunkSize, rows.Count - i);
            yield return rows.Skip(i).Take(remaining).ToList();
        }
    }

    private static async Task PublishJobRequest(
        ConsumeContext<ProcessAuctionImportCommand> context,
        ProcessAuctionImportCommand message,
        string correlationId)
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
                RowCount = message.Rows.Count
            }),
            TotalItems = message.Rows.Count,
            MaxRetryCount = 0
        });
    }

    private static async Task ReportJobBatchProgress(
        ConsumeContext<ProcessAuctionImportCommand> context,
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

    private static async Task ReportJobFailure(
        ConsumeContext<ProcessAuctionImportCommand> context,
        string correlationId,
        string errorMessage)
    {
        await context.Publish(new FailJobByCorrelationCommand
        {
            CorrelationId = correlationId,
            ErrorMessage = errorMessage
        });
    }

    private sealed record ValidatedImportRow(int RowNumber, ImportAuctionItemPayload Payload);
    private sealed record ImportRowValidationError(int RowNumber, string Field, string ErrorMessage);
    private sealed record RowValidationResult(
        IReadOnlyList<ValidatedImportRow> ValidRows,
        IReadOnlyList<ImportRowValidationError> Errors);
}
