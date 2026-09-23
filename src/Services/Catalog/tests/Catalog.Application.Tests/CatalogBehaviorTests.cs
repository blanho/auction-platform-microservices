using System.Reflection;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using Catalog.Application.DTOs;
using Catalog.Application.Filtering;
using Catalog.Application.Features.Brands.DeleteBrand;
using Catalog.Application.Features.Brands.GetBrands;
using Catalog.Application.Features.Brands.UpdateBrand;
using Catalog.Application.Features.Categories.DeleteCategory;
using Catalog.Application.Features.Categories.GetCategories;
using Catalog.Application.Features.Categories.GetCategoryTree;
using Catalog.Application.Features.Categories.UpdateCategory;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Catalog.Application.Tests;

public class CatalogBehaviorTests
{
    [Fact]
    public async Task CategoryTree_IncludesGrandchildrenAndOrdersSiblings()
    {
        var root = Category.Create("Root", "root");
        var child = Category.Create("Child", "child", parentCategoryId: root.Id);
        var grandchild = Category.Create("Grandchild", "grandchild", parentCategoryId: child.Id);
        var first = Category.Create("First", "first", displayOrder: -1, parentCategoryId: root.Id);
        var repository = Stub<ICategoryRepository>((method, _) => method.Name switch
        {
            "GetAllAsync" => Task.FromResult(new List<Category> { grandchild, child, root, first }),
            _ => throw new NotSupportedException(method.Name)
        });
        var result = await new GetCategoryTreeQueryHandler(repository).Handle(new(), default);
        var tree = Assert.Single(result.Value!);
        Assert.Equal(new[] { first.Id, child.Id }, tree.Children.Select(c => c.Id));
        Assert.Equal(grandchild.Id, Assert.Single(tree.Children[1].Children).Id);
    }

