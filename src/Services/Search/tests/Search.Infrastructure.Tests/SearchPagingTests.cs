using System.Globalization;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Application.Localization;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Search.Domain.Models;
using Search.Infrastructure.Configuration;
using Search.Infrastructure.Services;
using Xunit;

namespace Search.Infrastructure.Tests;

public sealed class SearchPagingTests
{
    [Theory]
    [InlineData(0, 0, 1, 10, 0)]
    [InlineData(-4, -8, 1, 10, 0)]
    [InlineData(2, 500, 2, 100, 100)]
    [InlineData(3, 20, 3, 20, 40)]
    [InlineData(100, 100, 100, 100, 9900)]
    public async Task Paging_UsesCommonBoundsAndMatchingResponseMetadata(
        int page, int size, int expectedPage, int expectedSize, int expectedOffset)
    {
        var fixture = new SearchFixture();
        var result = await fixture.Service.SearchAsync(new AuctionSearchRequest { Page = page, PageSize = size });
        using var body = JsonDocument.Parse(Assert.Single(fixture.Calls).RequestBodyInBytes!);
        Assert.Equal(expectedOffset, body.RootElement.GetProperty("from").GetInt32());
        Assert.Equal(expectedSize, body.RootElement.GetProperty("size").GetInt32());
        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedSize, result.PageSize);
        Assert.Equal((int)Math.Ceiling(250d / expectedSize), result.TotalPages);
    }

    [Fact]
    public async Task MissingPageSize_UsesConfiguredSearchDefaultAndConfiguredMaximum()
    {
        var fixture = new SearchFixture(new SearchOptions { DefaultPageSize = 40, MaxPageSize = 25 });
        var result = await fixture.Service.SearchAsync(new AuctionSearchRequest { Page = 2 });
        Assert.Equal(25, result.PageSize);
        using var body = JsonDocument.Parse(Assert.Single(fixture.Calls).RequestBodyInBytes!);
        Assert.Equal(25, body.RootElement.GetProperty("from").GetInt32());
        Assert.Equal(25, body.RootElement.GetProperty("size").GetInt32());
    }

    [Theory]
    [InlineData(101, 100)]
    [InlineData(int.MaxValue, 100)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public async Task DeepPage_RejectsBeforeCallingElasticsearch(int page, int size)
    {
        var fixture = new SearchFixture();
        var error = await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.SearchAsync(
            new AuctionSearchRequest { Page = page, PageSize = size }));
        Assert.Equal("Search pagination cannot exceed 10000 results.", error.Message);
        Assert.Empty(fixture.Calls);
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ja-JP");
            Assert.Equal("検索のページ指定は10000件の上限を超えることはできません。",
                UserMessageLocalizer.Translate(error.Message));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public async Task FailedSearch_ReturnsNormalizedEmptyMetadata()
    {
        var fixture = new SearchFixture(status: 503);
        var result = await fixture.Service.SearchAsync(new AuctionSearchRequest { Page = -1, PageSize = -1 });
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Results);
    }

    private sealed class SearchFixture
    {
        public List<ApiCallDetails> Calls { get; } = [];
        public AuctionSearchService Service { get; }

        public SearchFixture(SearchOptions? options = null, int status = 200)
        {
            var headers = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["x-elastic-product"] = ["Elasticsearch"]
            };
            const string response = """{"took":1,"timed_out":false,"_shards":{"total":1,"successful":1,"skipped":0,"failed":0},"hits":{"total":{"value":250,"relation":"eq"},"max_score":null,"hits":[]}}""";
            var invoker = new InMemoryRequestInvoker(Encoding.UTF8.GetBytes(response), status, headers: headers);
            var client = new ElasticsearchClient(new ElasticsearchClientSettings(invoker)
                .DisableDirectStreaming().MaximumRetries(0).OnRequestCompleted(Calls.Add));
            Service = new AuctionSearchService(client, Options.Create(options ?? new SearchOptions()),
                Options.Create(new ElasticsearchOptions()), NullLogger<AuctionSearchService>.Instance);
        }
    }
}
