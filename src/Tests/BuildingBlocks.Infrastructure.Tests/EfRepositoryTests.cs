using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using TestSupport;
using Xunit;

namespace BuildingBlocks.Infrastructure.Tests;

public sealed class EfRepositoryTests
{
    private static readonly Guid Actor = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 0, 0, 0, TimeSpan.Zero);

    [PostgresFact]
    public async Task Writes_AreStagedUntilCommitAndShareTheCallersTransaction()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = await OpenAsync(database);
        var repository = new TestRepository(db);
        var first = TestAggregate.Create("first");
        var second = TestAggregate.Create("second");

        Assert.Same(first, await repository.AddAsync(first));
        await repository.CreateAsync(second);
        await using var observer = await OpenAsync(database);
        Assert.Equal(0, await observer.Aggregates.CountAsync());
        Assert.Equal(Actor, first.CreatedBy);
        Assert.Equal(Now, first.CreatedAt);
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            await db.SaveChangesAsync();
            Assert.Equal(0, await observer.Aggregates.CountAsync());
            await transaction.RollbackAsync();
        }
        Assert.Equal(0, await observer.Aggregates.CountAsync());
    }

    [PostgresFact]
    public async Task Reads_ExcludeDeletedRowsAndWriteLookupTracksEvenWithNoTrackingDefault()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = await OpenAsync(database);
        var active = TestAggregate.Create("active");
        var deleted = TestAggregate.Create("deleted");
        deleted.MarkAsDeleted(Actor, Now);
        db.AddRange(active, deleted);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        var repository = new TestRepository(db);

        Assert.NotNull(await repository.GetByIdAsync(active.Id));
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.Null(await repository.GetByIdAsync(deleted.Id));
        Assert.Null(await repository.GetByIdForUpdateAsync(deleted.Id));
        Assert.False(await repository.ExistsAsync(deleted.Id));
        var tracked = await repository.GetByIdForUpdateAsync(active.Id);
        Assert.NotNull(tracked);
        tracked.Rename("updated");
        await repository.UpdateAsync(tracked);
        Assert.Equal(Actor, tracked.UpdatedBy);
        Assert.Equal(Now, tracked.UpdatedAt);
        await db.SaveChangesAsync();
        await using var observer = await OpenAsync(database);
        Assert.Equal("updated", (await observer.Aggregates.SingleAsync(row => row.Id == active.Id)).Name);
    }

    [PostgresFact]
    public async Task SoftDelete_IsIdempotentAndNeverPhysicallyRemovesRecords()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = await OpenAsync(database);
        var entity = TestAggregate.Create("delete");
        db.Add(entity);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = new TestRepository(db);

        await repository.DeleteAsync(entity.Id);
        await using var observer = await OpenAsync(database);
        Assert.False((await observer.Aggregates.SingleAsync()).IsDeleted);
        await db.SaveChangesAsync();
        var persisted = await observer.Aggregates.AsNoTracking().SingleAsync();
        Assert.True(persisted.IsDeleted);
        Assert.Equal(Actor, persisted.DeletedBy);
        Assert.Equal(Now, persisted.DeletedAt);
        Assert.Null(await repository.GetByIdAsync(entity.Id));
        await repository.DeleteAsync(entity.Id);
        await repository.DeleteAsync(Guid.NewGuid());
        Assert.Equal(0, await db.SaveChangesAsync());
    }

    [PostgresFact]
    public async Task BatchOperations_EnumerateInputsOnceAndPreserveAuditAndSoftDeletion()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = await OpenAsync(database);
        var repository = new TestRepository(db);
        var first = TestAggregate.Create("first");
        var second = TestAggregate.Create("second");
        var enumerationCount = 0;
        IEnumerable<TestAggregate> Batch()
        {
            if (++enumerationCount > 1) throw new InvalidOperationException("Input enumerated twice");
            yield return first;
            yield return second;
        }

        await repository.AddRangeAsync(Batch());
        Assert.Equal(1, enumerationCount);
        Assert.Equal(first.CreatedAt, second.CreatedAt);
        Assert.Equal(Actor, second.CreatedBy);
        first.Rename("changed");
        await repository.UpdateRangeAsync([first, second]);
        Assert.Equal(EntityState.Added, db.Entry(first).State);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await repository.DeleteRangeAsync([first.Id, first.Id, Guid.NewGuid()]);
        await db.SaveChangesAsync();
        Assert.Equal(2, await db.Aggregates.CountAsync());
        Assert.True((await db.Aggregates.SingleAsync(row => row.Id == first.Id)).IsDeleted);
        Assert.False((await db.Aggregates.SingleAsync(row => row.Id == second.Id)).IsDeleted);
        Assert.Equal("changed", (await db.Aggregates.SingleAsync(row => row.Id == first.Id)).Name);
    }

    [PostgresFact]
    public async Task CustomVisibility_AppliesToSingleAndBatchDeletes()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = await OpenAsync(database);
        var own = TestAggregate.Create("own", Actor);
        var other = TestAggregate.Create("other", Guid.NewGuid());
        db.AddRange(own, other);
        await db.SaveChangesAsync();
        var repository = new ScopedRepository(db);

        Assert.Null(await repository.GetByIdAsync(other.Id));
        await repository.DeleteAsync(other.Id);
        await repository.DeleteRangeAsync([own.Id, other.Id]);
        await db.SaveChangesAsync();
        Assert.False((await db.Aggregates.SingleAsync(row => row.Id == other.Id)).IsDeleted);
        Assert.True((await db.Aggregates.SingleAsync(row => row.Id == own.Id)).IsDeleted);
    }

    [PostgresFact]
    public async Task ConcurrentUpdates_RetainOptimisticConcurrencyChecks()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using (var seed = await OpenAsync(database))
        {
            seed.Add(TestAggregate.Create("original"));
            await seed.SaveChangesAsync();
        }
        await using var firstDb = await OpenAsync(database);
        await using var secondDb = await OpenAsync(database);
        var firstRepository = new TestRepository(firstDb);
        var secondRepository = new TestRepository(secondDb);
        var id = await firstDb.Aggregates.Select(row => row.Id).SingleAsync();
        var first = await firstRepository.GetByIdForUpdateAsync(id);
        var second = await secondRepository.GetByIdForUpdateAsync(id);
        Assert.NotNull(first);
        Assert.NotNull(second);
        first.Rename("first update");
        second.Rename("stale update");
        await firstRepository.UpdateAsync(first);
        await secondRepository.UpdateAsync(second);
        await firstDb.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondDb.SaveChangesAsync());
    }

    [PostgresFact]
    public async Task DetachedUpdate_DoesNotOverwriteRelatedEntities()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = await OpenAsync(database);
        var entity = TestAggregate.Create("root");
        entity.Children.Add(new TestChild { Name = "original child" });
        db.Add(entity);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var detached = await db.Aggregates.AsNoTracking().Include(row => row.Children).SingleAsync();
        detached.Rename("updated root");
        detached.Children.Single().Name = "stale child change";

        await new TestRepository(db).UpdateAsync(detached);
        await db.SaveChangesAsync();

        await using var observer = await OpenAsync(database);
        var persisted = await observer.Aggregates.Include(row => row.Children).SingleAsync();
        Assert.Equal("updated root", persisted.Name);
        Assert.Equal("original child", Assert.Single(persisted.Children).Name);
    }

    [PostgresFact]
    public async Task Paging_UsesSharedBoundsStableOrderingAndFilteredCounts()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = await OpenAsync(database);
        var repository = new TestRepository(db);
        await repository.AddRangeAsync([TestAggregate.Create("one"), TestAggregate.Create("two"), TestAggregate.Create("three")]);
        await db.SaveChangesAsync();
        var ids = await db.Aggregates.OrderBy(row => row.Id).Select(row => row.Id).ToListAsync();
        db.ChangeTracker.Clear();
        var first = await repository.GetPagedAsync(1, 1);
        var second = await repository.GetPagedAsync(2, 1);
        Assert.Equal(ids.Take(2), first.Items.Concat(second.Items).Select(row => row.Id));
        Assert.Equal(3, first.TotalCount);
        var normalized = await repository.GetPagedAsync(0, 500);
        Assert.Equal(1, normalized.Page);
        Assert.Equal(100, normalized.PageSize);
        Assert.Empty((await repository.GetPagedAsync(int.MaxValue, 100)).Items);
        Assert.Empty(db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task CancelledWrites_DoNotMutateEntitiesOrTrackingState()
    {
        await using var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql("Host=localhost;Database=repository_state_only").Options);
        var repository = new TestRepository(db);
        var entity = TestAggregate.Create("cancelled");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.CreateAsync(entity, cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.UpdateAsync(entity, cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.DeleteAsync(entity, cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.AddRangeAsync([entity], cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.UpdateRangeAsync([entity], cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.DeleteRangeAsync([entity.Id], cancellation.Token));
        Assert.False(entity.IsDeleted);
        Assert.Null(entity.UpdatedAt);
        Assert.Equal(default, entity.CreatedAt);
        Assert.Empty(db.ChangeTracker.Entries());
    }

    private static async Task<TestDbContext> OpenAsync(PostgresTestDatabase database)
    {
        var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().UseNpgsql(database.ConnectionString).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class TestRepository(TestDbContext db) : EfRepository<TestAggregate, TestDbContext>(db, new FixedClock(), Audit())
    {
    }

    private sealed class ScopedRepository(TestDbContext db) : EfRepository<TestAggregate, TestDbContext>(db, new FixedClock(), Audit())
    {
        protected override IQueryable<TestAggregate> Query => base.Query.Where(entity => entity.OwnerId == Actor);
    }

    private static IAuditContext Audit() => TestProxy.Create<IAuditContext>((method, _) =>
        method.Name == "get_UserId" ? Actor : throw new NotSupportedException(method.Name));

    private sealed class FixedClock : IDateTimeProvider
    {
        public DateTime UtcNow => EfRepositoryTests.Now.UtcDateTime;
        public DateTime Now => EfRepositoryTests.Now.UtcDateTime;
        public DateTimeOffset UtcNowOffset => EfRepositoryTests.Now;
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAggregate>().Property(entity => entity.Version).IsRowVersion();
            modelBuilder.Entity<TestAggregate>().HasMany(entity => entity.Children).WithOne()
                .HasForeignKey(child => child.AggregateId);
        }
    }

    private sealed class TestAggregate : AggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public Guid OwnerId { get; private set; }
        public List<TestChild> Children { get; private set; } = [];
        public static TestAggregate Create(string name, Guid ownerId = default) => new() { Name = name, OwnerId = ownerId };
        public void Rename(string name) => Name = name;
    }

    private sealed class TestChild
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AggregateId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
