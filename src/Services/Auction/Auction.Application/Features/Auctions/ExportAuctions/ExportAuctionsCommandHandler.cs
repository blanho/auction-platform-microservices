using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Commands.ExportAuctions;

public class ExportAuctionsCommandHandler : ICommandHandler<ExportAuctionsCommand, ExportAuctionsResult>
{
    private readonly IAuctionExportRepository _exportRepository;
    private readonly IEnumerable<IReportExporter> _exporters;
    private readonly ILogger<ExportAuctionsCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;

    public ExportAuctionsCommandHandler(
        IAuctionExportRepository exportRepository,
        IEnumerable<IReportExporter> exporters,
        ILogger<ExportAuctionsCommandHandler> logger,
        IDateTimeProvider dateTime)
    {
        _exportRepository = exportRepository;
        _exporters = exporters;
        _logger = logger;
        _dateTime = dateTime;
    }

    public async Task<Result<ExportAuctionsResult>> Handle(
        ExportAuctionsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Exporting auctions in {Format} format with filters: Status={Status}, Seller={Seller}",
            request.Format, request.StatusFilter, request.SellerFilter);

        var exporter = ResolveExporter(request.Format);
        if (exporter is null)
        {
            return Result.Failure<ExportAuctionsResult>(
                Error.Create("Export.UnsupportedFormat",
                    $"Export format '{request.Format}' is not supported."));
        }

        var auctions = await _exportRepository.GetAuctionsForExportAsync(
            request.StatusFilter,
            request.SellerFilter,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        var exportRows = MapToExportRows(auctions);
        var content = exporter.Export(exportRows);

        var timestamp = _dateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var fileName = $"auctions-export-{timestamp}{exporter.FileExtension}";

        _logger.LogInformation(
            "Export completed: {RecordCount} auctions exported as {Format} ({Size} bytes)",
            exportRows.Count, request.Format, content.Length);

        return Result<ExportAuctionsResult>.Success(new ExportAuctionsResult(
            Content: content,
            FileName: fileName,
            ContentType: exporter.ContentType,
            TotalRecords: exportRows.Count,
            Format: request.Format));
    }

    private IReportExporter? ResolveExporter(ExportFormat format)
    {
        return _exporters.FirstOrDefault(e => e.Format == format);
    }

    private static List<ExportAuctionRow> MapToExportRows(List<Auction> auctions)
    {
        return auctions.Select(a => new ExportAuctionRow(
            AuctionId: a.Id,
            Title: a.Item.Title,
            Seller: a.SellerUsername,
            Status: a.Status.ToString(),
            Currency: a.Currency,
            ReservePrice: a.ReservePrice,
            CurrentHighBid: a.CurrentHighBid,
            SoldAmount: a.SoldAmount,
            CreatedAt: a.CreatedAt,
            AuctionEnd: a.AuctionEnd,
            Category: a.Item.Category?.Name,
            Condition: a.Item.Condition)).ToList();
    }
}
