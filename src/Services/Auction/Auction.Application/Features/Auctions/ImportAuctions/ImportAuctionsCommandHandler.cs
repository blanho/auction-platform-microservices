using System.Diagnostics;
using Auctions.Application.DTOs;
using Auctions.Application.Features.Auctions.ImportAuctions;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Domain.Constants;

namespace Auctions.Application.Commands.ImportAuctions;

public class ImportAuctionsCommandHandler : ICommandHandler<ImportAuctionsCommand, ImportAuctionsResult>
{
    private const int BatchSize = 500;

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

        var existingCount = await _bulkRepository.CountByCorrelationIdAsync(
            request.CorrelationId, cancellationToken);

        var checkpoint = await _checkpointRepository.GetCheckpointAsync(
            request.CorrelationId, cancellationToken);

        if (checkpoint is not null)
        {
            _logger.LogInformation(
                "Resuming import from checkpoint: {ProcessedRows} rows already processed for {CorrelationId}",
                checkpoint.LastProcessedRowIndex, request.CorrelationId);
        }

        var validationResult = ValidateAllRows(request.Rows, request.Currency);

        if (validationResult.ValidRows.Count == 0)
        {
            await CleanupCheckpointAsync(request.CorrelationId, cancellationToken);

            return Result<ImportAuctionsResult>.Success(new ImportAuctionsResult(
                CorrelationId: request.CorrelationId,
                TotalRows: request.Rows.Count,
                SucceededCount: 0,
                FailedCount: validationResult.Errors.Count,
                SkippedDuplicateCount: existingCount,
                Duration: stopwatch.Elapsed,
                Errors: validationResult.Errors));
        }

        var rowsToProcess = ResumeFromCheckpoint(validationResult.ValidRows, checkpoint);

        var priorSucceeded = checkpoint?.SucceededCount ?? 0;

        var batchSucceeded = await ProcessBatchesAsync(
            rowsToProcess,
            request.SellerId,
            request.SellerUsername,
            request.Currency,
            request.CorrelationId,
            priorSucceeded,
            validationResult.Errors.Count,
            cancellationToken);

        var totalSucceeded = priorSucceeded + batchSucceeded;

        await CleanupCheckpointAsync(request.CorrelationId, cancellationToken);

        stopwatch.Stop();

        _logger.LogInformation(
            "Bulk import completed: {Succeeded}/{Total} succeeded, {Failed} validation errors in {Duration}ms",
            totalSucceeded, request.Rows.Count, validationResult.Errors.Count, stopwatch.ElapsedMilliseconds);

        return Result<ImportAuctionsResult>.Success(new ImportAuctionsResult(
            CorrelationId: request.CorrelationId,
            TotalRows: request.Rows.Count,
            SucceededCount: totalSucceeded,
            FailedCount: validationResult.Errors.Count,
            SkippedDuplicateCount: existingCount,
            Duration: stopwatch.Elapsed,
            Errors: validationResult.Errors));
    }

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
        Guid sellerId,
        string sellerUsername,
        string currency,
        string correlationId,
        int priorSucceeded,
        int failedCount,
        CancellationToken cancellationToken)
    {
        var totalInserted = 0;
        var batches = ChunkRows(validRows, BatchSize);

        foreach (var batch in batches)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var auctions = MapToEntities(batch, sellerId, sellerUsername, currency);
            await _bulkRepository.BulkInsertAsync(auctions, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            totalInserted += auctions.Count;

            var lastRowNumber = batch[^1].RowNumber;

            await _checkpointRepository.SaveCheckpointAsync(
                new ImportCheckpoint(
                    correlationId,
                    lastRowNumber,
                    priorSucceeded + totalInserted,
                    failedCount,
                    _dateTime.UtcNowOffset),
                cancellationToken);

            _logger.LogInformation(
                "Processed batch: {Inserted}/{Total} auctions inserted, checkpoint at row {Row}",
                totalInserted, validRows.Count, lastRowNumber);
        }

        return totalInserted;
    }

    private async Task CleanupCheckpointAsync(string correlationId, CancellationToken cancellationToken)
    {
        await _checkpointRepository.DeleteCheckpointAsync(correlationId, cancellationToken);
    }

    private RowValidationResult ValidateAllRows(IReadOnlyList<ImportAuctionRow> rows, string currency)
    {
        var validRows = new List<ValidatedRow>();
        var errors = new List<ImportRowError>();

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNumber = i + 1;
            var row = rows[i];
            var rowErrors = ValidateSingleRow(row, rowNumber, currency);

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

    private static List<ImportRowError> ValidateSingleRow(ImportAuctionRow row, int rowNumber, string currency)
    {
        var errors = new List<ImportRowError>();

        if (string.IsNullOrWhiteSpace(row.Title))
            errors.Add(new ImportRowError(rowNumber, nameof(row.Title), "Title is required."));
        else if (row.Title.Length > ValidationConstants.StringLength.Medium)
            errors.Add(new ImportRowError(rowNumber, nameof(row.Title),
                $"Title must not exceed {ValidationConstants.StringLength.Medium} characters."));

        if (string.IsNullOrWhiteSpace(row.Description))
            errors.Add(new ImportRowError(rowNumber, nameof(row.Description), "Description is required."));

        if (row.ReservePrice < 0)
            errors.Add(new ImportRowError(rowNumber, nameof(row.ReservePrice),
                "Reserve price must be non-negative."));

        if (row.BuyNowPrice.HasValue && row.BuyNowPrice.Value <= row.ReservePrice)
            errors.Add(new ImportRowError(rowNumber, nameof(row.BuyNowPrice),
                "Buy now price must exceed reserve price."));

        if (row.AuctionEnd <= DateTimeOffset.UtcNow)
            errors.Add(new ImportRowError(rowNumber, nameof(row.AuctionEnd),
                "Auction end date must be in the future."));

        if (row.YearManufactured.HasValue && (row.YearManufactured < 1900 || row.YearManufactured > DateTime.UtcNow.Year + 1))
            errors.Add(new ImportRowError(rowNumber, nameof(row.YearManufactured),
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
            var row = validatedRow.Row;

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
        IReadOnlyList<ValidatedRow> rows,
        int chunkSize)
    {
        for (var i = 0; i < rows.Count; i += chunkSize)
        {
            var remaining = Math.Min(chunkSize, rows.Count - i);
            yield return rows.Skip(i).Take(remaining).ToList();
        }
    }

    private sealed record ValidatedRow(int RowNumber, ImportAuctionRow Row);
    private sealed record RowValidationResult(
        IReadOnlyList<ValidatedRow> ValidRows,
        IReadOnlyList<ImportRowError> Errors);
}
