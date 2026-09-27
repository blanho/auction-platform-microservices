using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Catalog.Infrastructure.Tests;

public class CategoryRepositoryTrackingTests
{
    [Fact]
    public async Task UpdateAsync_DoesNotMarkLoadedChildrenModified()
    {
        using var context = CreateContext();
        var (parent, child) = CreateDetachedCategoryWithChild(context);
        var repository = new CategoryRepository(context, new DateTimeProvider(), new TestAuditContext());

        await repository.UpdateAsync(parent);

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.Equal(EntityState.Detached, context.Entry(child).State);
    }

    [Fact]
    public async Task DeleteAsync_DoesNotMarkLoadedChildrenModified()
    {
        using var context = CreateContext();
        var (parent, child) = CreateDetachedCategoryWithChild(context);
        var repository = new CategoryRepository(context, new DateTimeProvider(), new TestAuditContext());

        await repository.DeleteAsync(parent);

        Assert.True(parent.IsDeleted);
        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.Equal(EntityState.Detached, context.Entry(child).State);
    }

    private static CatalogDbContext CreateContext() => new(
        new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql("Host=localhost;Database=tracking_only;Username=test;Password=test")
            .Options);

    private static (Category Parent, Category Child) CreateDetachedCategoryWithChild(CatalogDbContext context)
    {
        var parent = Category.Create("Parent", "parent");
        var child = Category.Create("Child", "child", parentCategoryId: parent.Id);
        context.Categories.AddRange(parent, child);
        Assert.Single(parent.SubCategories);
        context.ChangeTracker.Clear();
        return (parent, child);
    }

    private sealed class TestAuditContext : IAuditContext
    {
        public Guid UserId => Guid.Empty;
        public string? Username => null;
        public string? CorrelationId => null;
        public string? IpAddress => null;
    }
}
