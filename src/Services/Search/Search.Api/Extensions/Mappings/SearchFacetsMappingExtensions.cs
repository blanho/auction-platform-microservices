using Elastic.Clients.Elasticsearch.Aggregations;
using Search.Api.Models;

namespace Search.Api.Extensions.Mappings;

public static class SearchFacetsMappingExtensions
{
    public static SearchFacets? ToFacets(this AggregateDictionary? aggregations)
    {
        if (aggregations == null)
            return null;

        var facets = new SearchFacets();

        if (aggregations.TryGetValue("categories", out var catAgg) &&
            catAgg is StringTermsAggregate catTerms)
        {
            facets.Categories = catTerms.Buckets.Select(b => new FacetBucket
            {
                Key = b.Key.ToString() ?? "",
                Count = b.DocCount
            }).ToList();
        }

        if (aggregations.TryGetValue("brands", out var brandAgg) &&
            brandAgg is StringTermsAggregate brandTerms)
        {
            facets.Brands = brandTerms.Buckets.Select(b => new FacetBucket
            {
                Key = b.Key.ToString() ?? "",
                Count = b.DocCount
            }).ToList();
        }

        if (aggregations.TryGetValue("conditions", out var condAgg) &&
            condAgg is StringTermsAggregate condTerms)
        {
            facets.Conditions = condTerms.Buckets.Select(b => new FacetBucket
            {
                Key = b.Key.ToString() ?? "",
                Count = b.DocCount
            }).ToList();
        }

        if (aggregations.TryGetValue("statuses", out var statusAgg) &&
            statusAgg is StringTermsAggregate statusTerms)
        {
            facets.Statuses = statusTerms.Buckets.Select(b => new FacetBucket
            {
                Key = b.Key.ToString() ?? "",
                Count = b.DocCount
            }).ToList();
        }

        if (aggregations.TryGetValue("price_stats", out var priceAgg) &&
            priceAgg is StatsAggregate priceStats)
        {
            facets.PriceRange = new PriceRangeFacet
            {
                Min = (decimal)(priceStats.Min ?? 0),
                Max = (decimal)(priceStats.Max ?? 0),
                Avg = (decimal)(priceStats.Avg ?? 0)
            };
        }

        return facets;
    }
}
