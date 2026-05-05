namespace Search.Api.Constants;

public static class ElasticsearchFields
{
    public const string Id = "id";
    public const string Title = "title";
    public const string TitleKeyword = "title.keyword";
    public const string TitleAutocomplete = "title.autocomplete";
    public const string Description = "description";
    
    public const string CategoryId = "categoryId";
    public const string CategoryName = "categoryName";
    public const string CategoryPath = "categoryPath";
    
    public const string BrandId = "brandId";
    public const string BrandName = "brandName";
    
    public const string SellerId = "sellerId";
    public const string SellerUsername = "sellerUsername";
    
    public const string CurrentPrice = "currentPrice";
    public const string StartPrice = "startPrice";
    public const string ReservePrice = "reservePrice";
    public const string BuyNowPrice = "buyNowPrice";
    
    public const string Status = "status";
    public const string Condition = "condition";
    
    public const string StartTime = "startTime";
    public const string EndTime = "endTime";
    public const string CreatedAt = "createdAt";
    public const string UpdatedAt = "updatedAt";
    public const string LastSyncedAt = "lastSyncedAt";
    
    public const string BidCount = "bidCount";
    public const string ViewCount = "viewCount";
    public const string WatchCount = "watchCount";
    
    public const string WinnerId = "winnerId";
    public const string WinnerUsername = "winnerUsername";
    public const string FinalPrice = "finalPrice";
    
    public const string IsFeatured = "isFeatured";
    public const string Tags = "tags";
    public const string ThumbnailUrl = "thumbnailUrl";
    public const string ImageUrls = "imageUrls";
    
    public const string ScoreField = "_score";
}

public static class AggregationNames
{
    public const string Categories = "categories";
    public const string Brands = "brands";
    public const string Conditions = "conditions";
    public const string Statuses = "statuses";
    public const string PriceStats = "price_stats";
}

public static class SearchDefaults
{
    public const int MaxDescriptionPreviewLength = 200;
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxCategoryAggregations = 50;
    public const int MaxBrandAggregations = 50;
    public const int MaxConditionAggregations = 10;
    public const int MaxStatusAggregations = 10;
    public const int MinAutocompleteLength = 2;
    public const int DefaultAutocompleteLimit = 10;
    public const int MaxRecentSearchesPerUser = 10;
    public const int MaxPopularSearches = 10;

    public const int TitleBoost = 3;
    public const int CategoryBoost = 2;
    public const int BrandBoost = 2;

    public const string HighlightPreTag = "<em>";
    public const string HighlightPostTag = "</em>";

    public const int NgramMinLength = 2;
    public const int NgramMaxLength = 20;

    public const string IndexHealthNone = "none";
    public const string IndexHealthUnknown = "unknown";
    public const string IndexHealthGreen = "green";

    public const long ZeroDocCount = 0;
    public const long ZeroSizeBytes = 0;
}

public static class SortFields
{
    public const string Price = "price";
    public const string EndTime = "endtime";
    public const string Bids = "bids";
    public const string Created = "created";
}
