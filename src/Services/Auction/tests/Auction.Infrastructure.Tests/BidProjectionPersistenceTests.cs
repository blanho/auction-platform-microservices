using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Messaging.Consumers;
using Auctions.Infrastructure.Persistence;
using Auctions.Infrastructure.Persistence.Repositories;
using BidService.Contracts.Constants;
using BidService.Contracts.Events;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TestSupport;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Infrastructure.Tests;

public class BidProjectionPersistenceTests
{
    [PostgresFact]
    public async Task HighestAcceptedBidsPersistAndDuplicateOrOlderMessagesCannotRegressTheProjection()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<AuctionDbContext>().UseNpgsql(database.ConnectionString).Options;
        var auction = AuctionEntity.Create(new CreateAuctionParams(Guid.NewGuid(), "seller",
            Item.Create("item", "description"), 100, DateTimeOffset.UtcNow.AddDays(1)));
        await using (var db = new AuctionDbContext(options))
        {
            await db.Database.MigrateAsync();
            db.Add(auction);
            await db.SaveChangesAsync();
        }
        var bidder = Guid.NewGuid();
        foreach (var (amount, status) in new[]
        {
            (130m, BidEventStatusNames.Accepted), (130m, BidEventStatusNames.Accepted),
            (120m, BidEventStatusNames.Accepted), (140m, BidEventStatusNames.AcceptedBelowReserve),
            (999m, BidEventStatusNames.Placed)
        })
        {
            await using var db = new AuctionDbContext(options);
            var audit = TestProxy.Create<IAuditContext>((method, _) => method.Name == "get_UserId" ? Guid.Empty : null);
            var repository = new AuctionRepository(db, new DateTimeProvider(), audit);
            var unitOfWork = TestProxy.Create<IUnitOfWork>((method, args) => method.Name == "SaveChangesAsync"
                ? db.SaveChangesAsync((CancellationToken)args![0]!) : throw new NotSupportedException(method.Name));
            var message = new HighestBidUpdatedEvent
            {
                Id = Guid.NewGuid(), AuctionId = auction.Id, BidderId = bidder,
                BidderUsername = "bidder", NewHighestAmount = amount, BidStatus = status
            };
            var context = TestProxy.Create<ConsumeContext<HighestBidUpdatedEvent>>((method, _) => method.Name switch
            {
                "get_Message" => message,
                "get_CancellationToken" => CancellationToken.None,
                _ => throw new NotSupportedException(method.Name)
            });
            await new BidPlacedConsumer(repository, unitOfWork, NullLogger<BidPlacedConsumer>.Instance).Consume(context);
        }
        await using var check = new AuctionDbContext(options);
        var persisted = await check.Auctions.SingleAsync();
        Assert.Equal(140m, persisted.CurrentHighBid);
        Assert.Equal(bidder, persisted.WinnerId);
    }
}
