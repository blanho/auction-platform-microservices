using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Auctions.Domain.Enums;
using Auctions.Infrastructure.Messaging.Consumers;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Application.Authorization;
using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationService.Contracts.Events;
using TestSupport;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Infrastructure.Tests;

public class SellerAccountEventTests
{
    public enum AccountChange { Deleted, Suspended, RolesRevoked }

    [Theory]
    [InlineData(AccountChange.Deleted)]
    [InlineData(AccountChange.Suspended)]
    [InlineData(AccountChange.RolesRevoked)]
    public async Task AccountChangePreservesCancellationGroupingAndSaveOrder(AccountChange change)
    {
        using var cancellation = new CancellationTokenSource();
        var fixture = new Fixture(cancellation.Token);
        var first = fixture.Auction("First", "bidder-a");
        var second = fixture.Auction("Second", "bidder-b");
        var third = fixture.Auction("Third", "bidder-a", scheduled: true);
        var noBid = fixture.Auction("No bid");
        var noWinner = fixture.Auction("No winner");
        noWinner.UpdateHighBid(10);
        fixture.Active.AddRange([first, second, third, noBid, noWinner]);
        var finished = fixture.Auction("Finished");
        finished.Finish(Guid.NewGuid(), "buyer", 20, true);
        fixture.All.AddRange(fixture.Active);
        fixture.All.Add(finished);

        await fixture.ConsumeAsync(change);

        Assert.All(fixture.Active, auction => Assert.Equal(Status.Cancelled, auction.Status));
        Assert.Equal(Status.Finished, finished.Status);
        Assert.Equal(change == AccountChange.Deleted ? "[Deleted User - seller]" : "seller", finished.SellerUsername);
        Assert.Equal(new[] { "First", "Third", "Second" }, fixture.Notifications.Select(message => message.AuctionTitle));
        Assert.Equal(new[] { "bidder-a", "bidder-a", "bidder-b" }, fixture.Notifications.Select(message => message.RecipientUsername));
        var reason = change switch
        {
            AccountChange.Deleted => "Seller account no longer active",
            AccountChange.Suspended => "Seller account suspended",
            _ => "Seller account privileges changed"
        };
        Assert.All(fixture.Notifications, message =>
        {
            Assert.Equal(reason, message.Reason);
            Assert.True(message.RefundExpected);
        });
        Assert.Equal(1, fixture.Calls.Count(call => call == "save"));
        var firstNotification = fixture.Calls.FindIndex(call => call.StartsWith("notify:", StringComparison.Ordinal));
        var save = fixture.Calls.IndexOf("save");
        Assert.Equal(change == AccountChange.Deleted, firstNotification < save);
        Assert.Equal(change == AccountChange.Deleted ? 6 : 5, fixture.Calls.Count(call => call.StartsWith("write:", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData(AccountChange.Deleted)]
    [InlineData(AccountChange.Suspended)]
    [InlineData(AccountChange.RolesRevoked)]
    public async Task NoActiveAuctionsProduceNoNotifications(AccountChange change)
    {
        var fixture = new Fixture();

        await fixture.ConsumeAsync(change);

        Assert.Empty(fixture.Notifications);
        Assert.DoesNotContain(fixture.Calls, call => call.StartsWith("write:", StringComparison.Ordinal));
        Assert.Equal(change == AccountChange.Deleted ? 1 : 0, fixture.Calls.Count(call => call == "save"));
    }

    [Theory]
    [InlineData(Roles.Seller)]
    [InlineData(Roles.Admin)]
    public async Task RetainedSellingPermissionSkipsAuctionProcessing(string role)
    {
        var fixture = new Fixture();

        await fixture.ConsumeRolesAsync([role]);

        Assert.Empty(fixture.Calls);
        Assert.Empty(fixture.Notifications);
    }

    [Theory]
    [InlineData(AccountChange.Deleted)]
    [InlineData(AccountChange.Suspended)]
    [InlineData(AccountChange.RolesRevoked)]
    public async Task CanceledWritePropagatesBeforeSaveOrNotifications(AccountChange change)
    {
        using var cancellation = new CancellationTokenSource();
        var fixture = new Fixture(cancellation.Token) { CancelWrite = true };
        fixture.Active.Add(fixture.Auction("First", "bidder"));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.ConsumeAsync(change));

        Assert.DoesNotContain("save", fixture.Calls);
        Assert.Empty(fixture.Notifications);
    }

    [Fact]
    public async Task UsernameSyncUpdatesOnlyChangedNamesAndDoesNotSaveOnReplay()
    {
        using var cancellation = new CancellationTokenSource();
        var fixture = new Fixture(cancellation.Token);
        var seller = fixture.Auction("Seller item");
        var unchangedSeller = fixture.Auction("Already synced seller");
        unchangedSeller.UpdateSellerUsername("new-name");
        var won = fixture.Auction("Won item");
        won.Finish(fixture.UserId, "old-name", 20, true);
        var unchangedWinner = fixture.Auction("Already synced winner");
        unchangedWinner.Finish(fixture.UserId, "new-name", 20, true);
        fixture.All.AddRange([seller, unchangedSeller]);
        fixture.Won.AddRange([won, unchangedWinner]);

        await fixture.ConsumeUpdatedAsync();
        await fixture.ConsumeUpdatedAsync();

        Assert.Equal("new-name", seller.SellerUsername);
        Assert.Equal("new-name", won.WinnerUsername);
        Assert.Equal(new[] { "write:Seller item", "write:Won item", "save" },
            fixture.Calls.Where(call => call.StartsWith("write:", StringComparison.Ordinal) || call == "save"));
    }

    private sealed class Fixture
    {
        private readonly CancellationToken _token;
        private readonly IAuctionReadRepository _read;
        private readonly IAuctionWriteRepository _write;
        private readonly IUnitOfWork _unit;
        private readonly IEventPublisher _publisher;
        public Guid UserId { get; } = Guid.NewGuid();
        public List<AuctionEntity> Active { get; } = [];
        public List<AuctionEntity> All { get; } = [];
        public List<AuctionEntity> Won { get; } = [];
        public List<string> Calls { get; } = [];
        public List<AuctionCancelledNotificationEvent> Notifications { get; } = [];
        public bool CancelWrite { get; init; }

        public Fixture(CancellationToken token = default)
        {
            _token = token;
            _read = TestProxy.Create<IAuctionReadRepository>((method, args) =>
            {
                Assert.Equal(UserId, args![0]);
                Assert.Equal(_token, args.OfType<CancellationToken>().Single());
                Calls.Add("read:" + method.Name);
                return method.Name switch
                {
                    nameof(IAuctionReadRepository.GetActiveAuctionsBySellerIdAsync) => Task.FromResult(Active),
                    nameof(IAuctionReadRepository.GetAllBySellerIdAsync) => Task.FromResult(All),
                    nameof(IAuctionReadRepository.GetAuctionsWithWinnerIdAsync) => Task.FromResult(Won),
                    _ => throw new NotSupportedException(method.Name)
                };
            });
            _write = TestProxy.Create<IAuctionWriteRepository>((method, args) =>
            {
                Assert.Equal(nameof(IAuctionWriteRepository.UpdateAsync), method.Name);
                Assert.Equal(_token, args!.OfType<CancellationToken>().Single());
                Calls.Add("write:" + Assert.IsType<AuctionEntity>(args![0]).Item.Title);
                if (CancelWrite) throw new OperationCanceledException(_token);
                return Task.CompletedTask;
            });
            _unit = TestProxy.Create<IUnitOfWork>((method, args) =>
            {
                Assert.Equal(nameof(IUnitOfWork.SaveChangesAsync), method.Name);
                Assert.Equal(_token, args!.OfType<CancellationToken>().Single());
                Calls.Add("save");
                return Task.FromResult(1);
            });
            _publisher = TestProxy.Create<IEventPublisher>((_, args) =>
            {
                Assert.Equal(_token, args!.OfType<CancellationToken>().Single());
                var message = Assert.IsType<AuctionCancelledNotificationEvent>(args![0]);
                Notifications.Add(message);
                Calls.Add("notify:" + message.AuctionTitle);
                return Task.CompletedTask;
            });
        }

        public AuctionEntity Auction(string title, string? bidder = null, bool scheduled = false)
        {
            var end = DateTimeOffset.UtcNow.AddDays(1);
            var item = Item.Create(title, "Description");
            var auction = scheduled
                ? AuctionEntity.CreateScheduled(UserId, "seller", item, 10, end)
                : AuctionEntity.Create(new CreateAuctionParams(UserId, "seller", item, 10, end));
            if (bidder is not null) auction.UpdateHighBid(15, Guid.NewGuid(), bidder);
            return auction;
        }

        public Task ConsumeAsync(AccountChange change) => change switch
        {
            AccountChange.Deleted => new UserDeletedConsumer(_read, _write, _unit, _publisher,
                NullLogger<UserDeletedConsumer>.Instance).Consume(Context(new UserDeletedEvent
                { UserId = UserId.ToString(), Username = "seller" })),
            AccountChange.Suspended => new UserSuspendedConsumer(_read, _write, _unit, _publisher,
                NullLogger<UserSuspendedConsumer>.Instance).Consume(Context(new UserSuspendedEvent
                { UserId = UserId.ToString(), Username = "seller", Reason = "policy" })),
            _ => ConsumeRolesAsync([])
        };

        public Task ConsumeRolesAsync(string[] roles) => new UserRoleChangedConsumer(_read, _write, _unit, _publisher,
            NullLogger<UserRoleChangedConsumer>.Instance).Consume(Context(new UserRoleChangedEvent
            { UserId = UserId.ToString(), Username = "seller", Roles = roles }));

        public Task ConsumeUpdatedAsync() => new UserUpdatedConsumer(_read, _write, _unit,
            NullLogger<UserUpdatedConsumer>.Instance).Consume(Context(new UserUpdatedEvent
            { UserId = UserId.ToString(), Username = "new-name" }));

        private ConsumeContext<T> Context<T>(T message) where T : class =>
            TestProxy.Create<ConsumeContext<T>>((method, _) => method.Name switch
            {
                "get_Message" => message,
                "get_CancellationToken" => _token,
                _ => throw new NotSupportedException(method.Name)
            });
    }
}
