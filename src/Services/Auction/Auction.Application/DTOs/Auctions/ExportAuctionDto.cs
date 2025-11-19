namespace Auctions.Application.DTOs.Auctions;
public class ExportAuctionDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? Condition { get; set; }
    public int? YearManufactured { get; set; }
    public decimal ReservePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public required string Seller { get; set; }
    public string? Winner { get; set; }
    public decimal? SoldAmount { get; set; }
    public decimal? CurrentHighBid { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset AuctionEnd { get; set; }
    public required string Status { get; set; }
}

public class ExportQueryDto
{
    public string? Status { get; set; }
    public string? Seller { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string Format { get; set; } = "json"; 
}

