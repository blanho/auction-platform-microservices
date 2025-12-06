namespace AuctionService.Application.DTOs;
public class ImportAuctionDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string Color { get; set; }
    public int Mileage { get; set; }
    public int ReservePrice { get; set; }
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
