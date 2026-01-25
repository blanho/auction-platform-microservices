using Elastic.Clients.Elasticsearch.Core.Search;
using Search.Api.Documents;
using Search.Api.Models;

namespace Search.Api.Extensions.Mappings;

public static class AuctionSearchMappingExtensions
{
    public static AuctionSearchResult ToResult(this AuctionDocument doc, Hit<AuctionDocument>? hit = null)
    {
        return new AuctionSearchResult
        {
            Id = doc.Id,
            Title = doc.Title,
            Description = doc.Description?.Length > 200 ? doc.Description[..200] + "..." : doc.Description,
            ThumbnailUrl = doc.ThumbnailUrl,
            CurrentPrice = doc.CurrentPrice,
            BuyNowPrice = doc.BuyNowPrice,
            Currency = doc.Currency,
            Status = doc.Status,
            Condition = doc.Condition,
            EndTime = doc.EndTime,
            BidCount = doc.BidCount,
            CategoryName = doc.CategoryName,
            BrandName = doc.BrandName,
            SellerId = doc.SellerId,
            SellerUsername = doc.SellerUsername,
            Score = hit?.Score,
            Highlights = hit?.Highlight?.ToDictionary(
                h => h.Key,
                h => h.Value.ToList())
        };
    }

    public static List<AuctionSearchResult> ToResultList(
        this IEnumerable<AuctionDocument> documents, 
        IEnumerable<Hit<AuctionDocument>> hits)
    {
        return documents
            .Zip(hits, (doc, hit) => doc.ToResult(hit))
            .ToList();
    }
}
