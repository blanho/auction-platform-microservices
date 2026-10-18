using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TestSupport;
using Xunit;

namespace Catalog.Infrastructure.Tests;

public sealed class RepositoryQueryTests
{
    [PostgresFact]
    public async Task SharedLookupsExcludeDeletedRowsWhileSlugUniquenessStillReservesDeletedSlugs()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(database.ConnectionString);
        dataSourceBuilder.EnableDynamicJson();
        await using var dataSource = dataSourceBuilder.Build();
        await using var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(dataSource).Options);
        await db.Database.MigrateAsync();
        var parent = Category.Create("parent", "parent");
        var child = Category.Create("child", "child", parentCategoryId: parent.Id);
        var deletedChild = Category.Create("deleted child", "deleted-child", parentCategoryId: parent.Id);
        var brand = Brand.Create("brand", "brand");
        var media = MediaFile.Create(Guid.NewGuid(), "video", 3, true);
        brand.AddFile(media);
        parent.AddFile(media);
        var deletedBrand = Brand.Create("deleted brand", "deleted-brand");
        deletedChild.MarkAsDeleted(Guid.Empty, DateTimeOffset.UtcNow);
        deletedBrand.MarkAsDeleted(Guid.Empty, DateTimeOffset.UtcNow);
        db.AddRange(parent, child, deletedChild, brand, deletedBrand);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var categories = new CategoryRepository(db, null!, null!);
        var brands = new BrandRepository(db, null!, null!);
        var loaded = (await categories.GetBySlugAsync("parent"))!;
        Assert.Equal(child.Id, Assert.Single(loaded.SubCategories).Id);
        Assert.Equal(media.FileId, Assert.Single(loaded.Files).FileId);
        Assert.True(await categories.HasChildrenAsync(parent.Id));
        Assert.False(await categories.HasChildrenAsync(child.Id));
        Assert.Equal(parent.Id, Assert.Single(await categories.GetByIdsAsync([parent.Id, deletedChild.Id, parent.Id])).Id);
        Assert.Null(await categories.GetBySlugAsync("deleted-child"));
        var loadedBrand = (await brands.GetBySlugAsync("brand"))!;
        Assert.Equal(brand.Id, loadedBrand.Id);
        var loadedMedia = Assert.Single(loadedBrand.Files);
        Assert.Equal(media.FileId, loadedMedia.FileId);
        Assert.Equal("video", loadedMedia.FileType);
        Assert.Equal(3, loadedMedia.DisplayOrder);
        Assert.True(loadedMedia.IsPrimary);
        Assert.Null(await brands.GetBySlugAsync("deleted-brand"));
        Assert.True(await categories.SlugExistsAsync("deleted-child"));
        Assert.True(await brands.SlugExistsAsync("deleted-brand"));
        Assert.False(await categories.SlugExistsAsync("parent", parent.Id));
        Assert.False(await brands.SlugExistsAsync("brand", brand.Id));
        Assert.Empty(db.ChangeTracker.Entries());
    }
}
