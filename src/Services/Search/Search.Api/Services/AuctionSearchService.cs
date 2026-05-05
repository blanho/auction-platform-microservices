using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Options;
using Search.Api.Configuration;
using Search.Api.Constants;
using Search.Api.Documents;
using Search.Api.Interfaces;
using Search.Api.Models;

namespace Search.Api.Services;

public class AuctionSearchService : IAuctionSearchService
{
    private static readonly string[] MultiMatchFields =
    [
        $"{ElasticsearchFields.Title}^{SearchDefaults.TitleBoost}",
        ElasticsearchFields.Description,
        $"{ElasticsearchFields.CategoryName}^{SearchDefaults.CategoryBoost}",
        $"{ElasticsearchFields.BrandName}^{SearchDefaults.BrandBoost}",
        ElasticsearchFields.Tags
    ];

    private readonly ElasticsearchClient _client;
    private readonly SearchOptions _options;
    private readonly string _indexName;
    private readonly ILogger<AuctionSearchService> _logger;

    public AuctionSearchService(
        ElasticsearchClient client,
        IOptions<SearchOptions> options,
        IOptions<ElasticsearchOptions> esOptions,
        ILogger<AuctionSearchService> logger)
    {
        _client = client;
        _options = options.Value;
        _indexName = esOptions.Value.GetIndexName();
        _logger = logger;
    }

    public async Task<AuctionSearchResponse> SearchAsync(
        AuctionSearchRequest request,
        CancellationToken ct = default)
    {
        var pageSize = Math.Min(request.PageSize, _options.MaxPageSize);

        try
        {
            var response = await _client.SearchAsync<AuctionDocument>(s => s
                .Index(_indexName)
                .From((request.Page - 1) * pageSize)
                .Size(pageSize)
                .Query(q => BuildQuery(request))
                .Highlight(h => h
                    .PreTags(new[] { SearchDefaults.HighlightPreTag })
                    .PostTags(new[] { SearchDefaults.HighlightPostTag })
                    .Fields(f => f
                        .Add(ElasticsearchFields.Title, hf => { })
                        .Add(ElasticsearchFields.Description, hf => { })))
                .Sort(BuildSortOptions(request.SortBy, request.SortDirection))
                .TrackTotalHits(new TrackHits(true))
                .Aggregations(agg => agg
                    .Add(AggregationNames.Categories, a => a
                        .Terms(t => t.Field(ElasticsearchFields.CategoryName).Size(SearchDefaults.MaxCategoryAggregations)))
                    .Add(AggregationNames.Brands, a => a
                        .Terms(t => t.Field(ElasticsearchFields.BrandName).Size(SearchDefaults.MaxBrandAggregations)))
                    .Add(AggregationNames.Conditions, a => a
                        .Terms(t => t.Field(ElasticsearchFields.Condition).Size(SearchDefaults.MaxConditionAggregations)))
                    .Add(AggregationNames.Statuses, a => a
                        .Terms(t => t.Field(ElasticsearchFields.Status).Size(SearchDefaults.MaxStatusAggregations)))
                    .Add(AggregationNames.PriceStats, a => a
                        .Stats(s => s.Field(ElasticsearchFields.CurrentPrice)))),
            ct);

            if (!response.IsValidResponse)
            {
                _logger.LogError("Search failed: {Error}", response.DebugInformation);
                return AuctionSearchResponse.Empty(request.Page, pageSize);
            }

            var results = response.Documents.ToResultList(response.Hits);
            var totalHits = response.Total;

            return new AuctionSearchResponse
            {
                Results = results,
                TotalCount = totalHits,
                Page = request.Page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalHits / pageSize),
                Facets = response.Aggregations.ToFacets(),
                Took = response.Took
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search error");
            throw;
        }
    }

