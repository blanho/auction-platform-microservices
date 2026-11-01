using Auctions.Application.Errors;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Features.Auctions.ExportAuctions;

public class ExportAuctionsCommandHandler : ICommandHandler<ExportAuctionsCommand, ExportAuctionsResult>
{
    private readonly IAuctionReadRepository _readRepository;
    private readonly IEnumerable<IReportExporter> _exporters;
    private readonly ILogger<ExportAuctionsCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;

    public ExportAuctionsCommandHandler(
        IAuctionReadRepository readRepository,
        IEnumerable<IReportExporter> exporters,
        ILogger<ExportAuctionsCommandHandler> logger,
        IDateTimeProvider dateTime)
    {
        _readRepository = readRepository;
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

        var exporter = _exporters.FirstOrDefault(candidate => candidate.Format == request.Format);
        if (exporter is null)
        {
            return Result.Failure<ExportAuctionsResult>(
                AuctionErrors.Export.UnsupportedFormat(request.Format.ToString()));
        }

        var auctions = await _readRepository.GetAuctionsForExportAsync(
            request.StatusFilter,
            request.SellerFilter,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        var exportRows = ExportAuctionMapper.ToRows(auctions);
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

}
