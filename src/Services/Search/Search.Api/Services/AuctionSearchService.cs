using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Options;
using Search.Api.Configuration;
using Search.Api.Documents;
using Search.Api.Interfaces;
using Search.Api.Models;

namespace Search.Api.Services;

public class AuctionSearchService : IAuctionSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly SearchOptions _options;
    private readonly ILogger<AuctionSearchService> _logger;

    public AuctionSearchService(
        ElasticsearchClient client,
        IOptions<SearchOptions> options,
        ILogger<AuctionSearchService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AuctionSearchResponse> SearchAsync(
        AuctionSearchRequest request,
        CancellationToken ct = default)
    {
        var indexName = IndexManagementService.GetIndexName();
        var pageSize = Math.Min(request.PageSize, _options.MaxPageSize);

        try
        {
            var response = await _client.SearchAsync<AuctionDocument>(s => s
                .Index(indexName)
                .From((request.Page - 1) * pageSize)
                .Size(pageSize)
                .Query(q => BuildQuery(request))
                .Highlight(h => h
                    .PreTags(new[] { "<em>" })
                    .PostTags(new[] { "</em>" })
                    .Fields(f => f
                        .Add("title", hf => { })
                        .Add("description", hf => { })))
                .Sort(BuildSortOptions(request.SortBy, request.SortDirection))
                .TrackTotalHits(new TrackHits(true))
                .Aggregations(agg => agg
                    .Add("categories", a => a.Terms(t => t.Field("categoryName").Size(50)))
                    .Add("brands", a => a.Terms(t => t.Field("brandName").Size(50)))
                    .Add("conditions", a => a.Terms(t => t.Field("condition").Size(10)))
                    .Add("statuses", a => a.Terms(t => t.Field("status").Size(10)))
                    .Add("price_stats", a => a.Stats(s => s.Field("currentPrice")))),
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
        var indexName = IndexManagementService.GetIndexName();

        try
        {
            var response = await _client.GetAsync<AuctionDocument>(auctionId.ToString(), g => g
                .Index(indexName),
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
        int maxSuggestions = 10,
        CancellationToken ct = default)
    {
        var indexName = IndexManagementService.GetIndexName();

        try
        {
            var response = await _client.SearchAsync<AuctionDocument>(s => s
                .Index(indexName)
                .Size(maxSuggestions)
                .Source(new SourceConfig(new SourceFilter
                {
                    Includes = new[] { "id", "title", "categoryName", "thumbnailUrl" }
                }))
                .Query(q => q.Match(m => m
                    .Field("title.autocomplete")
                    .Query(prefix))),
            ct);

            if (!response.IsValidResponse)
            {
                return Array.Empty<AutocompleteSuggestion>();
            }

            return response.Documents.Select(d => new AutocompleteSuggestion
            {
                Id = d.Id,
                Title = d.Title,
                Category = d.CategoryName,
                ThumbnailUrl = d.ThumbnailUrl
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Autocomplete error");
            return Array.Empty<AutocompleteSuggestion>();
        }
    }

    private static Query BuildQuery(AuctionSearchRequest request)
    {
        var mustQueries = new List<Query>();
        var filterQueries = new List<Query>();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            mustQueries.Add(Query.MultiMatch(new MultiMatchQuery
            {
                Query = request.Query,
                Fields = new[] { "title^3", "description", "categoryName^2", "brandName^2", "tags" },
                Type = TextQueryType.BestFields,
                Fuzziness = new Fuzziness("AUTO")
            }));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            filterQueries.Add(Query.Term(new TermQuery("categoryName")
            {
                Value = request.Category
            }));
        }

        if (!string.IsNullOrWhiteSpace(request.Brand))
        {
            filterQueries.Add(Query.Term(new TermQuery("brandName")
            {
                Value = request.Brand
            }));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            filterQueries.Add(Query.Term(new TermQuery("status")
            {
                Value = request.Status
            }));
        }

        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            var rangeQuery = new NumberRangeQuery("currentPrice");
            if (request.MinPrice.HasValue)
                rangeQuery.Gte = (double)request.MinPrice.Value;
            if (request.MaxPrice.HasValue)
                rangeQuery.Lte = (double)request.MaxPrice.Value;
            filterQueries.Add(Query.Range(rangeQuery));
        }

        if (request.EndingBefore.HasValue || request.EndingAfter.HasValue)
        {
            var rangeQuery = new DateRangeQuery("endTime");
            if (request.EndingAfter.HasValue)
                rangeQuery.Gte = request.EndingAfter.Value.ToString("o");
            if (request.EndingBefore.HasValue)
                rangeQuery.Lte = request.EndingBefore.Value.ToString("o");
            filterQueries.Add(Query.Range(rangeQuery));
        }

        if (request.FeaturedOnly)
        {
            filterQueries.Add(Query.Term(new TermQuery("isFeatured")
            {
                Value = true
            }));
        }

        return Query.Bool(new BoolQuery
        {
            Must = mustQueries.Count > 0 ? mustQueries : null,
            Filter = filterQueries
        });
    }

    private static ICollection<SortOptions> BuildSortOptions(string? sortBy, string? sortDirection)
    {
        var direction = sortDirection?.ToLowerInvariant() == "asc"
            ? SortOrder.Asc
            : SortOrder.Desc;

        var sortField = sortBy?.ToLowerInvariant() switch
        {
            "price" => "currentPrice",
            "endtime" => "endTime",
            "bids" => "bidCount",
            "created" => "createdAt",
            _ => "_score"
        };

        if (sortField == "_score")
        {
            return new List<SortOptions>
            {
                SortOptions.Score(new ScoreSort { Order = SortOrder.Desc }),
                SortOptions.Field("createdAt", new FieldSort { Order = SortOrder.Desc }),
                SortOptions.Field("id", new FieldSort { Order = SortOrder.Asc })
            };
        }

        return new List<SortOptions>
        {
            SortOptions.Field(sortField, new FieldSort { Order = direction }),
            SortOptions.Field("id", new FieldSort { Order = SortOrder.Asc })
        };
    }



}
