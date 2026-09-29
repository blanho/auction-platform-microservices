using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Application.Features.Auctions.ExportAuctions;
using Auctions.Application.Interfaces;
using Auctions.Infrastructure.Extensions;
using Auctions.Infrastructure.Persistence;
using Auctions.Infrastructure.Persistence.Repositories;
using Auctions.Infrastructure.Services;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Application.Implementations;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using StorageService.Contracts.Reports;
using TestSupport;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Infrastructure.Tests;

public class AuctionBrokerTests
{
    [BrokerFact]
    public async Task RegisteredWorkflowsHandleReorderedBatchesRollbackRedeliveryAndExport()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<AuctionDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using (var db = new AuctionDbContext(options)) await db.Database.MigrateAsync();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["RabbitMQ:Host"] = "localhost",
            ["RabbitMQ:Port"] = Environment.GetEnvironmentVariable("BACKEND_TEST_RABBIT_PORT"),
            ["RabbitMQ:Username"] = "guest",
            ["RabbitMQ:Password"] = "guest",
            ["ReportStorage:BaseUrl"] = "https://storage.test",
            ["ReportStorage:ApiKey"] = "test-key"
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddDbContext<AuctionDbContext>(o => o.UseNpgsql(database.ConnectionString));
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton(TestProxy.Create<IAuditContext>((method, _) => method.Name == "get_UserId" ? Guid.Empty : throw new NotSupportedException(method.Name)));
        services.AddSingleton(TestProxy.Create<IMediator>((_, _) => Task.CompletedTask));
        services.AddSingleton<ISanitizationService, HtmlSanitizationService>();
        services.AddSingleton(new FailureSwitch());
        services.AddScoped<IUnitOfWork, FailOnceUnitOfWork>();
        services.AddScoped<IAuctionBulkRepository, AuctionBulkRepository>();
        services.AddScoped<AuctionRepository>();
        services.AddScoped<IAuctionReadRepository>(p => p.GetRequiredService<AuctionRepository>());
        services.AddScoped<IAuctionWriteRepository>(p => p.GetRequiredService<AuctionRepository>());
        services.AddSingleton<IReportExporter, CsvReportExporter>();
        services.AddSingleton<IReportExporter, JsonReportExporter>();
        var storage = new ReportHandler();
        services.AddScoped(_ => new AuctionExportStorageClient(new HttpClient(storage, false), configuration));
        services.AddMassTransitWithOutbox(configuration);
        await using var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<IBusControl>();
        var hosted = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>().ToList();
        using var lifetime = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        foreach (var service in hosted) await service.StartAsync(lifetime.Token);
        await bus.StartAsync(lifetime.Token);
        var imports = new ConcurrentQueue<AuctionImportCompletedEvent>();
        var updates = new ConcurrentQueue<BulkAuctionUpdateCompletedEvent>();
        var exports = new ConcurrentQueue<AuctionExportCompletedEvent>();
        var results = bus.ConnectReceiveEndpoint("review-results-" + Guid.NewGuid().ToString("N"), e =>
        {
            e.Handler<AuctionImportCompletedEvent>(c => { imports.Enqueue(c.Message); return Task.CompletedTask; });
            e.Handler<BulkAuctionUpdateCompletedEvent>(c => { updates.Enqueue(c.Message); return Task.CompletedTask; });
            e.Handler<AuctionExportCompletedEvent>(c => { exports.Enqueue(c.Message); return Task.CompletedTask; });
        });
        await results.Ready;
        try
        {
            var correlation = Guid.NewGuid();
            var owner = Guid.NewGuid();
            var last = new ProcessAuctionImportBatchCommand
            {
                CorrelationId = correlation,
                SellerId = owner,
                SellerUsername = "seller",
                Currency = "USD",
                BatchNumber = 2,
                TotalBatches = 2,
                TotalRows = 2,
                Rows = [Row(2)]
            };
            await bus.Publish(last);
            await Until(async () => { await using var db = new AuctionDbContext(options); return await db.Auctions.CountAsync() == 1; });
            Assert.Empty(imports);
            await bus.Publish(last);
            await bus.Publish(last with { BatchNumber = 1, Rows = [Row(1)] });
            await Until(() => Task.FromResult(imports.Any(x => x.CorrelationId == correlation)));
            var completed = Assert.Single(imports, x => x.CorrelationId == correlation);
            Assert.Equal(2, completed.SucceededCount);
            Assert.Equal(0, completed.FailedCount);
            Assert.Equal(1, provider.GetRequiredService<FailureSwitch>().Failures);
            await using (var db = new AuctionDbContext(options)) Assert.Equal(2, await db.Auctions.CountAsync());

            var single = new ProcessAuctionImportCommand
            { CorrelationId = Guid.NewGuid(), SellerId = owner, SellerUsername = "seller", Currency = "USD", Rows = [Row(3)] };
            await bus.Publish(single);
            await Until(() => Task.FromResult(imports.Any(x => x.CorrelationId == single.CorrelationId)));
            List<Guid> ids;
            await using (var db = new AuctionDbContext(options)) ids = await db.Auctions.Select(x => x.Id).ToListAsync();
            var update = new ProcessBulkAuctionUpdateCommand
            { CorrelationId = Guid.NewGuid(), RequestedBy = owner, AuctionIds = ids, Activate = false };
            await bus.Publish(update);
            await Until(() => Task.FromResult(updates.Any(x => x.CorrelationId == update.CorrelationId)));
            Assert.Equal(3, Assert.Single(updates, x => x.CorrelationId == update.CorrelationId).SucceededCount);
            var export = new ProcessAuctionExportCommand
            { CorrelationId = Guid.NewGuid(), RequestedBy = owner, Format = "Json" };
            await bus.Publish(export);
            await Until(() => Task.FromResult(exports.Any(x => x.CorrelationId == export.CorrelationId)));
            var report = Assert.Single(exports, x => x.CorrelationId == export.CorrelationId);
            Assert.Equal(3, report.TotalRecords);
            Assert.Equal("/files/test-report/download", report.DownloadUrl);
            Assert.Equal(export.CorrelationId.ToString(), storage.RequestId);
            Assert.Contains("Row 1", storage.Content);
        }
        finally
        {
            using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await results.StopAsync(stop.Token);
            foreach (var service in hosted.AsEnumerable().Reverse()) await service.StopAsync(stop.Token);
            await bus.StopAsync(stop.Token);
        }
    }

    private static ImportAuctionItemPayload Row(int n) => new()
    { RowNumber = n, Title = $"Row {n}", Description = "Valid description", ReservePrice = 10, AuctionEnd = DateTimeOffset.UtcNow.AddDays(2) };
    private static async Task Until(Func<Task<bool>> predicate)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        while (!await predicate()) await Task.Delay(100, timeout.Token);
    }
    public sealed class FailureSwitch
    {
        public int Failures;
    }
    public sealed class FailOnceUnitOfWork(AuctionDbContext db, IMediator mediator, FailureSwitch state) : UnitOfWork(db, mediator)
    {
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var inserting = Context.ChangeTracker.Entries<AuctionEntity>().Any(x => x.State == EntityState.Added);
            var result = await base.SaveChangesAsync(cancellationToken);
            if (inserting && Interlocked.CompareExchange(ref state.Failures, 1, 0) == 0)
                throw new IOException("Injected failure after insert, before transaction commit");
            return result;
        }
    }
    private sealed class ReportHandler : HttpMessageHandler
    {
        public string? RequestId;
        public string Content = "";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestId = request.Headers.GetValues(ReportStorageContract.RequestIdHeader).Single();
            Content = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            { Content = JsonContent.Create(new StoredReportResponse(Guid.NewGuid(), "/files/test-report/download")) };
        }
    }
}
