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

public class ImportAuctionsBatchConsumer : IConsumer<ProcessAuctionImportBatchCommand>
{
    private readonly IAuctionWorkflowStore _workflow;
    private readonly IAuctionBulkRepository _bulkRepository;
    private readonly ISanitizationService _sanitizationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportAuctionsBatchConsumer> _logger;

    public ImportAuctionsBatchConsumer(
        IAuctionWorkflowStore workflow,
        IAuctionBulkRepository bulkRepository,
        ISanitizationService sanitizationService,
        IUnitOfWork unitOfWork,
        ILogger<ImportAuctionsBatchConsumer> logger)
    {
        _bulkRepository = bulkRepository;
        _sanitizationService = sanitizationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _workflow = workflow;
    }

    public async Task Consume(ConsumeContext<ProcessAuctionImportBatchCommand> context)
    {
        var message = context.Message;
        using var culture = new RequestCultureScope(message.Culture);
        var receiptKey = $"ImportAuctionsBatchConsumer:{message.CorrelationId}:{message.BatchNumber}";
        if (await _workflow.ExistsAsync(receiptKey, context.CancellationToken))
        {
            return;
        }

        var correlationId = message.CorrelationId.ToString();
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Processing import batch {BatchNumber}/{TotalBatches} for {CorrelationId}: {RowCount} rows",
            message.BatchNumber, message.TotalBatches, correlationId, message.Rows.Count);

        if (message.BatchNumber < 1 || message.BatchNumber > message.TotalBatches || message.TotalRows <= 0)
        {
            throw new ArgumentException("Invalid import batch metadata.");
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
                RowCount = message.TotalRows,
                TotalBatches = message.TotalBatches
            }),
            TotalItems = message.TotalRows,
            MaxRetryCount = 0
        });

        var validationResult = ValidateRows(message.Rows);
        var failedRowCount = message.Rows.Count - validationResult.ValidRows.Count;

        var insertedCount = 0;

        foreach (var chunk in validationResult.ValidRows.Chunk(AuctionDefaults.Batch.InsertBatchSize))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var auctions = chunk.Select(validatedRow => ImportAuctionMapper.ToAuction(
                validatedRow.Row, message.SellerId, message.SellerUsername, message.Currency, _sanitizationService))
                .ToList();
            await _bulkRepository.BulkInsertAsync(auctions, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            insertedCount += auctions.Count;
        }

        await ReportProgressAsync(context, correlationId, insertedCount, failedRowCount);
        stopwatch.Stop();

        _logger.LogInformation(
            "Import batch {BatchNumber}/{TotalBatches} completed for {CorrelationId}: {Inserted} inserted, {Failed} failed in {Duration}ms",
            message.BatchNumber, message.TotalBatches, correlationId,
            insertedCount, failedRowCount, stopwatch.ElapsedMilliseconds);

        var localizedErrors = validationResult.Errors.Select(error => new ImportRowErrorPayload
        {
            RowNumber = error.RowNumber,
            Field = error.Field,
            ErrorMessage = UserMessageLocalizer.Translate(error.ErrorMessage)
        }).ToList();
        var totals = await _workflow.CompleteAsync(receiptKey, message.CorrelationId,
            insertedCount, failedRowCount, localizedErrors, message.TotalBatches, context.CancellationToken);
        if (totals is not null)
        {
            await context.Publish(new AuctionImportCompletedEvent
            {
                CorrelationId = message.CorrelationId,
                SellerId = message.SellerId,
                TotalRows = message.TotalRows,
                SucceededCount = totals.Succeeded,
                FailedCount = totals.Failed,
                SkippedDuplicateCount = 0,
                Duration = stopwatch.Elapsed,
                CompletedAt = DateTimeOffset.UtcNow,
                Errors = totals.Errors
            });
        }
    }

    private static RowValidationResult ValidateRows(List<ImportAuctionItemPayload> rows)
    {
        var validRows = new List<ValidatedRow>();
        var errors = new List<ImportRowError>();

        foreach (var row in rows)
        {
            var importRow = ImportAuctionMapper.ToRow(row);
            var rowErrors = ImportAuctionRowValidator.Validate(importRow, row.RowNumber, DateTimeOffset.UtcNow);
            if (rowErrors.Count == 0)
            {
                validRows.Add(new ValidatedRow(row.RowNumber, importRow));
            }
            else
            {
                errors.AddRange(rowErrors);
            }
        }

        return new RowValidationResult(validRows, errors);
    }

    private static async Task ReportProgressAsync(
        ConsumeContext<ProcessAuctionImportBatchCommand> context,
        string correlationId,
        int completedCount,
        int failedCount)
    {
        await context.Publish(new ReportJobBatchProgressCommand
        {
            CorrelationId = correlationId,
            BatchId = $"import:{context.Message.BatchNumber}",
            CompletedCount = completedCount,
            FailedCount = failedCount
        });
    }

    private sealed record ValidatedRow(int RowNumber, ImportAuctionRow Row);
    private sealed record RowValidationResult(
        IReadOnlyList<ValidatedRow> ValidRows,
        IReadOnlyList<ImportRowError> Errors);
}
