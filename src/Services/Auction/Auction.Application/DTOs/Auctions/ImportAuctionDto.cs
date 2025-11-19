namespace Auctions.Application.DTOs.Auctions;
public class ImportAuctionDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? Condition { get; set; }
    public int? YearManufactured { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
    public decimal ReservePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTimeOffset AuctionEnd { get; set; }
}

public class ImportAuctionResultDto
{
    public int RowNumber { get; set; }
    public bool Success { get; set; }
    public Guid? AuctionId { get; set; }
    public string? Error { get; set; }
}

public class ImportAuctionsResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<ImportAuctionResultDto> Results { get; set; } = new();
}

