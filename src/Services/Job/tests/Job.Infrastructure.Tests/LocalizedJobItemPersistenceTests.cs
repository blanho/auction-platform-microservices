using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Localization;
using Jobs.Application.DTOs;
using Jobs.Application.Interfaces;
using Jobs.Application.Mappings;
using Jobs.Domain.Entities;
using Jobs.Domain.Enums;
using Jobs.Infrastructure.Messaging.Consumers;
using Jobs.Infrastructure.Persistence;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging.Abstractions;
using TestSupport;
using Xunit;

namespace Jobs.Infrastructure.Tests;

public class LocalizedJobItemPersistenceTests
{
    private const string Code = "Notification.Bulk.BatchSizeOutOfRange";

    [PostgresFact]
    public async Task SingleAndBatchResultsPersistMessageMetadataAcrossRestart()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        Guid jobId;
        await using (var db = Open(database))
        {
            await db.Database.MigrateAsync();
            var job = Job.Create(JobType.BulkNotification, Guid.NewGuid().ToString(), "{}", Guid.NewGuid(), 2);
            var first = job.AddItem("{}", 1);
            var second = job.AddItem("{}", 2);
            job.Start(); first.MarkProcessing(); second.MarkProcessing();
            db.Jobs.Add(job);
            await db.SaveChangesAsync();
            jobId = job.Id;
            var jobs = TestProxy.Create<IJobRepository>((method, _) => method.Name switch
            {
                nameof(IJobRepository.GetByIdForUpdateAsync) => Task.FromResult<Job?>(job),
                nameof(IJobRepository.UpdateAsync) => Task.CompletedTask,
                _ => throw new NotSupportedException(method.Name)
            });
            var items = TestProxy.Create<IJobItemRepository>((method, _) => method.Name switch
            {
                nameof(IJobItemRepository.GetByIdForUpdateAsync) => Task.FromResult<JobItem?>(first),
                nameof(IJobItemRepository.GetByIdsForUpdateAsync) => Task.FromResult(new List<JobItem> { second }),
                nameof(IJobItemRepository.UpdateAsync) => Task.CompletedTask,
                _ => throw new NotSupportedException(method.Name)
            });
            var uow = TestProxy.Create<IUnitOfWork>((method, _) => method.Name == nameof(IUnitOfWork.SaveChangesAsync)
                ? db.SaveChangesAsync() : throw new NotSupportedException(method.Name));
            var single = new ReportJobItemResultConsumer(jobs, items, uow, NullLogger<ReportJobItemResultConsumer>.Instance);
            var result = new ReportJobItemResultCommand
            {
                JobId = job.Id, JobItemId = first.Id, IsSuccess = false, IsFinalFailure = true,
                ErrorMessage = "Original single-result diagnostic", ErrorCode = Code, ErrorArguments = ["10", "500"]
            };
            await single.Consume(Context(result));
            await single.Consume(Context(result with { ErrorCode = "stale", ErrorArguments = ["stale"] }));
            await new ReportJobItemBatchResultConsumer(jobs, items, uow, NullLogger<ReportJobItemBatchResultConsumer>.Instance)
                .Consume(Context(new ReportJobItemBatchResultCommand
                {
                    JobId = job.Id,
                    Results = [new JobItemBatchResult
                    {
                        JobItemId = second.Id, IsSuccess = false, IsFinalFailure = true,
                        ErrorMessage = "Original batch-result diagnostic", ErrorCode = Code, ErrorArguments = ["10", "500"]
                    }]
                }));
        }
        await using (var db = Open(database))
        {
            var items = await db.JobItems.Where(item => item.JobId == jobId).OrderBy(item => item.SequenceNumber).ToListAsync();
            Assert.Equal(2, items.Count);
            var mapper = Mapper();
            using var culture = new RequestCultureScope("ja-JP");
            Assert.All(items, item =>
            {
                Assert.Equal(Code, item.ErrorCode);
                Assert.Equal(new[] { "10", "500" }, item.ErrorArguments);
                Assert.Equal("バッチサイズは10から500の間である必要があります。", mapper.Map<JobItemDto>(item).ErrorMessage);
            });
            Assert.Equal("Original single-result diagnostic", items[0].ErrorMessage);
            Assert.Equal("Original batch-result diagnostic", items[1].ErrorMessage);
            items[0].ResetForRetry();
            await db.SaveChangesAsync();
        }
        await using (var db = Open(database))
        {
            var reset = await db.JobItems.SingleAsync(item => item.JobId == jobId && item.SequenceNumber == 1);
            Assert.Null(reset.ErrorCode);
            Assert.Empty(reset.ErrorArguments);
            Assert.Null(reset.ErrorMessage);
        }
    }

    [PostgresFact]
    public async Task MigrationPreservesLegacyErrorsWithoutInventingMessageKeys()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        Guid itemId = Guid.NewGuid();
        await using (var db = Open(database))
        {
            await db.GetService<IMigrator>().MigrateAsync("20260927111904_DurableJobProgress");
            var job = Job.Create(JobType.BulkNotification, Guid.NewGuid().ToString(), "{}", Guid.NewGuid(), 1);
            db.Jobs.Add(job);
            await db.SaveChangesAsync();
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "JobItems" ("Id", "JobId", "SequenceNumber", "PayloadJson", "Status", "RetryCount",
                    "MaxRetryCount", "ErrorMessage", "CreatedAt", "CreatedBy", "UpdatedBy", "DeletedBy")
                VALUES ({itemId}, {job.Id}, 1, CAST({"{}"} AS jsonb), {(int)JobItemStatus.Failed}, 1, 3,
                    {"The uploaded file is incomplete."}, {DateTimeOffset.UtcNow}, {Guid.Empty}, {Guid.Empty}, {Guid.Empty})
                """);
            await db.Database.MigrateAsync();
        }
        await using var read = Open(database);
        var item = await read.JobItems.SingleAsync(row => row.Id == itemId);
        Assert.Null(item.ErrorCode);
        Assert.Empty(item.ErrorArguments);
        Assert.Equal("The uploaded file is incomplete.", item.ErrorMessage);
        using var culture = new RequestCultureScope("ja-JP");
        Assert.Equal("ファイルのアップロードが完了していません。", Mapper().Map<JobItemDto>(item).ErrorMessage);
    }

    private static JobDbContext Open(PostgresTestDatabase database) =>
        new(new DbContextOptionsBuilder<JobDbContext>().UseNpgsql(database.ConnectionString).Options);
    private static IMapper Mapper() => new MapperConfiguration(config => config.AddProfile<JobMappingProfile>(),
        NullLoggerFactory.Instance).CreateMapper();
    private static ConsumeContext<T> Context<T>(T message) where T : class =>
        TestProxy.Create<ConsumeContext<T>>((method, _) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => CancellationToken.None,
            _ => throw new NotSupportedException(method.Name)
        });
}
