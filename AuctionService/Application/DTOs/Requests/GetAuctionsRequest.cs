namespace AuctionService.Application.DTOs.Requests;

public class GetAuctionsRequest
{
    public string? Status { get; set; }
    public string? Seller { get; set; }
    public string? Winner { get; set; }
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public bool? IsFeatured { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? OrderBy { get; set; }
    public bool Descending { get; set; } = false;
}
