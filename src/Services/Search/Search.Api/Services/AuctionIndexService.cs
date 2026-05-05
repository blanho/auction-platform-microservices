using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using Search.Api.Configuration;
using Search.Api.Documents;
using Search.Api.Errors;
using Search.Api.Interfaces;
using Result = BuildingBlocks.Application.Abstractions.Result;

namespace Search.Api.Services;

public record BulkIndexResult(int Indexed, int Failed, List<string> Errors);

public class AuctionIndexService : IAuctionIndexService
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;
    private readonly ILogger<AuctionIndexService> _logger;

    public AuctionIndexService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> esOptions,
        ILogger<AuctionIndexService> logger)
    {
        _client = client;
        _indexName = esOptions.Value.GetIndexName();
        _logger = logger;
    }

    public async Task<Result> IndexAsync(AuctionDocument document, CancellationToken ct = default)
    {
        var indexName = _indexName;
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
                return Result.Failure(IndexErrors.IndexingFailed(document.Id, response.DebugInformation));
            }

            _logger.LogDebug("Indexed auction {AuctionId}", document.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing auction {AuctionId}", document.Id);
            return Result.Failure(IndexErrors.IndexingFailed(document.Id, ex.Message));
        }
    }

    public async Task<Result<BulkIndexResult>> BulkIndexAsync(
        IEnumerable<AuctionDocument> documents, 
        CancellationToken ct = default)
    {
        var indexName = _indexName;
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
                return Result.Failure<BulkIndexResult>(
                    IndexErrors.BulkIndexFailed(documentList.Count, response.DebugInformation));
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

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk index");
            return Result.Failure<BulkIndexResult>(
                IndexErrors.BulkIndexFailed(documentList.Count, ex.Message));
        }
    }

    public async Task<Result> PartialUpdateAsync(
        Guid auctionId, 
        object partialDocument, 
        CancellationToken ct = default)
    {
        var indexName = _indexName;

        try
        {
            var response = await _client.UpdateAsync<AuctionDocument, object>(
                indexName, 
                auctionId.ToString(), 
                u => u
                    .Doc(partialDocument)
                    .DocAsUpsert(false)
                    .RetryOnConflict(3),
                ct);

            if (!response.IsValidResponse)
            {
                _logger.LogError("Failed to partially update auction {AuctionId}: {Error}",
                    auctionId, response.DebugInformation);
                return Result.Failure(IndexErrors.UpdateFailed(auctionId, response.DebugInformation));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error partially updating auction {AuctionId}", auctionId);
            return Result.Failure(IndexErrors.UpdateFailed(auctionId, ex.Message));
        }
    }

    public async Task<Result> DeleteAsync(Guid auctionId, CancellationToken ct = default)
    {
        var indexName = _indexName;

        try
        {
            var response = await _client.DeleteAsync<AuctionDocument>(indexName, auctionId.ToString(), ct);

            if (!response.IsValidResponse && response.Result != Elastic.Clients.Elasticsearch.Result.NotFound)
            {
                _logger.LogError("Failed to delete auction {AuctionId}: {Error}",
                    auctionId, response.DebugInformation);
                return Result.Failure(IndexErrors.DeleteFailed(auctionId, response.DebugInformation));
            }

            _logger.LogDebug("Deleted auction {AuctionId} from index", auctionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting auction {AuctionId}", auctionId);
            return Result.Failure(IndexErrors.DeleteFailed(auctionId, ex.Message));
        }
    }

    public async Task<Result> UpdateBidInfoAsync(
        Guid auctionId, 
        decimal currentPrice, 
        int bidCount, 
        CancellationToken ct = default)
    {
        var indexName = _indexName;

        try
        {
            var response = await _client.UpdateAsync<AuctionDocument, object>(
                indexName, 
                auctionId.ToString(), 
                u => u
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
                return Result.Failure(IndexErrors.UpdateFailed(auctionId, response.DebugInformation));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating bid info for auction {AuctionId}", auctionId);
            return Result.Failure(IndexErrors.UpdateFailed(auctionId, ex.Message));
        }
    }
}
