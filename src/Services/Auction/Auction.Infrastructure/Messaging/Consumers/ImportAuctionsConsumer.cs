using BuildingBlocks.Application.Localization;
using System.Text.Json;
using System.Diagnostics;
using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Application.Features.Auctions.ImportAuctions;
using Auctions.Domain.Constants;
using Auctions.Domain.Entities;
using BuildingBlocks.Domain.Constants;
using JobService.Contracts.Commands;
using JobService.Contracts.Enums;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class ImportAuctionsConsumer : IConsumer<ProcessAuctionImportCommand>
{
    private readonly IAuctionWorkflowStore _workflow;
    private readonly IAuctionBulkRepository _bulkRepository;
    private readonly ISanitizationService _sanitizationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportAuctionsConsumer> _logger;

    public ImportAuctionsConsumer(
        IAuctionWorkflowStore workflow,
        IAuctionBulkRepository bulkRepository,
        ISanitizationService sanitizationService,
        IUnitOfWork unitOfWork,
        ILogger<ImportAuctionsConsumer> logger)
    {
        _bulkRepository = bulkRepository;
        _sanitizationService = sanitizationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _workflow = workflow;
    }

    public async Task Consume(ConsumeContext<ProcessAuctionImportCommand> context)
    {
        var message = context.Message;
        using var culture = new RequestCultureScope(message.Culture);
        var receiptKey = $"ImportAuctionsConsumer:{message.CorrelationId}";
        if (await _workflow.ExistsAsync(receiptKey, context.CancellationToken)) return;
        var stopwatch = Stopwatch.StartNew();
        var correlationId = message.CorrelationId.ToString();

        _logger.LogInformation(
            "Processing auction import {CorrelationId} for seller {SellerId} with {RowCount} rows",
            correlationId, message.SellerId, message.Rows.Count);

        await PublishJobRequest(context, message, correlationId);

        var validationResult = ValidateAllRows(message.Rows, message.Currency);
        var failedRowCount = message.Rows.Count - validationResult.ValidRows.Count;

        if (validationResult.ValidRows.Count == 0)
        {
            await ReportJobFailure(context, correlationId,
                $"All {failedRowCount} rows failed validation.");
            await PublishCompletionEvent(context, message, stopwatch.Elapsed, 0,
                failedRowCount, 0, validationResult.Errors);
            return;
        }

        var totalSucceeded = await ProcessBatchesAsync(
            validationResult.ValidRows, message, correlationId, context);
        await ReportJobBatchProgress(context, correlationId, totalSucceeded, failedRowCount);

        stopwatch.Stop();

        await PublishCompletionEvent(context, message, stopwatch.Elapsed,
            totalSucceeded, failedRowCount, 0, validationResult.Errors);

        _logger.LogInformation(
            "Import {CorrelationId} completed: {Succeeded}/{Total} succeeded in {Duration}ms",
            correlationId, totalSucceeded, message.Rows.Count, stopwatch.ElapsedMilliseconds);
    }

    private async Task<int> ProcessBatchesAsync(
        IReadOnlyList<ValidatedImportRow> validRows,
        ProcessAuctionImportCommand message,
        string correlationId,
        ConsumeContext<ProcessAuctionImportCommand> context)
    {
        var totalInserted = 0;

        foreach (var batch in validRows.Chunk(AuctionDefaults.Batch.InsertBatchSize))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var auctions = MapToEntities(batch, message.SellerId, message.SellerUsername, message.Currency);
            await _bulkRepository.BulkInsertAsync(auctions, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            totalInserted += auctions.Count;

            _logger.LogInformation(
                "Import batch processed: {Inserted}/{Total} for {CorrelationId}",
                totalInserted, validRows.Count, correlationId);
        }

        return totalInserted;
    }

    private async Task PublishCompletionEvent(
        ConsumeContext<ProcessAuctionImportCommand> context,
        ProcessAuctionImportCommand message,
        TimeSpan duration,
        int succeeded,
        int failed,
        int skipped,
        IReadOnlyList<ImportRowValidationError> errors)
    {
        await _workflow.CompleteAsync($"ImportAuctionsConsumer:{message.CorrelationId}", message.CorrelationId,
            succeeded, failed, [], 1, context.CancellationToken);
        if (message.ParentJobId.HasValue && message.ParentJobItemId.HasValue)
            await context.Publish(new ReportJobItemResultCommand
            {
                JobId = message.ParentJobId.Value,
                JobItemId = message.ParentJobItemId.Value,
                Attempt = message.Attempt,
                IsSuccess = failed == 0,
                IsFinalFailure = true,
                ErrorMessage = failed == 0 ? null : "Auction workflow completed with errors."
            });
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
                ErrorMessage = UserMessageLocalizer.Translate(e.ErrorMessage)
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

    private static async Task PublishJobRequest(
        ConsumeContext<ProcessAuctionImportCommand> context,
        ProcessAuctionImportCommand message,
        string correlationId)
    {
        if (message.ParentJobId is null)
            await context.Publish(new RequestJobCommand
            {
                JobType = nameof(JobType.AuctionImport),
                CorrelationId = correlationId,
                RequestedBy = message.SellerId,
                PayloadJson = JsonSerializer.Serialize(new
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
        if (context.Message.ParentJobId is not null) return;
        await context.Publish(new ReportJobBatchProgressCommand
        {
            CorrelationId = correlationId,
            BatchId = "import:complete",
            CompletedCount = completedCount,
            FailedCount = failedCount
        });
    }

    private static async Task ReportJobFailure(
        ConsumeContext<ProcessAuctionImportCommand> context,
        string correlationId,
        string errorMessage)
    {
        if (context.Message.ParentJobId is not null) return;
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
