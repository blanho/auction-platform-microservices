using Jobs.Application.Features.Jobs.GetJobHistory;
using Jobs.Domain.Entities;
using Jobs.Domain.Enums;
using Jobs.Infrastructure.Persistence;
using Jobs.Infrastructure.Persistence.Repositories;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TestSupport;
using Xunit;

namespace Jobs.Infrastructure.Tests;

public sealed class JobPagingTests
{
    [PostgresFact]
    public async Task JobLists_ApplyFiltersBeforePagingAndUseBoundedMetadata()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await db.Database.MigrateAsync();
        var requester = Guid.NewGuid();
        var jobs = Enumerable.Range(0, 4).Select(index => Job.Create(
            index == 2 ? JobType.DataExport : JobType.AuctionImport, Guid.NewGuid().ToString(), "{}", requester, 3)).ToArray();
        var timestamp = DateTimeOffset.UtcNow.AddDays(-1);
        foreach (var job in jobs) job.SetCreatedAudit(Guid.Empty, timestamp);
        jobs[3].MarkAsDeleted(Guid.Empty, timestamp);
        db.Jobs.AddRange(jobs);
        await db.SaveChangesAsync();
        var repository = new JobRepository(db, null!, null!);

        var first = await repository.GetFilteredAsync(JobType.AuctionImport, null, requester, 1, 1);
        var second = await repository.GetFilteredAsync(JobType.AuctionImport, null, requester, 2, 1);
        Assert.Equal(2, first.TotalCount);
        Assert.Equal(jobs.Take(2).OrderBy(job => job.Id).Select(job => job.Id),
            first.Items.Concat(second.Items).Select(job => job.Id));
        var normalized = await repository.GetPagedAsync(0, 500);
        Assert.Equal(1, normalized.Page);
        Assert.Equal(100, normalized.PageSize);
        Assert.Equal(3, normalized.TotalCount);
        Assert.Empty((await repository.GetPagedAsync(int.MaxValue, 100)).Items);
    }

    [PostgresFact]
    public async Task ItemsAndHistory_ReturnFilteredTotalsAndEffectivePagination()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await db.Database.MigrateAsync();
        var job = Job.Create(JobType.AuctionImport, Guid.NewGuid().ToString(), "{}", Guid.NewGuid(), 3);
        db.Jobs.Add(job);
        var items = Enumerable.Range(1, 3).Select(sequence => JobItem.Create(job.Id, "{}", sequence, 1)).ToArray();
        items[1].MarkProcessing();
        items[1].MarkCompleted();
        db.JobItems.AddRange(items);
        var logs = new[]
        {
            JobExecutionLog.CreateInformation(job.Id, "first"), JobExecutionLog.CreateInformation(job.Id, "second"),
            JobExecutionLog.CreateWarning(job.Id, "warning")
        };
        var timestamp = DateTimeOffset.UtcNow.AddDays(-1);
        foreach (var log in logs) log.SetCreatedAudit(Guid.Empty, timestamp);
        db.Set<JobExecutionLog>().AddRange(logs);
        await db.SaveChangesAsync();
        var itemRepository = new JobItemRepository(db, null!, null!);
        var result = await itemRepository.GetPagedItemsByJobIdAsync(job.Id, JobItemStatus.Pending, 2, 1);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(3, Assert.Single(result.Items).SequenceNumber);
        var logRepository = new JobExecutionLogRepository(db);
        var handler = new GetJobHistoryQueryHandler(new JobRepository(db, null!, null!), logRepository,
            NullLogger<GetJobHistoryQueryHandler>.Instance);
        var history = await handler.Handle(new GetJobHistoryQuery(job.Id, 2, 1, JobLogLevel.Information), default);
        Assert.True(history.IsSuccess);
        Assert.NotNull(history.Value);
        Assert.Equal(2, history.Value.TotalLogEntries);
        Assert.Equal(2, history.Value.Page);
        Assert.Equal(1, history.Value.PageSize);
        Assert.Equal(2, history.Value.TotalPages);
        Assert.True(history.Value.HasPreviousPage);
        Assert.False(history.Value.HasNextPage);
        Assert.Equal(logs.Take(2).OrderBy(log => log.Id).Last().Id, Assert.Single(history.Value.Entries).Id);
    }

    private static JobDbContext Open(PostgresTestDatabase database) => new(new DbContextOptionsBuilder<JobDbContext>()
        .UseNpgsql(database.ConnectionString).Options);

    [PostgresFact]
    public async Task JobRepository_PreservesChildAuditAndDetachedItemUpdates()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await db.Database.MigrateAsync();
        var actor = Guid.NewGuid();
        var audit = TestProxy.Create<IAuditContext>((_, _) => actor);
        var repository = new JobRepository(db, new DateTimeProvider(), audit);
        var job = Job.Create(JobType.AuctionImport, Guid.NewGuid().ToString(), "{}", actor, 1);
        job.AddItem("{}", 1);

        await repository.CreateAsync(job);
        var item = Assert.Single(job.Items);
        Assert.Equal(actor, item.CreatedBy);
        Assert.Equal(job.CreatedAt, item.CreatedAt);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var detached = await repository.GetByIdWithItemsAsync(job.Id);
        Assert.NotNull(detached);
        Assert.Empty(db.ChangeTracker.Entries());
        var child = Assert.Single(detached.Items);
        child.MarkProcessing();
        child.MarkCompleted();
        await repository.UpdateAsync(detached);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.Equal(JobItemStatus.Completed, (await db.JobItems.SingleAsync()).Status);
        Assert.Equal(actor, (await db.Jobs.SingleAsync()).UpdatedBy);
    }
}
