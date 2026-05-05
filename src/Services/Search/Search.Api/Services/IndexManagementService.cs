using BuildingBlocks.Web.Exceptions;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.Options;
using Search.Api.Configuration;
using Search.Api.Documents;
using Search.Api.Errors;
using Search.Api.Interfaces;
using Result = BuildingBlocks.Application.Abstractions.Result;

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

    public IndexManagementService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        ILogger<IndexManagementService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result> EnsureIndexExistsAsync(CancellationToken ct = default)
    {
        var indexName = GetIndexName();

        try
        {
            var existsResponse = await _client.Indices.ExistsAsync(indexName, ct);

            if (existsResponse.Exists)
            {
                _logger.LogDebug("Index {IndexName} already exists", indexName);
                return Result.Success();
            }

            return await CreateIndexAsync(indexName, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring index {IndexName} exists", indexName);
            return Result.Failure(IndexErrors.IndexCreationFailed(indexName, ex.Message));
        }
    }

    private async Task<Result> CreateIndexAsync(string indexName, CancellationToken ct)
    {
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
                            .TokenChars(new[] 
                            { 
                                TokenChar.Letter, 
                                TokenChar.Digit 
                            })))))
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
            return Result.Failure(IndexErrors.IndexCreationFailed(indexName, createResponse.DebugInformation));
        }

        _logger.LogInformation("Created index {IndexName}", indexName);
        return Result.Success();
    }

    public async Task<Result> RecreateIndexAsync(CancellationToken ct = default)
    {
        var indexName = GetIndexName();

        _logger.LogWarning("Recreating index {IndexName} - ALL DATA WILL BE LOST", indexName);

        var existsResponse = await _client.Indices.ExistsAsync(indexName, ct);
        if (existsResponse.Exists)
        {
            var deleteResponse = await _client.Indices.DeleteAsync(indexName, ct);
            if (!deleteResponse.IsValidResponse)
            {
                return Result.Failure(IndexErrors.DeleteFailed(Guid.Empty, deleteResponse.DebugInformation));
            }
        }

        return await EnsureIndexExistsAsync(ct);
    }

    public async Task<Result<IndexStats>> GetIndexStatsAsync(CancellationToken ct = default)
    {
        var indexName = GetIndexName();

        try
        {
            var existsResponse = await _client.Indices.ExistsAsync(indexName, ct);
            if (!existsResponse.Exists)
            {
                return Result.Success(new IndexStats(indexName, false, 0, "none", 0));
            }

            var statsResponse = await _client.Indices.StatsAsync(
                new IndicesStatsRequest(Elastic.Clients.Elasticsearch.Indices.Parse(indexName)), ct);

            if (!statsResponse.IsValidResponse || statsResponse.Indices == null)
            {
                return Result.Success(new IndexStats(indexName, true, 0, "unknown", 0));
            }

            var stats = BuildIndexStats(indexName, statsResponse);
            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats for index {IndexName}", indexName);
            return Result.Failure<IndexStats>(IndexErrors.ConnectionFailed(ex.Message));
        }
    }

    private static IndexStats BuildIndexStats(string indexName, IndicesStatsResponse response)
    {
        if (response.Indices?.TryGetValue(indexName, out var indexStats) == true)
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

    public async Task<Result> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _client.PingAsync(ct);
            return response.IsValidResponse 
                ? Result.Success() 
                : Result.Failure(IndexErrors.ConnectionFailed("Elasticsearch ping failed"));
        }
        catch (Exception ex)
        {
            return Result.Failure(IndexErrors.ConnectionFailed(ex.Message));
        }
    }

    private string GetIndexName() => _options.GetIndexName();
}
