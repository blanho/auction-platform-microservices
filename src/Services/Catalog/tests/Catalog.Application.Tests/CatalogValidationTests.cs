using System.Reflection;
using System.Text.Json;
using BuildingBlocks.Application.Helpers;
using Catalog.Application.DTOs;
using Catalog.Application.Features.Brands.CreateBrand;
using Catalog.Application.Features.Categories.CreateCategory;
using Catalog.Application.Features.Categories.DeleteCategory;
using Catalog.Application.Features.Categories.UpdateCategory;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Xunit;

namespace Catalog.Application.Tests;

public class CatalogValidationTests
{
    [Theory]
    [InlineData("日本の時計", "日本の時計")]
    [InlineData("Café & Tea", "cafe-tea")]
    [InlineData("Đồ cổ", "đo-co")]
    [InlineData("  東京 -- 時計  ", "東京-時計")]
    public void Slug_PreservesUnicodeLetters(string name, string expected) =>
        Assert.Equal(expected, SlugHelper.GenerateSlug(name));

    [Theory]
    [InlineData("!!!")]
    [InlineData("😀")]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateBrand_RejectsNamesThatCannotGenerateSlug(string name) =>
        Assert.False(new CreateBrandCommandValidator().Validate(new CreateBrandCommand(name, null, 0, false)).IsValid);

    [Fact]
    public void CreateCategory_ValidatesNormalizedSlugAndDatabaseLengths()
    {
        var validator = new CreateCategoryCommandValidator();
        Assert.True(validator.Validate(new CreateCategoryCommand("時計", null, "fa-box", null, 0, null)).IsValid);
        Assert.False(validator.Validate(new CreateCategoryCommand("時計", "!!!", "fa-box", null, 0, null)).IsValid);
        Assert.False(validator.Validate(new CreateCategoryCommand(new string('a', 101), null, "fa-box", null, 0, null)).IsValid);
        Assert.False(validator.Validate(new CreateCategoryCommand("時計", null, "fa-box", new string('a', 501), 0, null)).IsValid);
    }

    [Fact]
    public void CategoryUpdate_RejectsIncompleteJsonInsteadOfDefaultingStoredFields()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UpdateCategoryDto>("""{"name":"Changed","slug":"changed"}""", options));
        var dto = JsonSerializer.Deserialize<UpdateCategoryDto>("""
            {"name":"Changed","slug":"changed","icon":"fa-watch","displayOrder":7,"isActive":true,"parentCategoryId":null}
            """, options)!;
        Assert.True(dto.IsActive);
        Assert.Equal(7, dto.DisplayOrder);
        Assert.Null(dto.ParentCategoryId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ParentWithChildren_CannotBeDeletedOrDeactivated(bool delete)
    {
        var parent = Category.Create("Parent", "parent");
        var repository = Repository((method, _) => method.Name switch
        {
            "GetByIdAsync" => Task.FromResult<Category?>(parent),
            "HasChildrenAsync" => Task.FromResult(true),
            _ => throw new InvalidOperationException("No write should happen")
        });
        var error = delete
            ? (await new DeleteCategoryCommandHandler(repository, null!, null!).Handle(new(parent.Id), default)).Error
            : (await new UpdateCategoryCommandHandler(repository, null!, null!, null!).Handle(new(parent.Id, parent.Name, parent.Slug, parent.Icon, null, 0, false, null), default)).Error;
        Assert.Equal("Category.HasChildren", error?.Code);
        Assert.True(parent.IsActive);
    }

    [Fact]
    public async Task ActiveCategory_CannotMoveUnderInactiveParent()
    {
        var category = Category.Create("Child", "child");
        var parent = Category.Create("Parent", "parent", isActive: false);
        var repository = Repository((method, args) => method.Name == "GetByIdAsync"
            ? Task.FromResult<Category?>((Guid)args![0]! == category.Id ? category : parent)
            : throw new InvalidOperationException("No write should happen"));
        var result = await new UpdateCategoryCommandHandler(repository, null!, null!, null!)
            .Handle(new(category.Id, category.Name, category.Slug, category.Icon, null, 0, true, parent.Id), default);
        Assert.Equal("Category.ParentInactive", result.Error?.Code);
        Assert.Null(category.ParentCategoryId);
    }

    private static ICategoryRepository Repository(Func<MethodInfo, object?[]?, object?> handler)
    {
        var proxy = DispatchProxy.Create<ICategoryRepository, CatalogBehaviorTests.TestProxy>();
        ((CatalogBehaviorTests.TestProxy)(object)proxy).Handler = handler;
        return proxy;
    }
}
