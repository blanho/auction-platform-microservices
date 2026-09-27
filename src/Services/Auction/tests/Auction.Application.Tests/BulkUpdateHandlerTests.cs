using System.Reflection;
using Auctions.Application.DTOs.Audit;
using Auctions.Application.Features.Auctions.BulkUpdateAuctions;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Auctions.Domain.Enums;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Application.Tests;

public class BulkUpdateHandlerTests
{
    [Fact]
    public async Task BulkUpdate_ReadsOnceAndPublishesAuditForUpdatedAuctionsInRequestOrder()
    {
        var first = AuctionEntity.CreateScheduled(Guid.NewGuid(), "seller",
            Item.Create("First", "Description"), 1m, DateTimeOffset.UtcNow.AddDays(1));
        var second = AuctionEntity.CreateScheduled(Guid.NewGuid(), "seller",
            Item.Create("Second", "Description"), 1m, DateTimeOffset.UtcNow.AddDays(1));
        var missingId = Guid.NewGuid();
        var requestedIds = new List<Guid> { second.Id, missingId, first.Id };
        var batchReads = 0;
        var updatedIds = new List<Guid>();
        var repository = Stub<IAuctionWriteRepository>((method, args) => method.Name switch
        {
            nameof(IAuctionWriteRepository.GetByIdsForUpdateAsync) => ReadBatch(args!),
            nameof(IAuctionWriteRepository.UpdateAsync) => RecordUpdate((AuctionEntity)args![0]!),
            _ => throw new NotSupportedException(method.Name)
        });
        Task<List<AuctionEntity>> ReadBatch(object?[] args)
        {
            batchReads++;
            Assert.Equal(requestedIds, (IEnumerable<Guid>)args[0]!);
            return Task.FromResult(new List<AuctionEntity> { first, second });
        }
        Task RecordUpdate(AuctionEntity auction)
        {
            updatedIds.Add(auction.Id);
            return Task.CompletedTask;
        }
        var unitOfWork = Stub<IUnitOfWork>((method, _) => method.Name switch
        {
            nameof(IUnitOfWork.SaveChangesAsync) => Task.FromResult(1),
            _ => throw new NotSupportedException(method.Name)
        });
        var auditEntries = new List<(Guid EntityId, AuctionAuditData Data)>();
        var auditPublisher = Stub<IAuditPublisher>((method, args) => method.Name switch
        {
            nameof(IAuditPublisher.PublishBatchAsync) => RecordAudit(args!),
            _ => throw new NotSupportedException(method.Name)
        });
        Task RecordAudit(object?[] args)
        {
            auditEntries.AddRange((IEnumerable<(Guid, AuctionAuditData)>)args[0]!);
            Assert.Equal(AuditAction.Updated, args[1]);
            return Task.CompletedTask;
        }
        var handler = new BulkUpdateAuctionsCommandHandler(repository,
            NullLogger<BulkUpdateAuctionsCommandHandler>.Instance, unitOfWork,
            new DateTimeProvider(), auditPublisher);

        var result = await handler.Handle(new BulkUpdateAuctionsCommand(requestedIds, true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.Equal(1, batchReads);
        Assert.Equal(new[] { second.Id, first.Id }, updatedIds);
        Assert.Equal(new[] { second.Id, first.Id }, auditEntries.Select(entry => entry.EntityId));
        Assert.Equal(new[] { "Second", "First" }, auditEntries.Select(entry => entry.Data.Title));
        Assert.All(auditEntries, entry => Assert.Equal(nameof(Status.Live), entry.Data.Status));
    }

    private static T Stub<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, TestProxy>();
        ((TestProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    }
}
