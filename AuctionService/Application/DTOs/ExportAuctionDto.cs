namespace AuctionService.Application.DTOs;
public class ExportAuctionDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string Color { get; set; }
    public int Mileage { get; set; }
    public int ReservePrice { get; set; }
    public required string Seller { get; set; }
    public string? Winner { get; set; }
    public int? SoldAmount { get; set; }
    public int? CurrentHighBid { get; set; }
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
