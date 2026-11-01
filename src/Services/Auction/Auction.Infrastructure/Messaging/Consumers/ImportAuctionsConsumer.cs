using BuildingBlocks.Application.Localization;
using System.Text.Json;
using System.Diagnostics;
using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Application.Features.Auctions.ImportAuctions;
using Auctions.Domain.Constants;
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
        if (await _workflow.ExistsAsync(receiptKey, context.CancellationToken))
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = message.CorrelationId.ToString();

        _logger.LogInformation(
            "Processing auction import {CorrelationId} for seller {SellerId} with {RowCount} rows",
            correlationId, message.SellerId, message.Rows.Count);

        await PublishJobRequestAsync(context, message, correlationId);

        var validationResult = ValidateAllRows(message.Rows);
        var failedRowCount = message.Rows.Count - validationResult.ValidRows.Count;

        if (validationResult.ValidRows.Count == 0)
        {
            await ReportJobFailureAsync(context, correlationId,
                $"All {failedRowCount} rows failed validation.");
            await PublishCompletionEventAsync(context, message, stopwatch.Elapsed, 0,
                failedRowCount, 0, validationResult.Errors);
            return;
        }

        var totalSucceeded = await ProcessBatchesAsync(
            validationResult.ValidRows, message, correlationId, context);
        await ReportJobBatchProgressAsync(context, correlationId, totalSucceeded, failedRowCount);

        stopwatch.Stop();

        await PublishCompletionEventAsync(context, message, stopwatch.Elapsed,
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

            var auctions = batch.Select(validatedRow => ImportAuctionMapper.ToAuction(
                validatedRow.Row, message.SellerId, message.SellerUsername, message.Currency, _sanitizationService))
                .ToList();
            await _bulkRepository.BulkInsertAsync(auctions, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            totalInserted += auctions.Count;

            _logger.LogInformation(
                "Import batch processed: {Inserted}/{Total} for {CorrelationId}",
                totalInserted, validRows.Count, correlationId);
        }

        return totalInserted;
    }

    private async Task PublishCompletionEventAsync(
        ConsumeContext<ProcessAuctionImportCommand> context,
        ProcessAuctionImportCommand message,
        TimeSpan duration,
        int succeeded,
        int failed,
        int skipped,
        IReadOnlyList<ImportRowError> errors)
    {
        await _workflow.CompleteAsync($"ImportAuctionsConsumer:{message.CorrelationId}", message.CorrelationId,
            succeeded, failed, [], 1, context.CancellationToken);
        if (message.ParentJobId.HasValue && message.ParentJobItemId.HasValue)
        {
            await context.Publish(new ReportJobItemResultCommand
            {
                JobId = message.ParentJobId.Value,
                JobItemId = message.ParentJobItemId.Value,
                Attempt = message.Attempt,
                IsSuccess = failed == 0,
                IsFinalFailure = true,
                ErrorMessage = failed == 0 ? null : "Auction workflow completed with errors."
            });
        }

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
        List<ImportAuctionItemPayload> rows)
    {
        var validRows = new List<ValidatedImportRow>();
        var errors = new List<ImportRowError>();

        foreach (var row in rows)
        {
            var importRow = ImportAuctionMapper.ToRow(row);
            var rowErrors = ImportAuctionRowValidator.Validate(importRow, row.RowNumber, DateTimeOffset.UtcNow);

            if (rowErrors.Count == 0)
            {
                validRows.Add(new ValidatedImportRow(row.RowNumber, importRow));
            }
            else
            {
                errors.AddRange(rowErrors);
            }
        }

        return new RowValidationResult(validRows, errors);
    }

    private static async Task PublishJobRequestAsync(
        ConsumeContext<ProcessAuctionImportCommand> context,
        ProcessAuctionImportCommand message,
        string correlationId)
    {
        if (message.ParentJobId is not null)
        {
            return;
        }

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

    private static async Task ReportJobBatchProgressAsync(
        ConsumeContext<ProcessAuctionImportCommand> context,
        string correlationId,
        int completedCount,
        int failedCount)
    {
        if (context.Message.ParentJobId is not null)
        {
            return;
        }

        await context.Publish(new ReportJobBatchProgressCommand
        {
            CorrelationId = correlationId,
            BatchId = "import:complete",
            CompletedCount = completedCount,
            FailedCount = failedCount
        });
    }

    private static async Task ReportJobFailureAsync(
        ConsumeContext<ProcessAuctionImportCommand> context,
        string correlationId,
        string errorMessage)
    {
        if (context.Message.ParentJobId is not null)
        {
            return;
        }

        await context.Publish(new FailJobByCorrelationCommand
        {
            CorrelationId = correlationId,
            ErrorMessage = errorMessage
        });
    }

    private sealed record ValidatedImportRow(int RowNumber, ImportAuctionRow Row);
    private sealed record RowValidationResult(
        IReadOnlyList<ValidatedImportRow> ValidRows,
        IReadOnlyList<ImportRowError> Errors);
}
