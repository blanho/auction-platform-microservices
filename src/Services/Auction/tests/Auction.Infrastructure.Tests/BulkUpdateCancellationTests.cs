using System.Reflection;
using AuctionService.Contracts.Commands;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Auctions.Domain.Enums;
using Auctions.Infrastructure.Messaging.Consumers;
using BuildingBlocks.Application.Abstractions.Providers;
using MassTransit;
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
        var readRepository = Stub<IAuctionReadRepository>((method, _) => method.Name switch
        {
            nameof(IAuctionReadRepository.GetByIdsAsync) => Task.FromResult(new List<AuctionEntity> { auction }),
            _ => throw new NotSupportedException(method.Name)
        });
        var writeRepository = Stub<IAuctionWriteRepository>((method, _) => method.Name switch
        {
            nameof(IAuctionWriteRepository.UpdateAsync) => Task.FromException(new OperationCanceledException()),
            _ => throw new NotSupportedException(method.Name)
        });
        var consumer = new BulkUpdateAuctionsConsumer(readRepository, writeRepository, null!,
            new DateTimeProvider(), null!, NullLogger<BulkUpdateAuctionsConsumer>.Instance);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => consumer.Consume(context));
        Assert.Equal(Status.Live, auction.Status);
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
