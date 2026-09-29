using System.Data;
using BuildingBlocks.Application.Abstractions;
using Jobs.Domain.Entities;
using Jobs.Domain.Enums;
using Jobs.Infrastructure.Messaging;
using Jobs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TestSupport;
using Xunit;

namespace Jobs.Infrastructure.Tests;

public class JobProgressPersistenceTests
{
    [PostgresFact]
    public async Task EarlyProgressSurvivesRestartAndDuplicateDelivery()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var correlation = Guid.NewGuid().ToString();
        await using (var db = Open(database))
        {
            await db.Database.MigrateAsync();
            await Store(db).RecordAsync(Entry(correlation, "batch-2", 2, 1), default);
        }
        await using (var db = Open(database))
        {
            var job = Job.Create(JobType.AuctionImport, correlation, "{}", Guid.NewGuid(), 5);
            db.Jobs.Add(job);
            await db.SaveChangesAsync();
            await Store(db).ApplyAsync(job, default);
            await Store(db).RecordAsync(Entry(correlation, "batch-2", 2, 1), default);
            await Store(db).RecordAsync(Entry(correlation, "batch-1", 2, 0), default);
        }
        await using (var db = Open(database))
        {
            var job = await db.Jobs.SingleAsync();
            Assert.Equal(4, job.CompletedItems);
            Assert.Equal(1, job.FailedItems);
            Assert.Equal(JobStatus.CompletedWithErrors, job.Status);
            Assert.Equal(2, await db.ProgressEntries.CountAsync());
        }
    }

    [PostgresFact]
    public async Task RolledBackProgressCanBeRetriedWithoutLosingCounts()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var correlation = Guid.NewGuid().ToString();
        await using (var db = Open(database))
        {
            await db.Database.MigrateAsync();
            db.Jobs.Add(Job.Create(JobType.AuctionImport, correlation, "{}", Guid.NewGuid(), 3));
            await db.SaveChangesAsync();
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            await Store(db).RecordAsync(Entry(correlation, "first", 1, 0), default);
            await transaction.RollbackAsync();
        }
        await using (var db = Open(database))
        {
            Assert.Empty(await db.ProgressEntries.ToListAsync());
            await Store(db).RecordAsync(Entry(correlation, "first", 1, 0), default);
            Assert.Equal(1, (await db.Jobs.SingleAsync()).CompletedItems);
        }
    }

    [PostgresFact]
    public async Task ConcurrentBatchesPreserveBothUpdatesWithTransactionRetry()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var correlation = Guid.NewGuid().ToString();
        await using (var db = Open(database))
        {
            await db.Database.MigrateAsync();
            db.Jobs.Add(Job.Create(JobType.AuctionImport, correlation, "{}", Guid.NewGuid(), 2));
            await db.SaveChangesAsync();
        }
        await Task.WhenAll(Apply("one"), Apply("two"));
        await using var check = Open(database);
        var job = await check.Jobs.SingleAsync();
        Assert.Equal(2, job.CompletedItems);
        Assert.Equal(JobStatus.Completed, job.Status);

        async Task Apply(string batch)
        {
            for (var attempt = 0; ; attempt++)
            {
                await using var db = Open(database);
                await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                try
                {
                    await Store(db).RecordAsync(Entry(correlation, batch, 1, 0), default);
                    await transaction.CommitAsync();
                    return;
                }
                catch (Exception ex) when (attempt < 5 && IsRetryable(ex)) { }
            }
        }
    }

    [PostgresFact]
    public async Task FailureBeforeCreationIsAppliedAfterCreation()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await db.Database.MigrateAsync();
        var correlation = Guid.NewGuid().ToString();
        await Store(db).RecordAsync(new JobProgressEntry
        { CorrelationId = correlation, BatchId = "failure", ErrorMessage = "worker failed" }, default);
        var job = Job.Create(JobType.AuctionImport, correlation, "{}", Guid.NewGuid(), 1);
        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        await Store(db).ApplyAsync(job, default);
        Assert.Equal(JobStatus.Failed, job.Status);
        Assert.Equal("worker failed", job.ErrorMessage);
    }

    private static bool IsRetryable(Exception ex) => ex is DbUpdateConcurrencyException ||
        ex is PostgresException { SqlState: "40001" } || ex.InnerException is not null && IsRetryable(ex.InnerException);
    private static JobProgressEntry Entry(string correlation, string batch, int completed, int failed) => new()
    { CorrelationId = correlation, BatchId = batch, CompletedCount = completed, FailedCount = failed };
    private static JobDbContext Open(PostgresTestDatabase database) =>
        new(new DbContextOptionsBuilder<JobDbContext>().UseNpgsql(database.ConnectionString).Options);
    private static JobProgressStore Store(JobDbContext db) => new(db,
        TestProxy.Create<IUnitOfWork>((method, args) => method.Name == nameof(IUnitOfWork.SaveChangesAsync)
            ? db.SaveChangesAsync((CancellationToken)args![0]!) : throw new NotSupportedException(method.Name)));
}
