using Auctions.Application.Features.Auctions.BuyNow;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Extensions;
using Auctions.Infrastructure.Persistence;
using Auctions.Infrastructure.Persistence.Repositories;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Application.Abstractions.Providers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Messaging.Consumers;
using Notification.Infrastructure.Persistence;
using Orchestration.Infrastructure.Messaging;
using Orchestration.Infrastructure.Persistence;
using Orchestration.Sagas.BuyNow;
using OrchestrationService.Contracts.Events;
using Payment.Application.Interfaces;
using Payment.Infrastructure.Extensions;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Repositories;
using TestSupport;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Orchestration.Infrastructure.Tests;

public class SagaBrokerTests
{
    [BrokerFact]
    public async Task RegisteredSagaHostResumesAfterRestartAndCompletesBothSaleFlows()
    {
        await using var auctionDb = await PostgresTestDatabase.CreateAsync();
        await using var paymentDb = await PostgresTestDatabase.CreateAsync();
        await using var notificationDb = await PostgresTestDatabase.CreateAsync();
        await using var orchestrationDb = await PostgresTestDatabase.CreateAsync();
        await using var auctions = AuctionProvider(auctionDb.ConnectionString);
        var payments = PaymentProvider(paymentDb.ConnectionString);
        await using var notifications = NotificationProvider(notificationDb.ConnectionString);
        var host = HostProvider(orchestrationDb.ConnectionString);
        var retired = new List<ServiceProvider>();
        try
        {
            await Migrate<AuctionDbContext>(auctions); await Migrate<PaymentDbContext>(payments);
            await Migrate<NotificationDbContext>(notifications); await Migrate<OrchestrationDbContext>(host);
            await Start(auctions); await Start(payments); await Start(notifications); await Start(host);
            // Keep Payment's durable queue, then suspend it while the saga persists CreatingOrder.
            await Stop(payments);
            var auctionId = await SeedAuction(auctions);
            var buyer = Guid.NewGuid();
            var result = await Buy(auctions, auctionId, buyer);
            Assert.Equal("Processing", result.Status);
            Assert.False(result.Success);
            var replay = await Buy(auctions, auctionId, buyer);
            Assert.Equal(result.CorrelationId, replay.CorrelationId);
            await Eventually(async () => await State(host, result.CorrelationId) == "CreatingOrder");
            await Stop(host); retired.Add(host);
            host = HostProvider(orchestrationDb.ConnectionString);
            retired.Add(payments);
            payments = PaymentProvider(paymentDb.ConnectionString);
            await Start(host); await Start(payments);
            await Eventually(async () => await PurchaseStatus(auctions, result.CorrelationId) == "Completed");
            await using (var scope = payments.CreateAsyncScope())
            {
                var order = await scope.ServiceProvider.GetRequiredService<PaymentDbContext>().Orders.SingleAsync(x => x.AuctionId == auctionId);
                Assert.Equal(buyer, order.BuyerId); Assert.False(order.AwaitingBuyNowCompletion);
            }
            // Replaying the start cannot re-open a finalized workflow or create another order.
            await host.GetRequiredService<IBus>().Publish(new BuyNowSagaStarted
            { CorrelationId = result.CorrelationId, AuctionId = auctionId, BuyerId = buyer });
            await Eventually(async () => await State(host, result.CorrelationId) == "Final");

            // A persisted overdue deadline is recovered after a host restart.
            await Stop(host); retired.Add(host);
            var timedOutAuction = await SeedAuction(auctions);
            var timeoutId = Guid.NewGuid();
            await using (var scope = auctions.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
                var auction = await db.Auctions.Include(x => x.Item).SingleAsync(x => x.Id == timedOutAuction);
                auction.ReserveBuyNow(timeoutId, buyer);
                db.Add(new BuyNowPurchase { CorrelationId = timeoutId, AuctionId = timedOutAuction, BuyerId = buyer });
                await db.SaveChangesAsync();
            }
            await using (var db = new OrchestrationDbContext(new DbContextOptionsBuilder<OrchestrationDbContext>().UseNpgsql(orchestrationDb.ConnectionString).Options))
            {
                db.Add(new BuyNowSagaState
                {
                    CorrelationId = timeoutId,
                    AuctionId = timedOutAuction,
                    BuyerId = buyer,
                    CurrentState = "ReservingAuction",
                    RecoveryStep = "ReleaseReservation",
                    TimeoutTokenId = Guid.NewGuid(),
                    TimeoutAt = DateTimeOffset.UtcNow.AddMinutes(-1)
                });
                await db.SaveChangesAsync();
            }
            host = HostProvider(orchestrationDb.ConnectionString); await Start(host);
            await Eventually(async () => await PurchaseStatus(auctions, timeoutId) == "Failed");
            await using (var scope = auctions.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
                var auction = await db.Auctions.SingleAsync(x => x.Id == timedOutAuction);
                Assert.Equal(Auctions.Domain.Enums.Status.Live, auction.Status);
                Assert.Contains(timeoutId, auction.CancelledBuyNowAttempts);
            }

            // An ordinary auction sale creates one winner order and two durable notifications.
            var finishedId = await SeedAuction(auctions);
            await using (var scope = auctions.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
                var auction = await db.Auctions.Include(x => x.Item).SingleAsync(x => x.Id == finishedId);
                auction.Finish(buyer, "winner", 80, true);
                await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
            }
            await Eventually(async () =>
            {
                await using var scope = host.CreateAsyncScope();
                return await scope.ServiceProvider.GetRequiredService<OrchestrationDbContext>().AuctionCompletionSagas
                    .AnyAsync(x => x.CorrelationId == finishedId && x.CurrentState == "Final");
            });
            await using (var scope = notifications.CreateAsyncScope())
                Assert.Equal(2, await scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Notifications.CountAsync(x => x.AuctionId == finishedId));
            await using (var scope = payments.CreateAsyncScope())
                Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<PaymentDbContext>().Orders.CountAsync(x => x.AuctionId == finishedId));
        }
        finally
        {
            await Stop(host); retired.Add(host);
            await Stop(notifications); await Stop(payments); retired.Add(payments); await Stop(auctions);
            foreach (var provider in retired) await provider.DisposeAsync();
        }
    }

    private static IConfiguration Config(string connection) => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ConnectionStrings:DefaultConnection"] = connection,
        ["RabbitMQ:Host"] = "localhost",
        ["RabbitMQ:Port"] = Environment.GetEnvironmentVariable("BACKEND_TEST_RABBIT_PORT"),
        ["RabbitMQ:Username"] = "guest",
        ["RabbitMQ:Password"] = "guest"
    }).Build();
    private static ServiceCollection Services()
    {
        var s = new ServiceCollection(); s.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Warning)); return s;
    }
    private static ServiceProvider HostProvider(string connection)
    {
        var s = Services(); s.AddOrchestration(Config(connection)); return s.BuildServiceProvider();
    }
    private static ServiceProvider AuctionProvider(string connection)
    {
        var s = Services(); s.AddDbContext<AuctionDbContext>(x => x.UseNpgsql(connection));
        s.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(BuyNowCommand).Assembly));
        s.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        s.AddSingleton(TestProxy.Create<IAuditContext>((m, _) => m.Name == "get_UserId" ? Guid.Empty : null));
        s.AddSingleton(TestProxy.Create<IAuditPublisher>((_, _) => Task.CompletedTask));
        s.AddSingleton<IDistributedLock, TestLock>();
        s.AddScoped<IAuctionWriteRepository, AuctionRepository>();
        s.AddScoped<IUnitOfWork, Auctions.Infrastructure.Persistence.UnitOfWork>();
        s.AddScoped<BuyNowCommandHandler>();
        Auctions.Infrastructure.Extensions.MassTransitOutboxExtensions.AddMassTransitWithOutbox(s, Config(connection));
        return s.BuildServiceProvider();
    }
    private static ServiceProvider PaymentProvider(string connection)
    {
        var s = Services(); s.AddDbContext<PaymentDbContext>(x => x.UseNpgsql(connection));
        s.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(Payment.Application.EventHandlers.OrderCreatedDomainEventHandler).Assembly));
        s.AddScoped<IOrderRepository, OrderRepository>();
        s.AddScoped<IUnitOfWork, Payment.Infrastructure.Persistence.UnitOfWork>();
        Payment.Infrastructure.Extensions.MassTransitExtensions.AddMassTransitWithOutbox(s, Config(connection)); return s.BuildServiceProvider();
    }
    private static ServiceProvider NotificationProvider(string connection)
    {
        var s = Services(); s.AddDbContext<NotificationDbContext>(x => x.UseNpgsql(connection));
        s.AddScoped<INotificationService>(sp => TestProxy.Create<INotificationService>((m, a) =>
            m.Name == nameof(INotificationService.CreateNotificationAsync)
                ? PersistNotification(sp.GetRequiredService<NotificationDbContext>(), (CreateNotificationDto)a![0]!)
                : throw new NotSupportedException(m.Name)));
        s.AddMassTransit(x =>
        {
            x.AddConsumer<SendAuctionCompletionNotificationsConsumer>();
            x.AddEntityFrameworkOutbox<NotificationDbContext>(o => { o.UsePostgres(); o.UseBusOutbox(); });
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", ushort.Parse(Environment.GetEnvironmentVariable("BACKEND_TEST_RABBIT_PORT")!), "/", h => { h.Username("guest"); h.Password("guest"); });
                cfg.ReceiveEndpoint("notification-auction-completion-saga", e =>
                { e.UseEntityFrameworkOutbox<NotificationDbContext>(context); e.ConfigureConsumer<SendAuctionCompletionNotificationsConsumer>(context); });
            });
        });
        return s.BuildServiceProvider();
    }
    private static async Task<NotificationDto> PersistNotification(NotificationDbContext db, CreateNotificationDto dto)
    {
        db.Notifications.Add(Notification.Domain.Entities.Notification.Create(dto.UserId, dto.UserId, dto.Type,
            "auction completed", "order ready", auctionId: dto.AuctionId, referenceId: dto.ReferenceId));
        await db.SaveChangesAsync(); return new NotificationDto();
    }
    private static async Task Migrate<T>(ServiceProvider provider) where T : DbContext
    {
        await using var scope = provider.CreateAsyncScope(); await scope.ServiceProvider.GetRequiredService<T>().Database.MigrateAsync();
    }
    private static async Task Start(ServiceProvider provider)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        foreach (var service in provider.GetServices<IHostedService>()) await service.StartAsync(timeout.Token);
        await provider.GetRequiredService<IBusControl>().StartAsync(timeout.Token);
    }
    private static async Task Stop(ServiceProvider provider)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        foreach (var service in provider.GetServices<IHostedService>().Reverse()) await service.StopAsync(timeout.Token);
    }
    private static async Task<Guid> SeedAuction(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        var auction = AuctionEntity.Create(new CreateAuctionParams(Guid.NewGuid(), "seller", Item.Create("item", "description"),
            10, DateTimeOffset.UtcNow.AddHours(1), BuyNowPrice: 100));
        auction.ClearDomainEvents(); db.Add(auction); await db.SaveChangesAsync(); return auction.Id;
    }
    private static async Task<Auctions.Application.DTOs.Auctions.BuyNowResultDto> Buy(ServiceProvider provider, Guid auction, Guid buyer)
    {
        await using var scope = provider.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<BuyNowCommandHandler>().Handle(new BuyNowCommand(auction, buyer, "buyer"), default);
        Assert.True(result.IsSuccess, result.Error?.Message); return result.Value!;
    }
    private static async Task<string?> PurchaseStatus(ServiceProvider provider, Guid id)
    {
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AuctionDbContext>().Set<BuyNowPurchase>()
            .Where(x => x.CorrelationId == id).Select(x => x.Status).SingleOrDefaultAsync();
    }
    private static async Task<string?> State(ServiceProvider provider, Guid id)
    {
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<OrchestrationDbContext>().BuyNowSagas
            .Where(x => x.CorrelationId == id).Select(x => x.CurrentState).SingleOrDefaultAsync();
    }
    private static async Task Eventually(Func<Task<bool>> predicate)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(40));
        while (!await predicate()) await Task.Delay(100, timeout.Token);
    }
    private sealed class TestLock : IDistributedLock, IAsyncDisposable
    {
        public Task<IAsyncDisposable?> AcquireAsync(string resourceKey, TimeSpan? expiry = null, TimeSpan? wait = null, CancellationToken cancellationToken = default) => Task.FromResult<IAsyncDisposable?>(this);
        public Task<IAsyncDisposable?> TryAcquireAsync(string resourceKey, TimeSpan? expiry = null, CancellationToken cancellationToken = default) => Task.FromResult<IAsyncDisposable?>(this);
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
