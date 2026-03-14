#nullable enable
using BuildingBlocks.Domain.Entities;

namespace Auctions.Domain.Entities;

public class Item : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public string? Condition { get; private set; }
    public int? YearManufactured { get; private set; }

    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public Guid? BrandId { get; private set; }
    public Brand? Brand { get; private set; }

    public Auction? Auction { get; private set; }
    public Guid AuctionId { get; private set; }

    public List<MediaFile> Files { get; private set; } = new();

    public Dictionary<string, string> Attributes { get; private set; } = new();

    private Item() { }

    public static Item Create(
        string title,
        string description,
        string? condition = null,
        int? yearManufactured = null,
        Guid? categoryId = null,
        Guid? brandId = null)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Condition = condition,
            YearManufactured = yearManufactured,
            CategoryId = categoryId,
            BrandId = brandId,
            Files = new List<MediaFile>(),
            Attributes = new Dictionary<string, string>()
        };
    }

    public void UpdateDetails(string title, string description, string? condition = null, int? yearManufactured = null)
    {
        Title = title;
        Description = description;
        Condition = condition;
        YearManufactured = yearManufactured;
    }

    public void UpdateTitle(string title)
    {
        Title = title;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }

    public void UpdateCondition(string? condition)
    {
        Condition = condition;
    }

    public void UpdateYearManufactured(int? yearManufactured)
    {
        YearManufactured = yearManufactured;
    }

    public void UpdateCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
    }

    public void UpdateBrand(Guid? brandId)
    {
        BrandId = brandId;
    }

    public void AddFile(MediaFile file)
    {
        Files.Add(file);
    }

    public void RemoveFile(Guid fileId)
    {
        var file = Files.FirstOrDefault(f => f.FileId == fileId);
        if (file != null)
        {
            Files.Remove(file);
        }
    }

    public void SetAttribute(string key, string value)
    {
        Attributes[key] = value;
    }

    public void RemoveAttribute(string key)
    {
        Attributes.Remove(key);
    }

    internal static Item CreateSnapshot(Item source)
    {
        return new Item
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            Condition = source.Condition,
            YearManufactured = source.YearManufactured,
            CategoryId = source.CategoryId,
            BrandId = source.BrandId
        };
    }
}
