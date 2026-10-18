using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.EntityFrameworkCore;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using Storage.Domain.Events;
using Storage.Infrastructure.Persistence;
using Storage.Infrastructure.Persistence.Repositories;
using TestSupport;
using Xunit;

namespace Storage.Application.Tests;

public sealed class SharedRepositoryTests
{
    private static readonly Guid Actor = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static IAuditContext Audit => TestProxy.Create<IAuditContext>((method, _) =>
        method.Name == "get_UserId" ? Actor : null);
    private static IDateTimeProvider Clock => TestProxy.Create<IDateTimeProvider>((_, _) => Now);
    private static StoredFile File() => StoredFile.Create("image.png", Guid.NewGuid() + ".png", "image/png", 10,
        "https://storage/image.png", "images", Actor, StorageProvider.Local);

    [PostgresFact]
    public async Task SharedWritesPreserveTrackedLookupDomainDeletionAndPhysicalCleanup()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<StorageDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using var db = new StorageDbContext(options);
        await db.Database.MigrateAsync();
        IStoredFileRepository repository = new StoredFileRepository(db, Clock, Audit);
        var file = File();
        await repository.AddAsync(file);
        Assert.Equal(Actor, file.CreatedBy);
        Assert.Equal(Now, file.CreatedAt);
        await using var observer = new StorageDbContext(options);
        Assert.Empty(await observer.StoredFiles.ToListAsync());
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var tracked = (await repository.GetByIdAsync(file.Id))!;
        Assert.Equal(EntityState.Unchanged, db.Entry(tracked).State);
        tracked.MarkAsDeleted(Actor);
        var deletedAt = tracked.DeletedAt;
        await repository.UpdateAsync(tracked);
        Assert.Equal(deletedAt, tracked.DeletedAt);
        Assert.Equal(Actor, tracked.UpdatedBy);
        Assert.Equal(Now, tracked.UpdatedAt);
        await db.SaveChangesAsync();
        Assert.Null(await repository.GetByIdAsync(file.Id));
        Assert.Empty(await observer.StoredFiles.ToListAsync());
        Assert.Equal(FileStatus.Deleted, (await observer.StoredFiles.IgnoreQueryFilters().AsNoTracking().SingleAsync()).Status);
        var expired = await repository.GetSoftDeletedOlderThanAsync(Now.AddDays(1), 10);
        repository.RemoveRange(expired);
        await db.SaveChangesAsync();
        Assert.Empty(await observer.StoredFiles.IgnoreQueryFilters().ToListAsync());
    }

    [PostgresFact]
    public async Task CommonBatchDeletionRunsFileDomainPolicyOnceAndUsesSharedAuditTimestamp()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<StorageDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using var db = new StorageDbContext(options);
        await db.Database.MigrateAsync();
        var repository = new StoredFileRepository(db, Clock, Audit);
        var files = new[] { File(), File() };
        await repository.AddRangeAsync(files);
        await db.SaveChangesAsync();
        foreach (var file in files) file.ClearDomainEvents();
        await repository.DeleteAsync(files[0].Id);
        await repository.DeleteRangeAsync(files.Select(file => file.Id));
        await repository.DeleteRangeAsync(files.Select(file => file.Id));
        Assert.All(files, file =>
        {
            Assert.Equal(FileStatus.Deleted, file.Status);
            Assert.True(file.IsDeleted);
            Assert.Equal(Actor, file.DeletedBy);
            Assert.Equal(Now, file.DeletedAt);
            Assert.Single(file.DomainEvents.OfType<FileDeletedDomainEvent>());
        });
        await using var observer = new StorageDbContext(options);
        Assert.Equal(2, await observer.StoredFiles.CountAsync());
        await db.SaveChangesAsync();
        Assert.Empty(await observer.StoredFiles.ToListAsync());
        Assert.Equal(2, await observer.StoredFiles.IgnoreQueryFilters().CountAsync());
    }
}
