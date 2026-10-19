using Auctions.Application.Features.Auctions.CancelAuction;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Auctions.Domain.Enums;
using Auctions.Infrastructure.Persistence;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Infrastructure.Auditing;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TestSupport;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Infrastructure.Tests;

public class AuctionAuditOutboxTests
{
    [PostgresFact]
    public async Task CancellationPersistsAuditInTheSameSaveAsAuction()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var services = new ServiceCollection().AddLogging();
        services.AddDbContext<AuctionDbContext>(options => options.UseNpgsql(database.ConnectionString));
        services.AddMassTransit(configure =>
        {
            configure.AddEntityFrameworkOutbox<AuctionDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });
            configure.UsingInMemory();
        });
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        await db.Database.EnsureCreatedAsync();
        var sellerId = Guid.NewGuid();
        var auction = AuctionEntity.CreateScheduled(sellerId, "seller", Item.Create("Item", "Description"),
            10m, DateTimeOffset.UtcNow.AddDays(1));
        db.Auctions.Add(auction);
        await db.SaveChangesAsync();
        var repository = TestProxy.Create<IAuctionWriteRepository>((method, _) => method.Name switch
        {
            nameof(IAuctionWriteRepository.GetByIdForUpdateAsync) => Task.FromResult<AuctionEntity?>(auction),
            nameof(IAuctionWriteRepository.UpdateAsync) => Task.CompletedTask,
            _ => throw new NotSupportedException(method.Name)
        });
        var saves = 0;
        var unitOfWork = TestProxy.Create<IUnitOfWork>((method, args) =>
        {
            Assert.Equal(nameof(IUnitOfWork.SaveChangesAsync), method.Name);
            saves++;
            return db.SaveChangesAsync((CancellationToken)args![0]!);
        });
        var publisher = new AuditPublisher(scope.ServiceProvider.GetRequiredService<IPublishEndpoint>(),
            TestProxy.Create<IAuditContext>((method, _) => method.Name == "get_UserId" ? sellerId : null),
            NullLogger<AuditPublisher>.Instance, "auction-service");
        var handler = new CancelAuctionCommandHandler(repository, NullLogger<CancelAuctionCommandHandler>.Instance,
            unitOfWork, publisher);

        var result = await handler.Handle(new CancelAuctionCommand(auction.Id, sellerId, "Item unavailable"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, saves);
        await using var observer = new AuctionDbContext(new DbContextOptionsBuilder<AuctionDbContext>()
            .UseNpgsql(database.ConnectionString).Options);
        Assert.Equal(Status.Cancelled, (await observer.Auctions.SingleAsync()).Status);
        var message = await observer.Set<OutboxMessage>().SingleAsync();
        Assert.Contains("Item unavailable", message.Body);
        Assert.Contains("AuctionAuditData", message.Body);
    }
}
