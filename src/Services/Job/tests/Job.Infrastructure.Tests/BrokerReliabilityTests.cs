using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Jobs.Application.Interfaces;
using Jobs.Domain.Enums;
using Jobs.Infrastructure.Extensions;
using Jobs.Infrastructure.Messaging;
using Jobs.Infrastructure.Persistence;
using Jobs.Infrastructure.Persistence.Repositories;
using JobService.Contracts.Commands;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using TestSupport;
using Xunit;

namespace Jobs.Infrastructure.Tests;

public class BrokerReliabilityTests
{
    [BrokerFact]
    public async Task ProductionEndpointsPreserveEarlyAndConcurrentProgressAndFailUnsupportedItems()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<JobDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using (var seed = new JobDbContext(options)) await seed.Database.MigrateAsync();
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddDbContext<JobDbContext>(o => o.UseNpgsql(database.ConnectionString));
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton(TestProxy.Create<IAuditContext>((method, _) => method.Name == "get_UserId"
            ? Guid.Empty : throw new NotSupportedException(method.Name)));
        services.AddSingleton(TestProxy.Create<IMediator>((_, _) => Task.CompletedTask));
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobItemRepository, JobItemRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJobItemDispatcher, JobItemDispatcher>();
        services.AddMassTransitWithOutbox(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["RabbitMQ:Host"] = "localhost",
            ["RabbitMQ:Port"] = Environment.GetEnvironmentVariable("BACKEND_TEST_RABBIT_PORT"),
            ["RabbitMQ:Username"] = "guest",
            ["RabbitMQ:Password"] = "guest"
        }).Build());
        await using var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<IBusControl>();
        using var lifetime = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var hosted = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>().ToList();
        foreach (var service in hosted) await service.StartAsync(lifetime.Token);
        await bus.StartAsync(lifetime.Token);
        try
        {
            var correlation = Guid.NewGuid().ToString();
            await bus.Publish(new ReportJobBatchProgressCommand
            { CorrelationId = correlation, BatchId = "early", CompletedCount = 2 });
            await Until(async () =>
            {
                await using var db = new JobDbContext(options);
                return await db.ProgressEntries.AnyAsync(x => x.CorrelationId == correlation);
            });
            await bus.Publish(new RequestJobCommand
            { CorrelationId = correlation, JobType = "AuctionImport", RequestedBy = Guid.NewGuid(), TotalItems = 12, PayloadJson = "{}" });
            await Task.WhenAll(Enumerable.Range(0, 10).Select(i => bus.Publish(new ReportJobBatchProgressCommand
            { CorrelationId = correlation, BatchId = $"batch-{i}", CompletedCount = 1 })));
            await bus.Publish(new ReportJobBatchProgressCommand
            { CorrelationId = correlation, BatchId = "early", CompletedCount = 2 });
            await Until(async () =>
            {
                await using var db = new JobDbContext(options);
                return await db.Jobs.AnyAsync(x => x.CorrelationId == correlation && x.Status == JobStatus.Completed && x.CompletedItems == 12);
            });

            var streaming = Jobs.Domain.Entities.Job.CreateStreaming(JobType.DataExport,
                Guid.NewGuid().ToString(), "{}", Guid.NewGuid());
            await using (var seed = new JobDbContext(options))
            {
                seed.Jobs.Add(streaming);
                await seed.SaveChangesAsync();
            }
            var batch = new AddJobItemsBatchCommand
            {
                JobId = streaming.Id,
                Items = [new RequestJobItemPayload { SequenceNumber = 1, PayloadJson = "{}" }]
            };
            await Task.WhenAll(bus.Publish(batch), bus.Publish(batch), bus.Publish(batch with
            {
                Items = [new RequestJobItemPayload { SequenceNumber = 2, PayloadJson = "{}" }]
            }));
            await Until(async () =>
            {
                await using var db = new JobDbContext(options);
                return await db.Jobs.AnyAsync(x => x.Id == streaming.Id && x.TotalItems == 2);
            });
            await bus.Publish(new FinalizeJobInitializationCommand { JobId = streaming.Id, ExpectedTotalItems = 2 });
            await Until(async () =>
            {
                await using var db = new JobDbContext(options);
                return await db.Jobs.AnyAsync(x => x.Id == streaming.Id && x.Status == JobStatus.Pending);
            });
            await using (var check = new JobDbContext(options))
                Assert.Equal(2, await check.JobItems.CountAsync(x => x.JobId == streaming.Id));

            var itemCorrelation = Guid.NewGuid().ToString();
            await bus.Publish(new RequestJobCommand
            {
                CorrelationId = itemCorrelation,
                JobType = "ImageProcessing",
                RequestedBy = Guid.NewGuid(),
                PayloadJson = "{}",
                Items = [new RequestJobItemPayload { PayloadJson = "{}", SequenceNumber = 1 }]
            });
            await Until(async () =>
            {
                await using var db = new JobDbContext(options);
                return await db.Jobs.AnyAsync(x => x.CorrelationId == itemCorrelation);
            });
            using (var scope = provider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<JobDbContext>();
                var job = await db.Jobs.SingleAsync(x => x.CorrelationId == itemCorrelation);
                job.Start();
                await db.SaveChangesAsync();
                await scope.ServiceProvider.GetRequiredService<IJobItemDispatcher>().DispatchItemsAsync(job.Id);
            }
            try
            {
                await Until(async () =>
                {
                    await using var db = new JobDbContext(options);
                    return await db.Jobs.AnyAsync(x => x.CorrelationId == itemCorrelation && x.Status == JobStatus.Failed);
                });
            }
            catch (TaskCanceledException)
            {
                await using var db = new JobDbContext(options);
                var job = await db.Jobs.Include(x => x.Items).SingleAsync(x => x.CorrelationId == itemCorrelation);
                Assert.Fail($"Job={job.Status}; item={job.Items.Single().Status}; inbox={await db.Set<InboxState>().CountAsync()}; outbox={await db.Set<OutboxMessage>().CountAsync()}");
            }
        }
        finally
        {
            using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            foreach (var service in hosted.AsEnumerable().Reverse()) await service.StopAsync(stop.Token);
            await bus.StopAsync(stop.Token);
        }
    }

    private static async Task Until(Func<Task<bool>> predicate)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        while (!await predicate()) await Task.Delay(100, timeout.Token);
    }
}
