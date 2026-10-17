using AuctionService.Contracts.Commands;
using Jobs.Domain.Entities;
using Jobs.Domain.Enums;
using Jobs.Infrastructure.Messaging.Consumers;
using Jobs.Infrastructure.Persistence;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Contracts.Commands;
using PaymentService.Contracts.Commands;
using TestSupport;
using Xunit;

namespace Jobs.Infrastructure.Tests;

public class JobItemRoutingTests
{
    [PostgresFact]
    public async Task MalformedServiceJobPayloads_FailThePersistedItemWithoutDispatch()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<JobDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using var db = new JobDbContext(options);
        await db.Database.MigrateAsync();
        foreach (var type in new[] { JobType.ReportGeneration, JobType.BulkNotification })
        foreach (var payload in new[] { "null", "[]", "{\"attempt\":\"invalid\"}" })
        {
            var job = Job.Create(type, Guid.NewGuid().ToString(), "{}", Guid.NewGuid(), 1);
            var item = job.AddItem(payload, 1);
            job.Start();
            item.MarkProcessing();
            db.Jobs.Add(job);
            await db.SaveChangesAsync();
            var published = new List<object>();
            var command = new ProcessJobItemCommand { JobId = job.Id, JobItemId = item.Id };
            var context = TestProxy.Create<ConsumeContext<ProcessJobItemCommand>>((method, args) => method.Name switch
            {
                "get_Message" => command,
                "get_CancellationToken" => CancellationToken.None,
                "Publish" => Capture(args![0]!),
                _ => throw new NotSupportedException(method.Name)
            });
            await new ProcessJobItemConsumer(db).Consume(context);
            var failure = Assert.IsType<ReportJobItemResultCommand>(Assert.Single(published));
            Assert.Equal(job.Id, failure.JobId);
            Assert.Equal(item.Id, failure.JobItemId);
            Assert.False(failure.IsSuccess);
            Assert.True(failure.IsFinalFailure);

            Task Capture(object message) { published.Add(message); return Task.CompletedTask; }
        }
    }

    [PostgresFact]
    public async Task RoutesSupportedItemsUsingPersistedIdentityAndIgnoresStaleAttempts()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<JobDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using var db = new JobDbContext(options);
        await db.Database.MigrateAsync();
        foreach (var type in new[] { JobType.AuctionImport, JobType.BulkAuctionUpdate, JobType.DataExport,
                     JobType.ReportGeneration, JobType.BulkNotification })
        {
            var owner = Guid.NewGuid();
            var job = Job.Create(type, Guid.NewGuid().ToString(), "{}", owner, 1);
            var item = job.AddItem(System.Text.Json.JsonSerializer.Serialize(new
            {
                RequestedBy = Guid.NewGuid(), SellerId = Guid.NewGuid(),
                ParentJobId = Guid.NewGuid(), ParentJobItemId = Guid.NewGuid(), Attempt = 99
            }), 1);
            job.Start();
            item.MarkProcessing();
            db.Jobs.Add(job);
            await db.SaveChangesAsync();
            var published = new List<object>();
            var command = new ProcessJobItemCommand
            {
                JobId = job.Id,
                JobItemId = item.Id,
                Attempt = 0,
                JobType = "ImageProcessing",
                PayloadJson = "invalid JSON"
            };
            var consumer = new ProcessJobItemConsumer(db);
            await consumer.Consume(Context(command));
            var result = Assert.Single(published);
            switch (result)
            {
                case ProcessAuctionImportCommand import:
                    Assert.Equal(owner, import.SellerId);
                    Assert.Equal(item.Id, import.ParentJobItemId);
                    break;
                case ProcessBulkAuctionUpdateCommand update:
                    Assert.Equal(owner, update.RequestedBy);
                    Assert.Equal(item.Id, update.ParentJobItemId);
                    break;
                case ProcessAuctionExportCommand export:
                    Assert.Equal(owner, export.RequestedBy);
                    Assert.Equal(item.Id, export.ParentJobItemId);
                    break;
                case GenerateOrderReportCommand report:
                    Assert.Equal(owner, report.RequestedBy);
                    Assert.Equal(job.Id, report.ParentJobId);
                    Assert.Equal(item.Id, report.ParentJobItemId);
                    Assert.Equal(0, report.Attempt);
                    Assert.Equal(job.CreatedAt, report.RequestedAt);
                    Assert.NotEqual(Guid.Empty, report.CorrelationId);
                    break;
                case SendBulkNotificationCommand notification:
                    Assert.Equal(owner, notification.RequestedBy);
                    Assert.Equal(job.Id, notification.ParentJobId);
                    Assert.Equal(item.Id, notification.ParentJobItemId);
                    Assert.Equal(0, notification.Attempt);
                    Assert.NotEqual(Guid.Empty, notification.CorrelationId);
                    break;
                default: Assert.Fail("Unexpected routed command"); break;
            }
            await consumer.Consume(Context(command with { Attempt = 1 }));
            Assert.Single(published);

            ConsumeContext<ProcessJobItemCommand> Context(ProcessJobItemCommand message) =>
                TestProxy.Create<ConsumeContext<ProcessJobItemCommand>>((method, args) => method.Name switch
                {
                    "get_Message" => message,
                    "get_CancellationToken" => CancellationToken.None,
                    "Publish" => Capture(args![0]!),
                    _ => throw new NotSupportedException(method.Name)
                });
            Task Capture(object message) { published.Add(message); return Task.CompletedTask; }
        }
    }
}
