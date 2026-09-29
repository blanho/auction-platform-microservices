using Bidding.Application.Filtering;
using Bidding.Application.Features.Bids.GetWinningBids;
using Bidding.Application.Interfaces;
using Bidding.Domain.Entities;
using Bidding.Infrastructure.Persistence;
using Bidding.Infrastructure.Repositories;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TestSupport;
using Xunit;

namespace Bidding.Infrastructure.Tests;

public class WinningBidPaymentTests
{
    [PostgresFact]
    public async Task PaidFilterRunsBeforeCountAndPaginationAndReturnsAuthoritativeStatus()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = new BidDbContext(new DbContextOptionsBuilder<BidDbContext>()
            .UseNpgsql(database.ConnectionString).Options);
        await db.Database.MigrateAsync();
        var user = Guid.NewGuid();
        var bids = Enumerable.Range(1, 3).Select(i => Bid.Create(Guid.NewGuid(), user, "buyer", i * 10,
            DateTimeOffset.UtcNow.AddMinutes(-i))).ToList();
        foreach (var bid in bids) bid.Accept();
        db.Bids.AddRange(bids);
        await db.SaveChangesAsync();
        var repo = new BidRepository(db, new DateTimeProvider(), null!);
        IReadOnlyDictionary<Guid, AuctionPaymentStatus> statuses = new Dictionary<Guid, AuctionPaymentStatus>
        {
            [bids[1].AuctionId] = new("Completed", true),
            [bids[2].AuctionId] = new("Refunded", false)
        };
        var payments = TestProxy.Create<IPaymentStatusClient>((_, _) => Task.FromResult(statuses));
        var snapshots = TestProxy.Create<IAuctionSnapshotRepository>((_, _) => Task.FromResult<AuctionSnapshot?>(null));
        var handler = new GetWinningBidsQueryHandler(repo, payments, snapshots,
            NullLogger<GetWinningBidsQueryHandler>.Instance);
        var paid = await handler.Handle(new GetWinningBidsQuery(user, IsPaid: true, PageSize: 1), default);
        Assert.True(paid.IsSuccess);
        Assert.Equal(1, paid.Value!.TotalCount);
        var paidBid = Assert.Single(paid.Value.Items);
        Assert.Equal(bids[1].AuctionId, paidBid.AuctionId);
        Assert.True(paidBid.IsPaid);
        Assert.Equal("Completed", paidBid.PaymentStatus);
        var unpaid = await handler.Handle(new GetWinningBidsQuery(user, IsPaid: false, Page: 2, PageSize: 1), default);
        Assert.Equal(2, unpaid.Value!.TotalCount);
        Assert.Equal("Refunded", Assert.Single(unpaid.Value.Items).PaymentStatus);
        var all = await handler.Handle(new GetWinningBidsQuery(user), default);
        Assert.Equal(3, all.Value!.TotalCount);
    }

    [Fact]
    public async Task UnavailablePaymentServiceDoesNotReturnFalseUnpaidStatus()
    {
        var payments = TestProxy.Create<IPaymentStatusClient>((_, _) =>
            Task.FromException<IReadOnlyDictionary<Guid, AuctionPaymentStatus>>(new TimeoutException()));
        var handler = new GetWinningBidsQueryHandler(null!, payments, null!,
            NullLogger<GetWinningBidsQueryHandler>.Instance);
        await Assert.ThrowsAsync<TimeoutException>(() => handler.Handle(new GetWinningBidsQuery(Guid.NewGuid()), default));
    }
}
