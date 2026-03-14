using Elastic.Clients.Elasticsearch.Aggregations;
using Search.Api.Constants;
using Search.Api.Models;

namespace Search.Api.Extensions.Mappings;

public static class SearchFacetsMappingExtensions
{
    public static SearchFacets? ToFacets(this AggregateDictionary? aggregations)
    {
        if (aggregations is null)
            return null;

        return new SearchFacets
        {
            Categories = ExtractTermBuckets(aggregations, AggregationNames.Categories),
            Brands = ExtractTermBuckets(aggregations, AggregationNames.Brands),
            Conditions = ExtractTermBuckets(aggregations, AggregationNames.Conditions),
            Statuses = ExtractTermBuckets(aggregations, AggregationNames.Statuses),
            PriceRange = ExtractPriceRangeFacet(aggregations)
        };
    }

    private static List<FacetBucket> ExtractTermBuckets(
        AggregateDictionary aggregations, 
        string aggregationName)
    {
        if (!aggregations.TryGetValue(aggregationName, out var aggregate))
            return new List<FacetBucket>();

        if (aggregate is not StringTermsAggregate termsAggregate)
            return new List<FacetBucket>();

        return termsAggregate.Buckets
            .Select(MapToFacetBucket)
            .ToList();
    }

    private static FacetBucket MapToFacetBucket(StringTermsBucket bucket) =>
        new()
        {
            Key = bucket.Key.ToString() ?? string.Empty,
            Count = bucket.DocCount
        };

    private static PriceRangeFacet? ExtractPriceRangeFacet(AggregateDictionary aggregations)
    {
        if (!aggregations.TryGetValue(AggregationNames.PriceStats, out var aggregate))
            return null;

        if (aggregate is not StatsAggregate priceStats)
            return null;

        return new PriceRangeFacet
        {
            Min = (decimal)(priceStats.Min ?? 0),
            Max = (decimal)(priceStats.Max ?? 0),
            Avg = (decimal)(priceStats.Avg ?? 0)
        };
    }
}
