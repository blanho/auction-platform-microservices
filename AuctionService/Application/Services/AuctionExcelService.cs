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
        "Id", "Title", "Description", "Make", "Model", "Year", "Color", "Mileage",
        "ReservePrice", "Seller", "Winner", "SoldAmount", "CurrentHighBid",
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

            return new ImportAuctionDto
            {
                Title = reader.GetString("Title"),
                Description = reader.GetString("Description"),
                Make = reader.GetString("Make"),
                Model = reader.GetString("Model"),
                Year = reader.GetInt("Year"),
                Color = reader.GetString("Color"),
                Mileage = reader.GetInt("Mileage"),
                ReservePrice = reader.GetInt("ReservePrice"),
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
                auction.Make,
                auction.Model,
                auction.Year,
                auction.Color,
                auction.Mileage,
                auction.ReservePrice,
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
