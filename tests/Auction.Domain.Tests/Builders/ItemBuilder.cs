using Auctions.Domain.Entities;

namespace Auction.Domain.Tests.Builders;

public class ItemBuilder
{
    private string _title = "Test Item";
    private string _description = "Test Description";
    private string? _condition = "New";
    private int? _yearManufactured = 2024;
    private Guid? _categoryId = null;
    private Guid? _brandId = null;

    public static ItemBuilder Default() => new();

    public ItemBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ItemBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ItemBuilder WithCondition(string? condition)
    {
        _condition = condition;
        return this;
    }

    public ItemBuilder WithYearManufactured(int? year)
    {
        _yearManufactured = year;
        return this;
    }

    public ItemBuilder WithCategoryId(Guid? categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public ItemBuilder WithBrandId(Guid? brandId)
    {
        _brandId = brandId;
        return this;
    }

    public Item Build()
    {
        return Item.Create(
            _title,
            _description,
            _condition,
            _yearManufactured,
            _categoryId,
            _brandId);
    }
}
