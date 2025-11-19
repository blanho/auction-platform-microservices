using Elastic.Clients.Elasticsearch;
using Search.Api.Documents;
using Search.Api.Interfaces;

namespace Search.Api.Services;

public record BulkIndexResult(int Indexed, int Failed, List<string> Errors);

public class AuctionIndexService : IAuctionIndexService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<AuctionIndexService> _logger;

    public AuctionIndexService(
        ElasticsearchClient client,
        ILogger<AuctionIndexService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> IndexAsync(AuctionDocument document, CancellationToken ct = default)
    {
        var indexName = IndexManagementService.GetIndexName();
        document.LastSyncedAt = DateTimeOffset.UtcNow;

        try
        {
            var response = await _client.IndexAsync(document, i => i
                .Index(indexName)
                .Id(document.Id.ToString())
                .Refresh(Elastic.Clients.Elasticsearch.Refresh.False),
            ct);

            if (!response.IsValidResponse)
            {
                _logger.LogError("Failed to index auction {AuctionId}: {Error}",
                    document.Id, response.DebugInformation);
                return false;
            }

            _logger.LogDebug("Indexed auction {AuctionId}", document.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing auction {AuctionId}", document.Id);
            return false;
        }
    }

    public async Task<BulkIndexResult> BulkIndexAsync(IEnumerable<AuctionDocument> documents, CancellationToken ct = default)
    {
        var indexName = IndexManagementService.GetIndexName();
        var now = DateTimeOffset.UtcNow;

        var documentList = documents.ToList();
        foreach (var doc in documentList)
        {
            doc.LastSyncedAt = now;
        }

        try
        {
            var response = await _client.BulkAsync(b => b
                .Index(indexName)
                .IndexMany(documentList, (d, doc) => d.Id(doc.Id.ToString())),
            ct);

            if (!response.IsValidResponse)
            {
                _logger.LogError("Bulk index failed: {Error}", response.DebugInformation);
                return new BulkIndexResult(0, documentList.Count, new List<string> { response.DebugInformation });
            }

            var errors = response.ItemsWithErrors
                .Select(i => $"{i.Id}: {i.Error?.Reason}")
                .ToList();

            var result = new BulkIndexResult(
                documentList.Count - errors.Count,
                errors.Count,
                errors);

            _logger.LogInformation(
                "Bulk indexed {Indexed} documents, {Failed} failed",
                result.Indexed, result.Failed);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk index");
            return new BulkIndexResult(0, documentList.Count, new List<string> { ex.Message });
        }
    }

    public async Task<bool> PartialUpdateAsync(Guid auctionId, object partialDocument, CancellationToken ct = default)
    {
        var indexName = IndexManagementService.GetIndexName();

        try
        {
            var response = await _client.UpdateAsync<AuctionDocument, object>(indexName, auctionId.ToString(), u => u
                .Doc(partialDocument)
                .DocAsUpsert(false)
                .RetryOnConflict(3),
            ct);

            if (!response.IsValidResponse)
            {
                _logger.LogError("Failed to partially update auction {AuctionId}: {Error}",
                    auctionId, response.DebugInformation);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error partially updating auction {AuctionId}", auctionId);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid auctionId, CancellationToken ct = default)
    {
        var indexName = IndexManagementService.GetIndexName();

        try
        {
            var response = await _client.DeleteAsync<AuctionDocument>(indexName, auctionId.ToString(), ct);

            if (!response.IsValidResponse && response.Result != Elastic.Clients.Elasticsearch.Result.NotFound)
            {
                _logger.LogError("Failed to delete auction {AuctionId}: {Error}",
                    auctionId, response.DebugInformation);
                return false;
            }

            _logger.LogDebug("Deleted auction {AuctionId} from index", auctionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting auction {AuctionId}", auctionId);
            return false;
        }
    }

    public async Task<bool> UpdateBidInfoAsync(Guid auctionId, decimal currentPrice, int bidCount, CancellationToken ct = default)
    {
        var indexName = IndexManagementService.GetIndexName();

        try
        {

            var response = await _client.UpdateAsync<AuctionDocument, object>(indexName, auctionId.ToString(), u => u
                .Script(s => s
                    .Source("ctx._source.currentPrice = params.price; ctx._source.bidCount = params.count; ctx._source.lastSyncedAt = params.syncedAt")
                    .Lang("painless")
                    .Params(p => p
                        .Add("price", currentPrice)
                        .Add("count", bidCount)
                        .Add("syncedAt", DateTimeOffset.UtcNow.ToString("o"))))
                .RetryOnConflict(5),
            ct);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Failed to update bid info for auction {AuctionId}: {Error}",
                    auctionId, response.DebugInformation);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating bid info for auction {AuctionId}", auctionId);
            return false;
        }
    }
}
