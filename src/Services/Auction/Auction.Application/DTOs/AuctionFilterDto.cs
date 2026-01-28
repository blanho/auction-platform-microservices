namespace Auctions.Application.DTOs;

public class AuctionFilterDto
{
    public string? Status { get; set; }
    public string? Seller { get; set; }
    public string? Winner { get; set; }
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public bool? IsFeatured { get; set; }
    public string? OrderBy { get; set; }
    public bool Descending { get; set; } = true;
    public int PageNumber { get; set; } = PaginationDefaults.DefaultPage;
    public int PageSize { get; set; } = PaginationDefaults.DefaultPageSize;
}
