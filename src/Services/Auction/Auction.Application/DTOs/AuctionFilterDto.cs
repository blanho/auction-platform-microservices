using BuildingBlocks.Application.Paging;

namespace Auctions.Application.DTOs;

public class AuctionFilter
{
    public string? Status { get; init; }
    public string? Seller { get; init; }
    public string? Winner { get; init; }
    public string? SearchTerm { get; init; }
    public string? Category { get; init; }
    public bool? IsFeatured { get; init; }
}

public class AuctionFilterDto : QueryParameters<AuctionFilter> { }
