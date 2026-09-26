using System.Reflection;
using Auctions.Application.Features.Auctions.ImportAuctions;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Auction.Application.Tests;

public class ImportBatchingTests
{
    [Theory]
    [InlineData(0, 0, new int[0])]
    [InlineData(1, 0, new[] { 1 })]
    [InlineData(500, 0, new[] { 500 })]
    [InlineData(501, 0, new[] { 500, 1 })]
    [InlineData(1001, 500, new[] { 500, 1 })]
    public async Task Import_PreservesBatchSizesRowOrderAndCheckpointAfterSave(
        int rowCount, int alreadyProcessed, int[] expectedBatchSizes)
    {
        var calls = new List<string>();
        var batchSizes = new List<int>();
        var titles = new List<string>();
        var checkpoints = new List<ImportCheckpoint>();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var now = DateTimeOffset.UtcNow;
        var bulkRepository = Stub<IAuctionBulkRepository>((method, args) =>
        {
            Assert.Equal(token, args[^1]);
            Assert.Equal(nameof(IAuctionBulkRepository.BulkInsertAsync), method.Name);
            var batch = Assert.IsAssignableFrom<IReadOnlyList<Auctions.Domain.Entities.Auction>>(args[0]);
            batchSizes.Add(batch.Count);
            titles.AddRange(batch.Select(auction => auction.Item.Title));
            calls.Add("insert");
            return Task.CompletedTask;
        });
        var checkpointRepository = Stub<IImportCheckpointRepository>((method, args) =>
        {
            Assert.Equal(token, args[^1]);
            switch (method.Name)
            {
                case nameof(IImportCheckpointRepository.GetCheckpointAsync):
                    return Task.FromResult(alreadyProcessed == 0 ? null :
                        new ImportCheckpoint("import", alreadyProcessed, alreadyProcessed, 0, now));
                case nameof(IImportCheckpointRepository.SaveCheckpointAsync):
                    Assert.Equal("save", calls[^1]);
                    checkpoints.Add(Assert.IsType<ImportCheckpoint>(args[0]));
                    calls.Add("checkpoint");
                    return Task.CompletedTask;
                case nameof(IImportCheckpointRepository.DeleteCheckpointAsync):
                    calls.Add("cleanup");
                    return Task.CompletedTask;
                default:
                    throw new InvalidOperationException(method.Name);
            }
        });
        var unit = Stub<IUnitOfWork>((method, args) =>
        {
            Assert.Equal(nameof(IUnitOfWork.SaveChangesAsync), method.Name);
            Assert.Equal(token, args[^1]);
            Assert.Equal("insert", calls[^1]);
            calls.Add("save");
            return Task.FromResult(1);
        });
        var sanitizer = Stub<ISanitizationService>((_, args) => args[0]);
        var clock = Stub<IDateTimeProvider>((_, _) => now);
        var handler = new ImportAuctionsCommandHandler(bulkRepository, checkpointRepository,
            sanitizer, unit, NullLogger<ImportAuctionsCommandHandler>.Instance, clock);
        var rows = Enumerable.Range(1, rowCount).Select(number => new ImportAuctionRow(
            $"Item {number}", "Description", null, null, 100m, null, now.AddDays(7))).ToList();

        var result = await handler.Handle(new ImportAuctionsCommand(
            Guid.NewGuid(), "seller", "import", "USD", rows), token);

        Assert.True(result.IsSuccess);
        Assert.Equal(rowCount, result.Value!.SucceededCount);
        Assert.Equal(0, result.Value.FailedCount);
        Assert.Equal(alreadyProcessed, result.Value.SkippedDuplicateCount);
        Assert.Equal(expectedBatchSizes, batchSizes);
        Assert.Equal(Enumerable.Range(alreadyProcessed + 1, rowCount - alreadyProcessed)
            .Select(number => $"Item {number}"), titles);
        var processed = alreadyProcessed;
        foreach (var (checkpoint, batchSize) in checkpoints.Zip(expectedBatchSizes))
        {
            processed += batchSize;
            Assert.Equal(processed, checkpoint.LastProcessedRowIndex);
            Assert.Equal(processed, checkpoint.SucceededCount);
        }
        Assert.Equal(expectedBatchSizes.Length, checkpoints.Count);
        Assert.Equal("cleanup", calls[^1]);
    }

    private static T Stub<T>(Func<MethodInfo, object?[], object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, TestProxy>();
        ((TestProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[], object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args!);
    }
}
