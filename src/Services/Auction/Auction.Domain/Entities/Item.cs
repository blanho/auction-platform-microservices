#nullable enable
using BuildingBlocks.Domain.Entities;

namespace Auctions.Domain.Entities;

/// <summary>
/// Item is an owned child entity of the Auction aggregate.
/// It carries only the IDs of Brand and Category (resolved from the Catalog service).
/// Denormalized CategoryName and BrandName are stored here and kept in sync via
/// integration events from the Catalog service, avoiding cross-service joins at query time.
/// </summary>
public class Item : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public string? Condition { get; private set; }
    public int? YearManufactured { get; private set; }

    /// <summary>FK reference to Catalog.Category — no navigation property.</summary>
    public Guid? CategoryId { get; private set; }

    /// <summary>Denormalized from Catalog service; updated via CategoryUpdatedIntegrationEvent.</summary>
    public string? CategoryName { get; private set; }

    /// <summary>FK reference to Catalog.Brand — no navigation property.</summary>
    public Guid? BrandId { get; private set; }

    /// <summary>Denormalized from Catalog service; updated via BrandUpdatedIntegrationEvent.</summary>
    public string? BrandName { get; private set; }

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
        string? categoryName = null,
        Guid? brandId = null,
        string? brandName = null)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Condition = condition,
            YearManufactured = yearManufactured,
            CategoryId = categoryId,
            CategoryName = categoryName,
            BrandId = brandId,
            BrandName = brandName,
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

    public void UpdateTitle(string title) => Title = title;

    public void UpdateDescription(string description) => Description = description;

    public void UpdateCondition(string? condition) => Condition = condition;

    public void UpdateYearManufactured(int? yearManufactured) => YearManufactured = yearManufactured;

    public void UpdateCategory(Guid? categoryId, string? categoryName = null)
    {
        CategoryId = categoryId;
        if (categoryName is not null) CategoryName = categoryName;
    }

    public void UpdateBrand(Guid? brandId, string? brandName = null)
    {
        BrandId = brandId;
        if (brandName is not null) BrandName = brandName;
    }

    /// <summary>Called when a Catalog BrandUpdatedIntegrationEvent is received.</summary>
    public void SyncBrandName(string brandName) => BrandName = brandName;

    /// <summary>Called when a Catalog CategoryUpdatedIntegrationEvent is received.</summary>
    public void SyncCategoryName(string categoryName) => CategoryName = categoryName;

    public void AddFile(MediaFile file) => Files.Add(file);

    public void RemoveFile(Guid fileId)
    {
        var file = Files.FirstOrDefault(f => f.FileId == fileId);
        if (file != null)
            Files.Remove(file);
    }

    public void SetAttribute(string key, string value) => Attributes[key] = value;

    public void RemoveAttribute(string key) => Attributes.Remove(key);

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
            CategoryName = source.CategoryName,
            BrandId = source.BrandId,
            BrandName = source.BrandName
        };
    }
}