    public async Task<AuctionSearchResult?> GetByIdAsync(
        Guid auctionId,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetAsync<AuctionDocument>(auctionId.ToString(), g => g
                .Index(_indexName),
            ct);

            if (!response.IsValidResponse || response.Source == null)
            {
                return null;
            }

            return response.Source.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get by ID error for auction {AuctionId}", auctionId);
            return null;
        }
    }

    public async Task<IReadOnlyList<AutocompleteSuggestion>> AutocompleteAsync(
        string prefix,
        int maxSuggestions = SearchDefaults.DefaultAutocompleteLimit,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < SearchDefaults.MinAutocompleteLength)
            return Array.Empty<AutocompleteSuggestion>();

        try
        {
            var response = await _client.SearchAsync<AuctionDocument>(s => s
                .Index(_indexName)
                .Size(maxSuggestions)
                .Source(new SourceConfig(new SourceFilter
                {
                    Includes = new[] 
                    { 
                        ElasticsearchFields.Id, 
                        ElasticsearchFields.Title, 
                        ElasticsearchFields.CategoryName, 
                        ElasticsearchFields.ThumbnailUrl 
                    }
                }))
                .Query(q => q.Match(m => m
                    .Field(ElasticsearchFields.TitleAutocomplete)
                    .Query(prefix))),
            ct);

            if (!response.IsValidResponse)
                return Array.Empty<AutocompleteSuggestion>();

            return MapToAutocompleteSuggestions(response.Documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Autocomplete error for prefix: {Prefix}", prefix);
            return Array.Empty<AutocompleteSuggestion>();
        }
    }

    private static List<AutocompleteSuggestion> MapToAutocompleteSuggestions(
        IEnumerable<AuctionDocument> documents) =>
        documents.Select(d => new AutocompleteSuggestion
        {
            Id = d.Id,
            Title = d.Title,
            Category = d.CategoryName,
            ThumbnailUrl = d.ThumbnailUrl
        }).ToList();

    private static Query BuildQuery(AuctionSearchRequest request)
    {
        var mustQueries = BuildMustQueries(request);
        var filterQueries = BuildFilterQueries(request);

        return Query.Bool(new BoolQuery
        {
            Must = mustQueries.Count > 0 ? mustQueries : null,
            Filter = filterQueries
        });
    }

    private static List<Query> BuildMustQueries(AuctionSearchRequest request)
    {
        var queries = new List<Query>();

        if (!string.IsNullOrWhiteSpace(request.Query))
            queries.Add(BuildMultiMatchQuery(request.Query));

        return queries;
    }

    private static Query BuildMultiMatchQuery(string searchQuery) =>
        Query.MultiMatch(new MultiMatchQuery
        {
            Query = searchQuery,
            Fields = MultiMatchFields,
            Type = TextQueryType.BestFields,
            Fuzziness = new Fuzziness("AUTO")
        });

    private static List<Query> BuildFilterQueries(AuctionSearchRequest request)
    {
        var filters = new List<Query>();

        AddTermFilter(filters, ElasticsearchFields.CategoryName, request.Category);
        AddTermFilter(filters, ElasticsearchFields.CategoryId, request.CategoryId);
        AddTermFilter(filters, ElasticsearchFields.BrandName, request.Brand);
        AddTermFilter(filters, ElasticsearchFields.BrandId, request.BrandId);
        AddTermFilter(filters, ElasticsearchFields.SellerId, request.SellerId);
        AddTermFilter(filters, ElasticsearchFields.Status, request.Status);
        AddTermFilter(filters, ElasticsearchFields.Condition, request.Condition);
        AddPriceRangeFilter(filters, request.MinPrice, request.MaxPrice);
        AddEndTimeFilter(filters, request.EndingAfter, request.EndingBefore);
        AddFeaturedFilter(filters, request.FeaturedOnly);

        return filters;
    }

    private static void AddTermFilter(List<Query> filters, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        filters.Add(Query.Term(new TermQuery(fieldName) { Value = value }));
    }

    private static void AddTermFilter(List<Query> filters, string fieldName, Guid? id)
    {
        if (!id.HasValue)
            return;

        filters.Add(Query.Term(new TermQuery(fieldName) { Value = id.Value.ToString() }));
    }

    private static void AddPriceRangeFilter(List<Query> filters, decimal? minPrice, decimal? maxPrice)
    {
        if (!minPrice.HasValue && !maxPrice.HasValue)
            return;

        var rangeQuery = new NumberRangeQuery(ElasticsearchFields.CurrentPrice);

        if (minPrice.HasValue)
            rangeQuery.Gte = (double)minPrice.Value;

        if (maxPrice.HasValue)
            rangeQuery.Lte = (double)maxPrice.Value;

        filters.Add(Query.Range(rangeQuery));
    }

    private static void AddEndTimeFilter(
        List<Query> filters, 
        DateTimeOffset? endingAfter, 
        DateTimeOffset? endingBefore)
    {
        if (!endingAfter.HasValue && !endingBefore.HasValue)
            return;

        var rangeQuery = new DateRangeQuery(ElasticsearchFields.EndTime);

        if (endingAfter.HasValue)
            rangeQuery.Gte = endingAfter.Value.ToString("o");

        if (endingBefore.HasValue)
            rangeQuery.Lte = endingBefore.Value.ToString("o");

        filters.Add(Query.Range(rangeQuery));
    }

    private static void AddFeaturedFilter(List<Query> filters, bool featuredOnly)
    {
        if (!featuredOnly)
            return;

        filters.Add(Query.Term(new TermQuery(ElasticsearchFields.IsFeatured)
        {
            Value = true
        }));
    }

    private static ICollection<SortOptions> BuildSortOptions(string? sortBy, string? sortDirection)
    {
        var direction = ParseSortDirection(sortDirection);
        var sortField = MapSortFieldName(sortBy);

        return IsRelevanceSort(sortField)
            ? BuildRelevanceSortOptions()
            : BuildFieldSortOptions(sortField, direction);
    }

    private static SortOrder ParseSortDirection(string? sortDirection) =>
        sortDirection?.ToLowerInvariant() == "asc" 
            ? SortOrder.Asc 
            : SortOrder.Desc;

    private static string MapSortFieldName(string? sortBy) =>
        sortBy?.ToLowerInvariant() switch
        {
            SortFields.Price => ElasticsearchFields.CurrentPrice,
            SortFields.EndTime => ElasticsearchFields.EndTime,
            SortFields.Bids => ElasticsearchFields.BidCount,
            SortFields.Created => ElasticsearchFields.CreatedAt,
            _ => ElasticsearchFields.ScoreField
        };

    private static bool IsRelevanceSort(string sortField) =>
        sortField == ElasticsearchFields.ScoreField;

    private static List<SortOptions> BuildRelevanceSortOptions() =>
        new()
        {
            SortOptions.Score(new ScoreSort { Order = SortOrder.Desc }),
            SortOptions.Field(ElasticsearchFields.CreatedAt, new FieldSort { Order = SortOrder.Desc }),
            SortOptions.Field(ElasticsearchFields.Id, new FieldSort { Order = SortOrder.Asc })
        };

    private static List<SortOptions> BuildFieldSortOptions(string sortField, SortOrder direction) =>
        new()
        {
            SortOptions.Field(sortField, new FieldSort { Order = direction }),
            SortOptions.Field(ElasticsearchFields.Id, new FieldSort { Order = SortOrder.Asc })
        };

}
