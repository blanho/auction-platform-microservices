using System.Text;
using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Search.Domain.Models;
using Search.Infrastructure.Configuration;
using Search.Infrastructure.Services;
using Xunit;

namespace Search.Infrastructure.Tests;

public class SearchSortRequestTests
{
    [Theory]
    [InlineData("price", "asc", "currentPrice", "asc")]
    [InlineData("PRICE", "ASC", "currentPrice", "asc")]
    [InlineData("endtime", "desc", "endTime", "desc")]
    [InlineData("ENDTIME", "asc", "endTime", "asc")]
    [InlineData("bids", null, "bidCount", "desc")]
    [InlineData("created", "unknown", "createdAt", "desc")]
    public async Task FieldSort_PreservesDirectionAndIdTieBreaker(
        string sortBy, string? direction, string expectedField, string expectedDirection)
    {
        using var request = await CaptureSearchAsync(sortBy, direction);
        var sorts = request.RootElement.GetProperty("sort");

        Assert.Equal(2, sorts.GetArrayLength());
        Assert.Equal(expectedDirection, sorts[0].GetProperty(expectedField).GetProperty("order").GetString());
        Assert.Equal("asc", sorts[1].GetProperty("id").GetProperty("order").GetString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("relevance")]
    [InlineData("unknown")]
    [InlineData("currentPrice")]
    [InlineData("id")]
    public async Task RelevanceSort_PreservesScoreDateAndIdOrdering(string? sortBy)
    {
        using var request = await CaptureSearchAsync(sortBy, "asc");
        var sorts = request.RootElement.GetProperty("sort");

        Assert.Equal(3, sorts.GetArrayLength());
        Assert.Equal("desc", sorts[0].GetProperty("_score").GetProperty("order").GetString());
        Assert.Equal("desc", sorts[1].GetProperty("createdAt").GetProperty("order").GetString());
        Assert.Equal("asc", sorts[2].GetProperty("id").GetProperty("order").GetString());
    }

    [Fact]
    public async Task SearchQuery_SendsTextAndEveryRequestedFilterToElasticsearch()
    {
        var categoryId = Guid.NewGuid();
        using var request = await CaptureSearchAsync(null, null, new AuctionSearchRequest
        {
            Query = "camera", CategoryId = categoryId, MinPrice = 10m, MaxPrice = 100m, FeaturedOnly = true
        });
        var query = request.RootElement.GetProperty("query").GetProperty("bool");

        var must = query.GetProperty("must");
        var textQuery = must.ValueKind == JsonValueKind.Array ? Assert.Single(must.EnumerateArray()) : must;
        Assert.Equal("camera", textQuery.GetProperty("multi_match").GetProperty("query").GetString());
        var filters = query.GetProperty("filter").EnumerateArray().ToArray();
        Assert.Equal(3, filters.Length);
        Assert.Contains(filters, filter => filter.TryGetProperty("term", out var term) &&
            term.TryGetProperty("categoryId", out var category) && category.GetProperty("value").GetString() == categoryId.ToString());
        Assert.Contains(filters, filter => filter.TryGetProperty("term", out var term) &&
            term.TryGetProperty("isFeatured", out var featured) && featured.GetProperty("value").GetBoolean());
        var price = Assert.Single(filters, filter => filter.TryGetProperty("range", out _))
            .GetProperty("range").GetProperty("currentPrice");
        Assert.Equal(10m, price.GetProperty("gte").GetDecimal());
        Assert.Equal(100m, price.GetProperty("lte").GetDecimal());
    }

    private static async Task<JsonDocument> CaptureSearchAsync(string? sortBy, string? direction, AuctionSearchRequest? search = null)
    {
        ApiCallDetails? call = null;
        var headers = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["x-elastic-product"] = new[] { "Elasticsearch" }
        };
        var response = """{"took":1,"timed_out":false,"_shards":{"total":1,"successful":1,"skipped":0,"failed":0},"hits":{"total":{"value":0,"relation":"eq"},"max_score":null,"hits":[]}}""";
        var invoker = new InMemoryRequestInvoker(Encoding.UTF8.GetBytes(response), 200, headers: headers);
        var client = new ElasticsearchClient(new ElasticsearchClientSettings(invoker)
            .DisableDirectStreaming().MaximumRetries(0).OnRequestCompleted(details => call = details));
        var service = new AuctionSearchService(client, Options.Create(new SearchOptions()),
            Options.Create(new ElasticsearchOptions()), NullLogger<AuctionSearchService>.Instance);

        await service.SearchAsync(search ?? new AuctionSearchRequest { SortBy = sortBy, SortDirection = direction });

        Assert.NotNull(call?.RequestBodyInBytes);
        return JsonDocument.Parse(call.RequestBodyInBytes);
    }
}
