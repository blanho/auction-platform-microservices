using System.Text;
using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Search.Domain.Documents;
using Search.Infrastructure.Configuration;
using Search.Infrastructure.Services;
using Xunit;

namespace Search.Infrastructure.Tests;

public class BidIndexRequestTests
{
    [Fact]
    public async Task BidStateUpdate_SendsAtomicScriptWithOriginalTicksAndReserveFallback()
    {
        ApiCallDetails? call = null;
        var service = Service(Client(200, """{"_index":"auctions","_id":"test","_version":1,"result":"updated","_shards":{"total":1,"successful":1,"failed":0}}""", details => call = details));
        var occurredAt = new DateTimeOffset(2026, 9, 24, 1, 0, 0, TimeSpan.Zero).AddTicks(7);
        var result = await service.ApplyBidStateAsync(Guid.NewGuid(), null, occurredAt, true);
        Assert.True(result.IsSuccess, result.Error?.ToString());
        using var request = JsonDocument.Parse(call!.RequestBodyInBytes!);
        var script = request.RootElement.GetProperty("script");
        Assert.Equal(BidStateScript.Source, script.GetProperty("source").GetString());
        var parameters = script.GetProperty("params");
        Assert.Equal(occurredAt.UtcTicks, parameters.GetProperty("ticks").GetInt64());
        Assert.False(parameters.GetProperty("hasPrice").GetBoolean());
        Assert.True(parameters.GetProperty("isRetraction").GetBoolean());
    }

    [Fact]
    public async Task ReplayedAuctionCreation_DoesNotReplaceExistingDocument()
    {
        ApiCallDetails? call = null;
        var service = Service(Client(409, """{"error":{"type":"version_conflict_engine_exception","reason":"already exists"},"status":409}""", details => call = details));
        var result = await service.IndexAsync(new AuctionDocument { Id = Guid.NewGuid() });
        Assert.True(result.IsSuccess, result.Error?.ToString());
        Assert.Contains("op_type=create", call!.Uri!.Query);
    }

    [Fact]
    public async Task ExistingIndex_ReceivesBidOrderingMappings()
    {
        var calls = new List<ApiCallDetails>();
        var client = Client(200, """{"acknowledged":true}""", calls.Add);
        var service = new IndexManagementService(client, Options.Create(new ElasticsearchOptions()), NullLogger<IndexManagementService>.Instance);
        var result = await service.EnsureIndexExistsAsync();
        Assert.True(result.IsSuccess, result.Error?.ToString());
        var mapping = Assert.Single(calls, call => call.Uri!.AbsolutePath.EndsWith("_mapping"));
        using var request = JsonDocument.Parse(mapping.RequestBodyInBytes!);
        var properties = request.RootElement.GetProperty("properties");
        Assert.Equal("long", properties.GetProperty("lastBidEventTicks").GetProperty("type").GetString());
        Assert.Equal("boolean", properties.GetProperty("lastBidEventIsRetraction").GetProperty("type").GetString());
    }

    private static AuctionIndexService Service(ElasticsearchClient client) =>
        new(client, Options.Create(new ElasticsearchOptions()), NullLogger<AuctionIndexService>.Instance);

    private static ElasticsearchClient Client(int status, string response, Action<ApiCallDetails> onRequest)
    {
        var headers = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase) { ["x-elastic-product"] = new[] { "Elasticsearch" } };
        var invoker = new InMemoryRequestInvoker(Encoding.UTF8.GetBytes(response), status, headers: headers);
        return new ElasticsearchClient(new ElasticsearchClientSettings(invoker)
            .DisableDirectStreaming().MaximumRetries(0).OnRequestCompleted(onRequest));
    }
}
