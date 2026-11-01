using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Jobs;
using Auctions.Infrastructure.Persistence;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Quartz;
using TestSupport;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Infrastructure.Tests;

public class AuctionJobCancellationTests
{
    [Theory]
    [InlineData("activation")]
    [InlineData("completion")]
    [InlineData("ending-soon")]
    public async Task CancellationFromAnItemOperationStopsTheJob(string jobType)
    {
        using var cancellation = new CancellationTokenSource();
        var item = Item.Create("Item", "Description");
        var auction = jobType == "activation"
            ? AuctionEntity.CreateScheduled(Guid.NewGuid(), "seller", item, 10, DateTimeOffset.UtcNow.AddDays(1))
            : AuctionEntity.Create(new CreateAuctionParams(Guid.NewGuid(), "seller", item, 10, DateTimeOffset.UtcNow.AddDays(-1)));
        var services = new ServiceCollection();
        services.AddSingleton(TestProxy.Create<IAuctionReadRepository>((_, args) =>
        {
            Assert.Equal(cancellation.Token, args!.OfType<CancellationToken>().Single());
            return Task.FromResult(new List<AuctionEntity> { auction });
        }));
        services.AddSingleton(TestProxy.Create<IAuctionWriteRepository>((_, args) => Cancel(args)));
        services.AddSingleton(TestProxy.Create<IBidFinalizationClient>((_, args) => Cancel(args)));
        services.AddSingleton(TestProxy.Create<IBookmarkRepository>((_, args) => Cancel(args)));
        services.AddSingleton(TestProxy.Create<IUnitOfWork>((_, _) =>
            throw new Xunit.Sdk.XunitException("A canceled item must not be saved.")));
        services.AddSingleton(TestProxy.Create<IEventPublisher>((_, _) =>
            throw new Xunit.Sdk.XunitException("A canceled lookup must not publish a notification.")));
        services.AddScoped(_ => new AuctionDbContext(new DbContextOptionsBuilder<AuctionDbContext>()
            .UseNpgsql("Host=localhost;Database=unused").Options));
        using var provider = services.BuildServiceProvider();
        IJob job = jobType switch
        {
            "activation" => new AuctionActivationJob(NullLogger<AuctionActivationJob>.Instance, provider),
            "completion" => new CheckAuctionFinishedJob(NullLogger<CheckAuctionFinishedJob>.Instance, provider),
            _ => new AuctionEndingSoonNotificationJob(NullLogger<AuctionEndingSoonNotificationJob>.Instance, provider)
        };
        var detail = TestProxy.Create<IJobDetail>((method, _) => method.Name == "get_Key"
            ? new JobKey("cancellation-test") : throw new NotSupportedException(method.Name));
        var context = TestProxy.Create<IJobExecutionContext>((method, _) => method.Name switch
        {
            "get_JobDetail" => detail,
            "get_FireTimeUtc" => DateTimeOffset.UtcNow,
            "get_RefireCount" => 0,
            "get_CancellationToken" => cancellation.Token,
            _ => throw new NotSupportedException(method.Name)
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => job.Execute(context));

        Assert.True(cancellation.IsCancellationRequested);

        object Cancel(object?[]? args)
        {
            Assert.Equal(cancellation.Token, args!.OfType<CancellationToken>().Single());
            cancellation.Cancel();
            throw new OperationCanceledException(cancellation.Token);
        }
    }
}
