using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Search.Infrastructure.Configuration;
using Search.Infrastructure.Services;
using Xunit;

namespace Search.Infrastructure.Tests;

public class SearchCancellationTests
{
    [Fact]
    public async Task SearchAndIndexCalls_PropagateCancellation()
    {
        var client = new ElasticsearchClient(new ElasticsearchClientSettings(new Uri("http://127.0.0.1:1"))
            .MaximumRetries(0));
        var options = Options.Create(new ElasticsearchOptions());
        var search = new AuctionSearchService(client, Options.Create(new SearchOptions()), options,
            NullLogger<AuctionSearchService>.Instance);
        var index = new AuctionIndexService(client, options, NullLogger<AuctionIndexService>.Instance);
        var management = new IndexManagementService(client, options, NullLogger<IndexManagementService>.Instance);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            search.GetByIdAsync(Guid.NewGuid(), cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            index.DeleteAsync(Guid.NewGuid(), cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            management.IsHealthyAsync(cancellation.Token));
    }

    [Fact]
    public async Task SearchCall_PropagatesCancellationWhenTransportReturnsAfterCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var headers = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["x-elastic-product"] = ["Elasticsearch"]
        };
        var invoker = new InMemoryRequestInvoker([], 200, headers: headers);
        var client = new ElasticsearchClient(new ElasticsearchClientSettings(invoker)
            .MaximumRetries(0)
            .OnRequestCompleted(_ => cancellation.Cancel()));
        var service = new AuctionSearchService(client, Options.Create(new SearchOptions()),
            Options.Create(new ElasticsearchOptions()), NullLogger<AuctionSearchService>.Instance);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GetByIdAsync(Guid.NewGuid(), cancellation.Token));
    }
}
