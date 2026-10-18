using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using JobService.Contracts.Commands;
using Microsoft.Extensions.Options;
using Search.Infrastructure.Configuration;
using Search.Infrastructure.Consumers;
using SearchService.Contracts.Commands;
using Xunit;

namespace Search.Infrastructure.Tests;

public class SearchReindexIntegrationTests
{
    [ElasticsearchFact]
    public async Task ReindexPicksUpAddedMappingWhilePreservingSourceBidOrderingAndOtherDocuments()
    {
        var url = Environment.GetEnvironmentVariable("BACKEND_TEST_ELASTICSEARCH")!;
        var index = "job-reindex-test-" + Guid.NewGuid().ToString("N");
        using var http = new HttpClient { BaseAddress = new Uri(url) };
        var id = Guid.NewGuid();
        var untouched = Guid.NewGuid();
        try
        {
            await Put(index, new { mappings = new { properties = new { title = new { type = "text" } } } });
            var source = new { title = "Auction", currentPrice = 125m, lastBidEventTicks = 639000000000000007L, lastBidEventIsRetraction = true };
            await Put($"{index}/_doc/{id}?refresh=true", source);
            await Put($"{index}/_doc/{untouched}?refresh=true", source);
            await Put($"{index}/_mapping", new { properties = new { title = new { type = "text", fields = new { exact = new { type = "keyword" } } } } });
            var before = await http.GetFromJsonAsync<JsonObject>($"{index}/_doc/{id}");
            var client = new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(url)));
            var consumer = new ReindexAuctionsConsumer(client, Options.Create(new ElasticsearchOptions { IndexPrefix = index }));
            var command = new ReindexAuctionsCommand
            {
                AuctionIds = [id], RequestedBy = Guid.NewGuid(), ParentJobId = Guid.NewGuid(), ParentJobItemId = Guid.NewGuid()
            };
            for (var attempt = 0; attempt < 2; attempt++)
            {
                var results = new List<ReportJobItemResultCommand>();
                await consumer.Consume(SearchReindexTests.Context(command, results));
                Assert.True(Assert.Single(results).IsSuccess);
            }
            var after = await http.GetFromJsonAsync<JsonObject>($"{index}/_doc/{id}");
            Assert.True(JsonNode.DeepEquals(before!["_source"], after!["_source"]));
            Assert.Equal(3, after["_version"]!.GetValue<int>());
            var other = await http.GetFromJsonAsync<JsonObject>($"{index}/_doc/{untouched}");
            Assert.Equal(1, other!["_version"]!.GetValue<int>());
            var response = await http.PostAsJsonAsync($"{index}/_search", new { query = new { term = new Dictionary<string, string> { ["title.exact"] = "Auction" } } });
            response.EnsureSuccessStatusCode();
            var search = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(1, search!["hits"]!["total"]!["value"]!.GetValue<int>());
        }
        finally
        {
            (await http.DeleteAsync(index)).EnsureSuccessStatusCode();
        }
        async Task Put(string path, object value) => (await http.PutAsJsonAsync(path, value)).EnsureSuccessStatusCode();
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ElasticsearchFactAttribute : FactAttribute
{
    public ElasticsearchFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BACKEND_TEST_ELASTICSEARCH")))
            Skip = "Set BACKEND_TEST_ELASTICSEARCH to a disposable Elasticsearch server.";
    }
}
