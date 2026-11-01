using System.Diagnostics;
using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;
using Auctions.Domain.Constants;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Localization;

namespace Auctions.Application.Features.Auctions.ImportAuctions;

public class ImportAuctionsCommandHandler : ICommandHandler<ImportAuctionsCommand, ImportAuctionsResult>
{
    private readonly IAuctionBulkRepository _bulkRepository;
    private readonly IImportCheckpointRepository _checkpointRepository;
    private readonly ISanitizationService _sanitizationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportAuctionsCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;

    public ImportAuctionsCommandHandler(
        IAuctionBulkRepository bulkRepository,
        IImportCheckpointRepository checkpointRepository,
        ISanitizationService sanitizationService,
        IUnitOfWork unitOfWork,
        ILogger<ImportAuctionsCommandHandler> logger,
        IDateTimeProvider dateTime)
    {
        _bulkRepository = bulkRepository;
        _checkpointRepository = checkpointRepository;
        _sanitizationService = sanitizationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _dateTime = dateTime;
    }

    public async Task<Result<ImportAuctionsResult>> Handle(
        ImportAuctionsCommand request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Starting bulk import of {RowCount} auctions for seller {Seller} with correlation {CorrelationId}",
            request.Rows.Count, request.SellerUsername, request.CorrelationId);

        var checkpoint = await _checkpointRepository.GetCheckpointAsync(
            request.CorrelationId, cancellationToken);

        if (checkpoint is not null)
        {
            _logger.LogInformation(
                "Resuming import from checkpoint: {ProcessedRows} rows already processed for {CorrelationId}",
                checkpoint.LastProcessedRowIndex, request.CorrelationId);
        }

        var validationResult = ValidateAllRows(request.Rows);
        var failedRowCount = request.Rows.Count - validationResult.ValidRows.Count;

        if (validationResult.ValidRows.Count == 0)
        {
            await _checkpointRepository.DeleteCheckpointAsync(request.CorrelationId, cancellationToken);

            return Result<ImportAuctionsResult>.Success(new ImportAuctionsResult(
                CorrelationId: request.CorrelationId,
                TotalRows: request.Rows.Count,
                SucceededCount: 0,
                FailedCount: failedRowCount,
                SkippedDuplicateCount: 0,
                Duration: stopwatch.Elapsed,
                Errors: LocalizeErrors(validationResult.Errors)));
        }

        var rowsToProcess = ResumeFromCheckpoint(validationResult.ValidRows, checkpoint);
        var skippedDuplicateCount = validationResult.ValidRows.Count - rowsToProcess.Count;

        var priorSucceeded = checkpoint?.SucceededCount ?? 0;

        var batchSucceeded = await ProcessBatchesAsync(
            rowsToProcess,
            new ImportBatchContext(request.SellerId, request.SellerUsername, request.Currency,
                request.CorrelationId, priorSucceeded, failedRowCount),
            cancellationToken);

        var totalSucceeded = priorSucceeded + batchSucceeded;

        await _checkpointRepository.DeleteCheckpointAsync(request.CorrelationId, cancellationToken);

        stopwatch.Stop();

        _logger.LogInformation(
            "Bulk import completed: {Succeeded}/{Total} succeeded, {Failed} rows failed validation in {Duration}ms",
            totalSucceeded, request.Rows.Count, failedRowCount, stopwatch.ElapsedMilliseconds);

        return Result<ImportAuctionsResult>.Success(new ImportAuctionsResult(
            CorrelationId: request.CorrelationId,
            TotalRows: request.Rows.Count,
            SucceededCount: totalSucceeded,
            FailedCount: failedRowCount,
            SkippedDuplicateCount: skippedDuplicateCount,
            Duration: stopwatch.Elapsed,
            Errors: LocalizeErrors(validationResult.Errors)));
    }

    private static List<ImportRowError> LocalizeErrors(IReadOnlyList<ImportRowError> errors) =>
        errors.Select(error => error with
        {
            ErrorMessage = UserMessageLocalizer.Translate(error.ErrorMessage)
        }).ToList();

    private static IReadOnlyList<ValidatedRow> ResumeFromCheckpoint(
        IReadOnlyList<ValidatedRow> validRows,
        ImportCheckpoint? checkpoint)
    {
        if (checkpoint is null || checkpoint.LastProcessedRowIndex <= 0)
        {
            return validRows;
        }

        return validRows
            .Where(r => r.RowNumber > checkpoint.LastProcessedRowIndex)
            .ToList();
    }

    private async Task<int> ProcessBatchesAsync(
        IReadOnlyList<ValidatedRow> validRows,
        ImportBatchContext batchContext,
        CancellationToken cancellationToken)
    {
        var totalInserted = 0;
        var batches = validRows.Chunk(AuctionDefaults.Batch.InsertBatchSize);

        foreach (var batch in batches)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var auctions = batch.Select(row => ImportAuctionMapper.ToAuction(
                row.Row, batchContext.SellerId, batchContext.SellerUsername, batchContext.Currency, _sanitizationService))
                .ToList();
            await _bulkRepository.BulkInsertAsync(auctions, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            totalInserted += auctions.Count;

            var lastRowNumber = batch[^1].RowNumber;

            await _checkpointRepository.SaveCheckpointAsync(
                new ImportCheckpoint(
                    batchContext.CorrelationId,
                    lastRowNumber,
                    batchContext.PriorSucceeded + totalInserted,
                    batchContext.FailedCount,
                    _dateTime.UtcNowOffset),
                cancellationToken);

            _logger.LogInformation(
                "Processed batch: {Inserted}/{Total} auctions inserted, checkpoint at row {Row}",
                totalInserted, validRows.Count, lastRowNumber);
        }

        return totalInserted;
    }

    private static RowValidationResult ValidateAllRows(IReadOnlyList<ImportAuctionRow> rows)
    {
        var validRows = new List<ValidatedRow>();
        var errors = new List<ImportRowError>();

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNumber = i + 1;
            var row = rows[i];
            var rowErrors = ImportAuctionRowValidator.Validate(row, rowNumber, DateTimeOffset.UtcNow);

            if (rowErrors.Count == 0)
            {
                validRows.Add(new ValidatedRow(rowNumber, row));
            }
            else
            {
                errors.AddRange(rowErrors);
            }
        }

        return new RowValidationResult(validRows, errors);
    }

    private sealed record ImportBatchContext(Guid SellerId, string SellerUsername, string Currency,
        string CorrelationId, int PriorSucceeded, int FailedCount);
    private sealed record ValidatedRow(int RowNumber, ImportAuctionRow Row);
    private sealed record RowValidationResult(
        IReadOnlyList<ValidatedRow> ValidRows,
        IReadOnlyList<ImportRowError> Errors);
}
