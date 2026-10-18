using System.Reflection;
using System.Text;
using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Options;
using Search.Infrastructure.Configuration;
using Search.Infrastructure.Consumers;
using SearchService.Contracts.Commands;
using Xunit;

namespace Search.Infrastructure.Tests;

public class SearchReindexTests
{
    [Fact]
    public async Task ReindexesOnlyRequestedIdsWithoutReplacingSourceOrDeletingIndex()
    {
        var command = Command();
        var calls = new List<ApiCallDetails>();
        var results = new List<ReportJobItemResultCommand>();
        await Consumer(200, Success, calls).Consume(Context(command, results));
        var call = Assert.Single(calls);
        Assert.EndsWith("/_update_by_query", call.Uri!.AbsolutePath);
        Assert.Contains("wait_for_completion=true", call.Uri.Query);
        Assert.Contains("refresh=true", call.Uri.Query);
        using var body = JsonDocument.Parse(call.RequestBodyInBytes!);
        Assert.Equal(command.AuctionIds[0].ToString(), body.RootElement.GetProperty("query").GetProperty("ids").GetProperty("values")[0].GetString());
        Assert.Equal("abort", body.RootElement.GetProperty("conflicts").GetString());
        Assert.False(body.RootElement.TryGetProperty("script", out _));
        var result = Assert.Single(results);
        Assert.True(result.IsSuccess);
        Assert.Equal(command.ParentJobId, result.JobId);
        Assert.Equal(command.ParentJobItemId, result.JobItemId);
        Assert.Equal(3, result.Attempt);
    }

    [Theory]
    [InlineData(409, "{\"error\":{\"type\":\"version_conflict_engine_exception\",\"reason\":\"conflict\"},\"status\":409}")]
    [InlineData(503, "{\"error\":{\"type\":\"unavailable_shards_exception\",\"reason\":\"unavailable\"},\"status\":503}")]
    [InlineData(200, "{\"timed_out\":true,\"total\":1,\"updated\":0,\"failures\":[]}")]
    [InlineData(200, "{\"timed_out\":false,\"total\":1,\"updated\":0,\"version_conflicts\":1,\"failures\":[]}")]
    [InlineData(200, "{\"total\":1,\"updated\":0,\"failures\":[{\"index\":\"auctions\",\"id\":\"a\",\"status\":400,\"cause\":{\"type\":\"mapper_parsing_exception\",\"reason\":\"bad mapping\"}}]}")]
    public async Task FailuresPropagateForRetryWithoutSuccess(int status, string response)
    {
        var results = new List<ReportJobItemResultCommand>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => Consumer(status, response, []).Consume(Context(Command(), results)));
        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingDocumentsFailInsteadOfClaimingAFullRebuild()
    {
        var results = new List<ReportJobItemResultCommand>();
        await Consumer(200, "{\"total\":0,\"updated\":0,\"failures\":[]}", []).Consume(Context(Command(), results));
        Assert.False(Assert.Single(results).IsSuccess);
        Assert.True(results[0].IsFinalFailure);
    }

    [Fact]
    public async Task InvalidBatchesNeverReachElasticsearch()
    {
        var command = Command();
        foreach (var ids in new List<Guid>?[] { null, [], [Guid.Empty], [command.AuctionIds[0], command.AuctionIds[0]], Enumerable.Range(0, 501).Select(_ => Guid.NewGuid()).ToList() })
        {
            var calls = new List<ApiCallDetails>();
            var results = new List<ReportJobItemResultCommand>();
            await Consumer(200, Success, calls).Consume(Context(command with { AuctionIds = ids! }, results));
            Assert.Empty(calls);
            Assert.False(Assert.Single(results).IsSuccess);
        }
    }

    [Fact]
    public async Task CancellationDoesNotPublishCompletion()
    {
        var calls = new List<ApiCallDetails>();
        var results = new List<ReportJobItemResultCommand>();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Consumer(200, Success, calls)
            .Consume(Context(Command(), results, new CancellationToken(true))));
        Assert.Empty(calls);
        Assert.Empty(results);
    }

    private const string Success = """{"timed_out":false,"total":1,"updated":1,"version_conflicts":0,"failures":[]}""";
    private static ReindexAuctionsCommand Command() => new()
    {
        AuctionIds = [Guid.NewGuid()], RequestedBy = Guid.NewGuid(),
        ParentJobId = Guid.NewGuid(), ParentJobItemId = Guid.NewGuid(), Attempt = 3
    };
    private static ReindexAuctionsConsumer Consumer(int status, string response, List<ApiCallDetails> calls)
    {
        var headers = new Dictionary<string, IEnumerable<string>> { ["x-elastic-product"] = new[] { "Elasticsearch" } };
        var invoker = new InMemoryRequestInvoker(Encoding.UTF8.GetBytes(response), status, headers: headers);
        var client = new ElasticsearchClient(new ElasticsearchClientSettings(invoker)
            .DisableDirectStreaming().MaximumRetries(0).OnRequestCompleted(calls.Add));
        return new(client, Options.Create(new ElasticsearchOptions()));
    }
    internal static ConsumeContext<ReindexAuctionsCommand> Context(ReindexAuctionsCommand command,
        List<ReportJobItemResultCommand> results, CancellationToken ct = default)
    {
        var proxy = DispatchProxy.Create<ConsumeContext<ReindexAuctionsCommand>, ConsumerFailureTests.TestProxy>();
        ((ConsumerFailureTests.TestProxy)(object)proxy).Handler = (method, args) => method.Name switch
        {
            "get_Message" => command,
            "get_CancellationToken" => ct,
            "Publish" => Capture(args![0]!),
            _ => throw new NotSupportedException(method.Name)
        };
        return proxy;
        Task Capture(object message) { results.Add(Assert.IsType<ReportJobItemResultCommand>(message)); return Task.CompletedTask; }
    }
}
