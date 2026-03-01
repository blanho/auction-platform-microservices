using System.Diagnostics;
using System.Runtime.CompilerServices;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Features.Auctions.ExportAuctions.Streaming;

public sealed class ExportAuctionsStreamHandler
    : ICommandHandler<ExportAuctionsStreamCommand, ExportAuctionsStreamResult>
{
    private readonly IAuctionStreamingExportRepository _streamingRepository;
    private readonly IEnumerable<IStreamingReportExporter> _exporters;
    private readonly ILogger<ExportAuctionsStreamHandler> _logger;
    private readonly IDateTimeProvider _dateTime;

    public ExportAuctionsStreamHandler(
        IAuctionStreamingExportRepository streamingRepository,
        IEnumerable<IStreamingReportExporter> exporters,
        ILogger<ExportAuctionsStreamHandler> logger,
        IDateTimeProvider dateTime)
    {
        _streamingRepository = streamingRepository;
        _exporters = exporters;
        _logger = logger;
        _dateTime = dateTime;
    }

    public async Task<Result<ExportAuctionsStreamResult>> Handle(
        ExportAuctionsStreamCommand request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Starting streaming export in {Format} format, batch size {BatchSize}",
            request.Format, request.BatchSize);

        var exporter = ResolveExporter(request.Format);
        if (exporter is null)
        {
            return Result.Failure<ExportAuctionsStreamResult>(
                Error.Create("Export.UnsupportedFormat",
                    $"Export format '{request.Format}' is not supported."));
        }

        var auctionStream = _streamingRepository.StreamAuctionsForExportAsync(
            request.StatusFilter,
            request.SellerFilter,
            request.StartDate,
            request.EndDate,
            request.BatchSize,
            cancellationToken);

        var exportRowStream = MapToExportRowsAsync(auctionStream, cancellationToken);

        var totalRecords = 0;

        var countingStream = CountRecordsAsync(exportRowStream, count => totalRecords = count, cancellationToken);

        await exporter.ExportAsync(countingStream, request.OutputStream, cancellationToken);

        stopwatch.Stop();

        var timestamp = _dateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var fileName = $"auctions-export-{timestamp}{exporter.FileExtension}";

        _logger.LogInformation(
            "Streaming export completed: {RecordCount} auctions in {Duration}ms",
            totalRecords, stopwatch.ElapsedMilliseconds);

        return Result<ExportAuctionsStreamResult>.Success(new ExportAuctionsStreamResult(
            FileName: fileName,
            ContentType: exporter.ContentType,
            TotalRecords: totalRecords,
            Format: request.Format,
            Duration: stopwatch.Elapsed));
    }

    private IStreamingReportExporter? ResolveExporter(ExportFormat format)
    {
        return _exporters.FirstOrDefault(e => e.Format == format);
    }

    private static async IAsyncEnumerable<ExportAuctionRow> MapToExportRowsAsync(
        IAsyncEnumerable<Domain.Entities.Auction> auctions,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var auction in auctions.WithCancellation(cancellationToken))
        {
            yield return new ExportAuctionRow(
                AuctionId: auction.Id,
                Title: auction.Item.Title,
                Seller: auction.SellerUsername,
                Status: auction.Status.ToString(),
                Currency: auction.Currency,
                ReservePrice: auction.ReservePrice,
                CurrentHighBid: auction.CurrentHighBid,
                SoldAmount: auction.SoldAmount,
                CreatedAt: auction.CreatedAt,
                AuctionEnd: auction.AuctionEnd,
                Category: auction.Item.Category?.Name,
                Condition: auction.Item.Condition);
        }
    }

    private static async IAsyncEnumerable<ExportAuctionRow> CountRecordsAsync(
        IAsyncEnumerable<ExportAuctionRow> source,
        Action<int> onComplete,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var count = 0;

        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            count++;
            yield return item;
        }

        onComplete(count);
    }
}