    [Theory]
    [InlineData("missing", "Category.ParentNotFound")]
    [InlineData("self", "Category.CannotBeOwnParent")]
    [InlineData("descendant", "Category.CannotBeOwnParent")]
    public async Task UpdateCategory_RejectsInvalidParentWithoutWriting(string scenario, string errorCode)
    {
        var category = Category.Create("Root", "root");
        var child = Category.Create("Child", "child", parentCategoryId: category.Id);
        var parentId = scenario == "self" ? category.Id : scenario == "descendant" ? child.Id : Guid.NewGuid();
        var repository = Stub<ICategoryRepository>((method, args) => method.Name switch
        {
            "GetByIdAsync" => Task.FromResult((Guid)args![0]! == category.Id ? category : (Guid)args[0]! == child.Id ? child : null),
            _ => throw new NotSupportedException(method.Name)
        });
        var handler = new UpdateCategoryCommandHandler(repository, null!, null!, null!);
        var result = await handler.Handle(new(category.Id, "Renamed", null, "fa-box", null, 0, true, parentId), default);
        Assert.Equal(errorCode, result.Error?.Code);
        Assert.Equal("Root", category.Name);
        Assert.Null(category.ParentCategoryId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CategoryList_ForwardsActiveFilter(bool activeOnly)
    {
        var repository = Stub<ICategoryRepository>((method, args) =>
        {
            Assert.Equal("GetAllAsync", method.Name);
            Assert.Equal(!activeOnly, args![0]);
            return Task.FromResult(new List<Category>());
        });
        var mapper = Stub<IMapper>((_, _) => new List<CategoryDto>());
        var result = await new GetCategoriesQueryHandler(repository, mapper).Handle(new(activeOnly), default);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteCategory_UsesSoftDeleteRepositoryAndSaves()
    {
        var category = Category.Create("Category", "category");
        var deleted = false;
        var saved = false;
        var repository = Stub<ICategoryRepository>((method, args) =>
        {
            if (method.Name == "GetByIdAsync") return Task.FromResult<Category?>(category);
            if (method.Name == "HasChildrenAsync") return Task.FromResult(false);
            Assert.Equal("DeleteAsync", method.Name);
            Assert.Equal(category.Id, args![0]);
            deleted = true;
            return Task.CompletedTask;
        });
        var unit = SavingUnit(() => { Assert.True(deleted); saved = true; });
        var result = await new DeleteCategoryCommandHandler(repository, unit, NullLogger<DeleteCategoryCommandHandler>.Instance)
            .Handle(new(category.Id), default);
        Assert.True(result.IsSuccess);
        Assert.True(saved);
    }

    [Fact]
    public async Task DeleteBrand_UsesSoftDeleteRepositoryAndSaves()
    {
        var brand = Brand.Create("Brand", "brand");
        var deleted = false;
        var saved = false;
        var repository = Stub<IBrandRepository>((method, args) =>
        {
            if (method.Name == "GetByIdAsync") return Task.FromResult<Brand?>(brand);
            Assert.Equal("DeleteAsync", method.Name);
            Assert.Equal(brand.Id, args![0]);
            deleted = true;
            return Task.CompletedTask;
        });
        var unit = SavingUnit(() => { Assert.True(deleted); saved = true; });
        var result = await new DeleteBrandCommandHandler(repository, unit, NullLogger<DeleteBrandCommandHandler>.Instance)
            .Handle(new(brand.Id), default);
        Assert.True(result.IsSuccess);
        Assert.True(saved);
    }

    [Theory]
    [InlineData("  Café & Tea  ", "cafe-tea")]
    [InlineData(null, "original")]
    public async Task UpdateBrand_PersistsNormalizedSlugOrPreservesItWhenNameOmitted(string? name, string expectedSlug)
    {
        var brand = Brand.Create("Original", "original");
        var updated = false;
        var repository = Stub<IBrandRepository>((method, args) =>
        {
            switch (method.Name)
            {
                case "GetByIdAsync": return Task.FromResult<Brand?>(brand);
                case "SlugExistsAsync":
                    Assert.Equal(expectedSlug, args![0]);
                    Assert.Equal(brand.Id, args[1]);
                    return Task.FromResult(false);
                case "UpdateAsync":
                    Assert.Equal(expectedSlug, ((Brand)args![0]!).Slug);
                    updated = true;
                    return Task.CompletedTask;
                default: throw new NotSupportedException(method.Name);
            }
        });
        var handler = new UpdateBrandCommandHandler(repository, SavingUnit(() => Assert.True(updated)),
            Stub<IMapper>((_, _) => new BrandDto()), NullLogger<UpdateBrandCommandHandler>.Instance);
        var result = await handler.Handle(new(brand.Id, name, null, null, null, null), default);
        Assert.True(result.IsSuccess);
        Assert.True(updated);
    }

    [Theory]
    [InlineData(2, 5, 2, 5)]
    [InlineData(0, 1000, 1, 100)]
    public async Task BrandList_ForwardsBoundedQueryAndPreservesTotalCount(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        var brand = Brand.Create("Featured", "featured", isFeatured: true);
        var repository = Stub<IBrandRepository>((method, args) =>
        {
            Assert.Equal("GetPagedAsync", method.Name);
            var parameters = Assert.IsType<BrandQueryParams>(args![0]);
            Assert.False(parameters.ActiveOnly);
            Assert.True(parameters.FeaturedOnly);
            Assert.Equal(expectedPage, parameters.Page);
            Assert.Equal(expectedPageSize, parameters.PageSize);
            Assert.Equal("Featured", parameters.Search);
            Assert.Equal("name", parameters.SortBy);
            Assert.True(parameters.SortDescending);
            return Task.FromResult(new PaginatedResult<Brand>(new[] { brand }, 42, parameters.Page, parameters.PageSize));
        });
        var mapper = Stub<IMapper>((_, args) =>
        {
            Assert.Equal(brand.Id, Assert.Single((IEnumerable<Brand>)args![0]!).Id);
            return new List<BrandDto> { new() { Id = brand.Id } };
        });
        var result = await new GetBrandsQueryHandler(repository, mapper)
            .Handle(new(false, true, page, pageSize, " Featured ", "name", "desc"), default);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value!.TotalCount);
        Assert.Equal(expectedPage, result.Value.Page);
        Assert.Equal(expectedPageSize, result.Value.PageSize);
        Assert.Single(result.Value.Items);
    }

    private static IUnitOfWork SavingUnit(Action onSave) => Stub<IUnitOfWork>((method, _) =>
    {
        Assert.Equal("SaveChangesAsync", method.Name);
        onSave();
        return Task.FromResult(1);
    });

    private static T Stub<T>(Func<MethodInfo, object?[]?, object?> invoke) where T : class
    {
        var proxy = DispatchProxy.Create<T, TestProxy>();
        ((TestProxy)(object)proxy).Handler = invoke;
        return proxy;
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Handler(targetMethod!, args);
    }
}
