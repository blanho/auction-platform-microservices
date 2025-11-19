namespace Search.Api.Models;

public class AuctionSearchRequest
{

    public string? Query { get; set; }

    public Guid? CategoryId { get; set; }

    public string? CategorySlug { get; set; }

    public Guid? BrandId { get; set; }

    public Guid? SellerId { get; set; }

    public List<string>? Statuses { get; set; }

    public List<string>? Conditions { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public DateTimeOffset? EndingAfter { get; set; }
    public DateTimeOffset? EndingBefore { get; set; }

    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }

    public string? Category { get; set; }

    public string? Brand { get; set; }

    public string? Status { get; set; }

    public bool FeaturedOnly { get; set; } = false;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public List<object>? SearchAfter { get; set; }

    public bool IncludeFacets { get; set; } = false;
}

public class AuctionSearchResponse
{
    public List<AuctionSearchResult> Results { get; set; } = new();
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasMore => Page < TotalPages;

    public static AuctionSearchResponse Empty(int page, int pageSize) => new()
    {
        Results = new List<AuctionSearchResult>(),
        TotalCount = 0,
        Page = page,
        PageSize = pageSize,
        TotalPages = 0
    };

    public List<object>? NextCursor { get; set; }

    public SearchFacets? Facets { get; set; }

    public long Took { get; set; }
}

public class AuctionSearchResult
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }

    public decimal CurrentPrice { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public string Currency { get; set; } = "USD";

    public string Status { get; set; } = string.Empty;
    public string? Condition { get; set; }

    public DateTimeOffset? EndTime { get; set; }
    public int BidCount { get; set; }

    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }

    public Guid SellerId { get; set; }
    public string SellerUsername { get; set; } = string.Empty;

    public double? Score { get; set; }

    public Dictionary<string, List<string>>? Highlights { get; set; }
}

public class SearchFacets
{
    public List<FacetBucket> Categories { get; set; } = new();
    public List<FacetBucket> Brands { get; set; } = new();
    public List<FacetBucket> Conditions { get; set; } = new();
    public List<FacetBucket> Statuses { get; set; } = new();
    public PriceRangeFacet? PriceRange { get; set; }
}

public class FacetBucket
{
    public string Key { get; set; } = string.Empty;
    public string? Label { get; set; }
    public long Count { get; set; }
}

public class PriceRangeFacet
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Avg { get; set; }
}

public class AutocompleteSuggestion
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? ThumbnailUrl { get; set; }
}
