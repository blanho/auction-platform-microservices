#nullable enable
using AuctionService.Application.DTOs;
using Common.Utilities.Excel;
using Microsoft.Extensions.Logging;
using System.Reflection;
using IExcelService = Common.Utilities.Excel.IExcelService;

namespace AuctionService.Application.Services;
public interface IAuctionExcelService
{
    List<ImportAuctionDto> ParseImportFile(Stream fileStream);
    byte[] GenerateExportFile(List<ExportAuctionDto> auctions);
    byte[] GetImportTemplate();
}

public class AuctionExcelService : IAuctionExcelService
{
    private readonly IExcelService _excelService;
    private readonly ILogger<AuctionExcelService> _logger;

    private const string TemplateResourceName = "AuctionService.Application.Templates.auction_import_template.xlsx";

    private static readonly string[] ExportHeaders =
    {
        "Id", "Title", "Description", "Condition", "YearManufactured",
        "ReservePrice", "Currency", "Seller", "Winner", "SoldAmount", "CurrentHighBid",
        "CreatedAt", "AuctionEnd", "Status"
    };

    public AuctionExcelService(IExcelService excelService, ILogger<AuctionExcelService> logger)
    {
        _excelService = excelService;
        _logger = logger;
    }

    public List<ImportAuctionDto> ParseImportFile(Stream fileStream)
    {
        var auctions = _excelService.ParseFile<ImportAuctionDto>(fileStream, reader =>
        {
            if (reader.IsEmpty)
                return null;

            var yearManufacturedStr = reader.GetString("YearManufactured");
            int? yearManufactured = string.IsNullOrWhiteSpace(yearManufacturedStr) 
                ? null 
                : int.TryParse(yearManufacturedStr, out var year) ? year : null;

            return new ImportAuctionDto
            {
                Title = reader.GetString("Title"),
                Description = reader.GetString("Description"),
                Condition = reader.GetString("Condition"),
                YearManufactured = yearManufactured,
                ReservePrice = reader.GetDecimal("ReservePrice"),
                Currency = reader.GetString("Currency") ?? "USD",
                AuctionEnd = reader.GetDateTimeOffset("AuctionEnd") ?? DateTimeOffset.UtcNow.AddDays(7)
            };
        });

        _logger.LogInformation("Parsed {Count} auctions from Excel file", auctions.Count);
        return auctions;
    }

    public byte[] GenerateExportFile(List<ExportAuctionDto> auctions)
    {
        var config = new ExcelExportConfig
        {
            SheetName = "Auctions",
            HeaderBackgroundColor = "ADD8E6",
            AutoFitColumns = true,
            AddAutoFilter = true,
            FreezeHeaderRow = true
        };

        var bytes = _excelService.GenerateFile(
            auctions,
            ExportHeaders,
            auction => new object?[]
            {
                auction.Id.ToString(),
                auction.Title,
                auction.Description,
                auction.Condition,
                auction.YearManufactured,
                auction.ReservePrice,
                auction.Currency,
                auction.Seller,
                auction.Winner,
                auction.SoldAmount,
                auction.CurrentHighBid,
                auction.CreatedAt,
                auction.AuctionEnd,
                auction.Status
            },
            config);

        _logger.LogInformation("Generated Excel export with {Count} auctions", auctions.Count);
        return bytes;
    }

    public byte[] GetImportTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        using var stream = assembly.GetManifestResourceStream(TemplateResourceName);
        
        if (stream == null)
        {
            _logger.LogError("Import template not found: {ResourceName}", TemplateResourceName);
            throw new FileNotFoundException($"Import template not found: {TemplateResourceName}");
        }

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        
        _logger.LogInformation("Retrieved import template from embedded resource");
        return memoryStream.ToArray();
    }
}
