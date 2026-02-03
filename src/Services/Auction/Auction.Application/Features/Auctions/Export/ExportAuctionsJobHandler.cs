using Auctions.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.BackgroundJobs.Core;
using BuildingBlocks.Application.Helpers;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Features.Auctions.Export;

public sealed class ExportAuctionsJobHandler : IBackgroundJobHandler
{
    private const int BatchSize = 5000;
    private readonly ILogger<ExportAuctionsJobHandler> _logger;

    public string JobType => BackgroundJobTypes.Export;

    public ExportAuctionsJobHandler(ILogger<ExportAuctionsJobHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(BackgroundJobContext context)
    {
        var ct = context.CancellationToken;
        var repository = context.GetService<IAuctionRepository>();
        var excelService = context.GetService<IExcelService>();

        var format = context.GetMetadata<string>("format") ?? "excel";

        _logger.LogInformation("Starting export job {JobId} with format {Format}", context.Job.Id, format);

        var totalCount = await repository.GetTotalCountAsync(ct);
        await context.ReportProgressAsync(0, totalCount);

        var allAuctions = new List<ExportAuctionRow>();
        var page = 1;
        var startTime = DateTimeOffset.UtcNow;

        while (!ct.IsCancellationRequested)
        {
            var pagedResult = await repository.GetPagedAsync(page, BatchSize, ct);

            if (!pagedResult.Items.Any())
                break;

            foreach (var auction in pagedResult.Items)
            {
                allAuctions.Add(MapToExportRow(auction));
            }

            var estimatedSeconds = BackgroundJobHelper.CalculateEstimatedSecondsRemaining(
                allAuctions.Count, totalCount, startTime);

            await context.ReportProgressAsync(allAuctions.Count, totalCount);

            if (allAuctions.Count % (BatchSize * 5) == 0)
            {
                await context.SetCheckpointAsync(
                    $"page:{page}",
                    resultFileName: null);
            }

            if (!pagedResult.HasNextPage)
                break;

            page++;
        }

        var fileBytes = GenerateExportFile(allAuctions, format, excelService);
        var extension = ExportHelper.GetFileExtension(format);
        var resultFileName = ExportHelper.GenerateExportFileName("auctions_export", extension);

        await BackgroundJobHelper.SaveResultFileAsync(context.Job, resultFileName, fileBytes, ct);

        _logger.LogInformation("Export job {JobId} completed. File: {FileName}, Size: {Size} bytes",
            context.Job.Id, resultFileName, fileBytes.Length);
    }

    private static ExportAuctionRow MapToExportRow(dynamic auction)
    {
        return new ExportAuctionRow
        {
            Id = auction.Id.ToString(),
            Title = auction.Item?.Title ?? "",
            Description = auction.Item?.Description ?? "",
            Condition = auction.Item?.Condition,
            ReservePrice = auction.ReservePrice,
            CurrentHighBid = auction.CurrentHighBid,
            Currency = auction.Currency,
            Seller = auction.SellerUsername,
            Status = auction.Status.ToString(),
            CreatedAt = auction.CreatedAt.ToString("O"),
            AuctionEnd = auction.AuctionEnd.ToString("O")
        };
    }

    private static byte[] GenerateExportFile(
        List<ExportAuctionRow> data,
        string format,
        IExcelService excelService)
    {
        var headers = new[]
        {
            "Id", "Title", "Description", "Condition", "ReservePrice",
            "CurrentHighBid", "Currency", "Seller", "Status", "CreatedAt", "AuctionEnd"
        };

        return format.ToLowerInvariant() switch
        {
            "csv" => ExportHelper.GenerateCsv(data, headers, row => new[]
            {
                row.Id, row.Title, row.Description, row.Condition, row.ReservePrice.ToString(),
                row.CurrentHighBid?.ToString(), row.Currency, row.Seller, row.Status, row.CreatedAt, row.AuctionEnd
            }),
            "json" => ExportHelper.GenerateJson(data),
            _ => excelService.GenerateFile(data, headers, row => new object?[]
            {
                row.Id, row.Title, row.Description, row.Condition, row.ReservePrice,
                row.CurrentHighBid, row.Currency, row.Seller, row.Status, row.CreatedAt, row.AuctionEnd
            })
        };
    }
}

public sealed class ExportAuctionRow
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string? Condition { get; init; }
    public decimal ReservePrice { get; init; }
    public decimal? CurrentHighBid { get; init; }
    public string Currency { get; init; } = "USD";
    public string Seller { get; init; } = "";
    public string Status { get; init; } = "";
    public string CreatedAt { get; init; } = "";
    public string AuctionEnd { get; init; } = "";
}
