using BuildingBlocks.Web.Exceptions;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.Options;
using Search.Api.Configuration;
using Search.Api.Documents;
using Search.Api.Interfaces;

namespace Search.Api.Services;

public record IndexStats(
    string IndexName,
    bool Exists,
    long DocumentCount,
    string Health,
    long SizeInBytes);

public class IndexManagementService : IIndexManagementService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly ILogger<IndexManagementService> _logger;

    private const string IndexPrefix = "auctions";

    public IndexManagementService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        ILogger<IndexManagementService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> EnsureIndexExistsAsync(CancellationToken ct = default)
    {
        var indexName = GetIndexName();

        try
        {
            var existsResponse = await _client.Indices.ExistsAsync(indexName, ct);

            if (existsResponse.Exists)
            {
                _logger.LogDebug("Index {IndexName} already exists", indexName);
                return true;
            }

            var createResponse = await _client.Indices.CreateAsync(indexName, c => c
                .Settings(s => s
                    .NumberOfShards(_options.NumberOfShards)
                    .NumberOfReplicas(_options.NumberOfReplicas)
                    .RefreshInterval(new Duration("1s"))
                    .Analysis(a => a
                        .Analyzers(an => an
                            .Custom("auction_analyzer", ca => ca
                                .Tokenizer("standard")
                                .Filter(new[] { "lowercase", "asciifolding" }))
                            .Custom("autocomplete_analyzer", ca => ca
                                .Tokenizer("autocomplete_tokenizer")
                                .Filter(new[] { "lowercase", "asciifolding" }))
                            .Custom("autocomplete_search_analyzer", ca => ca
                                .Tokenizer("standard")
                                .Filter(new[] { "lowercase", "asciifolding" })))
                        .Tokenizers(t => t
                            .EdgeNGram("autocomplete_tokenizer", e => e
                                .MinGram(2)
                                .MaxGram(20)
                                .TokenChars(new[] { Elastic.Clients.Elasticsearch.Analysis.TokenChar.Letter, Elastic.Clients.Elasticsearch.Analysis.TokenChar.Digit })))))
                .Mappings(m => m
                    .Dynamic(DynamicMapping.Strict)
                    .Properties<AuctionDocument>(p => p
                        .Keyword(k => k.Id)
                        .Text(k => k.Title, t => t
                            .Analyzer("auction_analyzer")
                            .Fields(f => f
                                .Keyword("keyword")
                                .Text("autocomplete", ac => ac
                                    .Analyzer("autocomplete_analyzer")
                                    .SearchAnalyzer("autocomplete_search_analyzer"))))
                        .Text(k => k.Description, t => t.Analyzer("auction_analyzer"))
                        .Keyword(k => k.CategoryId)
                        .Keyword(k => k.CategoryName)
                        .Keyword(k => k.CategoryPath)
                        .Keyword(k => k.BrandId)
                        .Keyword(k => k.BrandName)
                        .Keyword(k => k.SellerId)
                        .Keyword(k => k.SellerUsername)
                        .DoubleNumber(k => k.StartPrice)
                        .DoubleNumber(k => k.CurrentPrice)
                        .DoubleNumber(k => k.ReservePrice)
                        .DoubleNumber(k => k.BuyNowPrice)
                        .Keyword(k => k.Currency)
                        .Keyword(k => k.Status)
                        .Keyword(k => k.Condition)
                        .Date(k => k.StartTime)
                        .Date(k => k.EndTime)
                        .Date(k => k.CreatedAt)
                        .Date(k => k.UpdatedAt)
                        .IntegerNumber(k => k.BidCount)
                        .Keyword(k => k.WinnerId)
                        .Keyword(k => k.WinnerUsername)
                        .DoubleNumber(k => k.FinalPrice)
                        .Keyword(k => k.ImageUrls)
                        .Keyword(k => k.ThumbnailUrl)
                        .Object(k => k.Attributes, o => o.Enabled(false))
                        .Keyword(k => k.Tags)
                        .Boolean(k => k.IsFeatured)
                        .GeoPoint(k => k.Location)
                        .Date(k => k.LastSyncedAt))),
            ct);

            if (!createResponse.IsValidResponse)
            {
                _logger.LogError("Failed to create index {IndexName}: {Error}",
                    indexName, createResponse.DebugInformation);
                return false;
            }

            _logger.LogInformation("Created index {IndexName}", indexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring index {IndexName} exists", indexName);
            throw;
        }
    }

    public async Task RecreateIndexAsync(CancellationToken ct = default)
    {
        var indexName = GetIndexName();

        _logger.LogWarning("Recreating index {IndexName} - ALL DATA WILL BE LOST", indexName);

        var existsResponse = await _client.Indices.ExistsAsync(indexName, ct);
        if (existsResponse.Exists)
        {
            var deleteResponse = await _client.Indices.DeleteAsync(indexName, ct);
            if (!deleteResponse.IsValidResponse)
            {
                throw new ConflictException($"Failed to delete index: {deleteResponse.DebugInformation}");
            }
        }

        await EnsureIndexExistsAsync(ct);
    }

    public async Task<IndexStats> GetIndexStatsAsync(CancellationToken ct = default)
    {
        var indexName = GetIndexName();

        var existsResponse = await _client.Indices.ExistsAsync(indexName, ct);
        if (!existsResponse.Exists)
        {
            return new IndexStats(indexName, false, 0, "none", 0);
        }

        var statsResponse = await _client.Indices.StatsAsync(new IndicesStatsRequest(Elastic.Clients.Elasticsearch.Indices.Parse(indexName)), ct);

        if (!statsResponse.IsValidResponse || statsResponse.Indices == null)
        {
            return new IndexStats(indexName, true, 0, "unknown", 0);
        }

        if (statsResponse.Indices.TryGetValue(indexName, out var indexStats))
        {
            return new IndexStats(
                indexName,
                true,
                indexStats.Primaries?.Docs?.Count ?? 0,
                "green",
                indexStats.Primaries?.Store?.SizeInBytes ?? 0);
        }

        return new IndexStats(indexName, true, 0, "unknown", 0);
    }

    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _client.PingAsync(ct);
            return response.IsValidResponse;
        }
        catch
        {
            return false;
        }
    }

    public static string GetIndexName() => IndexPrefix;
}
