using System.Reflection;
using AuctionService.Contracts.Commands;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Auctions.Domain.Enums;
using Auctions.Domain.Events;
using Auctions.Infrastructure.Messaging.Consumers;
using Auctions.Infrastructure.Persistence;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Providers;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Infrastructure.Tests;

public class BulkUpdateCancellationTests
{
    [Fact]
    public async Task CanceledWrite_DoesNotCountAuctionAsFailure()
    {
        var auction = AuctionEntity.CreateScheduled(
            Guid.NewGuid(), "seller", Item.Create("Item", "Description"), 1m,
            DateTimeOffset.UtcNow.AddDays(1));
        var message = new ProcessBulkAuctionUpdateCommand
        {
            CorrelationId = Guid.NewGuid(),
            AuctionIds = [auction.Id],
            Activate = true
        };
        var context = Stub<ConsumeContext<ProcessBulkAuctionUpdateCommand>>((method, _) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => CancellationToken.None,
            "Publish" => Task.CompletedTask,
            _ => throw new NotSupportedException(method.Name)
        });
        var writeRepository = Stub<IAuctionWriteRepository>((method, _) => method.Name switch
        {
            nameof(IAuctionWriteRepository.GetByIdsForUpdateAsync) => Task.FromResult(new List<AuctionEntity> { auction }),
            nameof(IAuctionWriteRepository.UpdateAsync) => Task.FromException(new OperationCanceledException()),
            _ => throw new NotSupportedException(method.Name)
        });
        var consumer = new BulkUpdateAuctionsConsumer(writeRepository, null!,
            new DateTimeProvider(), null!, NullLogger<BulkUpdateAuctionsConsumer>.Instance);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => consumer.Consume(context));
        Assert.Equal(Status.Live, auction.Status);
    }

    [Fact]
    public async Task BulkUpdate_UsesOneTrackedBatchAndKeepsItemTitlesInEvents()
    {
        var auctions = new List<AuctionEntity>
        {
            AuctionEntity.CreateScheduled(Guid.NewGuid(), "seller", Item.Create("First", "Description"),
                1m, DateTimeOffset.UtcNow.AddDays(1)),
            AuctionEntity.CreateScheduled(Guid.NewGuid(), "seller", Item.Create("Second", "Description"),
                1m, DateTimeOffset.UtcNow.AddDays(1))
        };
        var message = new ProcessBulkAuctionUpdateCommand
        {
            CorrelationId = Guid.NewGuid(),
            AuctionIds = auctions.Select(auction => auction.Id).ToList(),
            Activate = true
        };
        var published = new List<object>();
        var context = Stub<ConsumeContext<ProcessBulkAuctionUpdateCommand>>((method, args) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => CancellationToken.None,
            "Publish" => Record(args!),
            _ => throw new NotSupportedException(method.Name)
        });
        Task Record(object?[] args)
        {
            published.Add(args[0]!);
            return Task.CompletedTask;
        }
        var batchReads = 0;
        var updatedIds = new List<Guid>();
        var writeRepository = Stub<IAuctionWriteRepository>((method, args) => method.Name switch
        {
            nameof(IAuctionWriteRepository.GetByIdsForUpdateAsync) => ReadBatch(),
            nameof(IAuctionWriteRepository.UpdateAsync) => RecordUpdate((AuctionEntity)args![0]!),
            _ => throw new NotSupportedException(method.Name)
        });
        Task<List<AuctionEntity>> ReadBatch()
        {
            batchReads++;
            return Task.FromResult(auctions);
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
        var options = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseNpgsql("Host=localhost;Database=auction_bulk_test")
            .Options;
        await using var dbContext = new AuctionDbContext(options);
        var consumer = new BulkUpdateAuctionsConsumer(writeRepository, unitOfWork,
            new DateTimeProvider(), dbContext, NullLogger<BulkUpdateAuctionsConsumer>.Instance);

        await consumer.Consume(context);

        Assert.Equal(1, batchReads);
        Assert.Equal(message.AuctionIds, updatedIds);
        Assert.All(auctions, auction => Assert.Equal(Status.Live, auction.Status));
        Assert.Equal(new[] { "First", "Second" }, auctions.Select(auction =>
            Assert.Single(auction.DomainEvents.OfType<AuctionStatusChangedDomainEvent>()).Title));
        var progress = Assert.Single(published.OfType<ReportJobBatchProgressCommand>());
        Assert.Equal(2, progress.CompletedCount);
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
