using Elastic.Clients.Elasticsearch;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Options;
using Search.Infrastructure.Configuration;
using SearchService.Contracts.Commands;

namespace Search.Infrastructure.Consumers;

public sealed class ReindexAuctionsConsumer(ElasticsearchClient client, IOptions<ElasticsearchOptions> options)
    : IConsumer<ReindexAuctionsCommand>
{
    public const int MaxAuctionsPerItem = 500;

    public async Task Consume(ConsumeContext<ReindexAuctionsCommand> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;
        ct.ThrowIfCancellationRequested();
        if (message.ParentJobId is null || message.ParentJobItemId is null)
            throw new ArgumentException("A parent job and item are required.");

        string? error = null;
        if (message.RequestedBy == Guid.Empty || message.AuctionIds is null ||
            message.AuctionIds.Count is < 1 or > MaxAuctionsPerItem ||
            message.AuctionIds.Contains(Guid.Empty) || message.AuctionIds.Distinct().Count() != message.AuctionIds.Count)
        {
            error = "Provide a requester and 1–500 distinct, nonempty auction IDs.";
        }
        else
        {
            // Reprocess existing _source under the current mapping. Internal version checks
            // prevent a snapshot from overwriting bids or other concurrent document updates.
            var response = await client.UpdateByQueryAsync(options.Value.GetIndexName(), r => r
                .Query(q => q.Ids(i => i.Values(new Ids(message.AuctionIds.Select(id => id.ToString())))))
                .Conflicts(Conflicts.Abort)
                .MaxDocs(message.AuctionIds.Count)
                .Refresh(true)
                .WaitForCompletion(true), ct);
            ct.ThrowIfCancellationRequested();
            if (!response.IsValidResponse || response.TimedOut == true || response.VersionConflicts > 0 ||
                response.Failures?.Count > 0)
                throw new InvalidOperationException("Search reindexing failed or conflicted with an index update.");

            if (response.Total != message.AuctionIds.Count || response.Updated != message.AuctionIds.Count)
                error = "Some requested auctions are missing from the search projection. Restore them before retrying.";
        }

        await context.Publish(new ReportJobItemResultCommand
        {
            JobId = message.ParentJobId.Value,
            JobItemId = message.ParentJobItemId.Value,
            Attempt = message.Attempt,
            IsSuccess = error is null,
            IsFinalFailure = error is not null,
            ErrorMessage = error
        }, ct);
    }
}
