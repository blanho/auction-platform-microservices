namespace AuctionService.Application.DTOs.Requests;

/// <summary>
/// Request DTO for querying auctions with filtering, sorting, and pagination.
/// </summary>
public class GetAuctionsRequest
{
    /// <summary>
    /// Filter by auction status (Live, ReserveNotMet, Finished, etc.)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by seller username
    /// </summary>
    public string? Seller { get; set; }

    /// <summary>
    /// Filter by winner username
    /// </summary>
    public string? Winner { get; set; }

    /// <summary>
    /// Search term for auction title, description, or make/model
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Page number (1-based). Default: 1
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Default: 10
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Field to order by (e.g., "make", "auctionEnd", "currentHighBid")
    /// </summary>
    public string? OrderBy { get; set; }

    /// <summary>
    /// Sort in descending order. Default: false
    /// </summary>
    public bool Descending { get; set; } = false;
}
