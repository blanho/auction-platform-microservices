#nullable enable
using BuildingBlocks.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auctions.Domain.Entities;

public class Item : BaseEntity
{
    private string _title = string.Empty;
    private string _description = string.Empty;

    public string Title
    {
        get => _title;
        set => _title = value;
    }

    public string Description
    {
        get => _description;
        set => _description = value;
    }

    public string? Condition { get; set; }
    public int? YearManufactured { get; set; }

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid? BrandId { get; set; }
    public Brand? Brand { get; set; }

    public Auction? Auction { get; set; }
    public Guid AuctionId { get; set; }

    [Column(TypeName = "jsonb")]
    public List<ItemFileInfo> Files { get; set; } = new();
    
    [Column(TypeName = "jsonb")]
    public Dictionary<string, string> Attributes { get; set; } = new();
    
    public void UpdateDetails(string title, string description, string? condition = null, int? yearManufactured = null)
    {
        Title = title;
        Description = description;
        Condition = condition;
        YearManufactured = yearManufactured;
    }

    public void AddFile(ItemFileInfo file)
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
            _title = source._title,
            _description = source._description,
            Condition = source.Condition,
            YearManufactured = source.YearManufactured,
            CategoryId = source.CategoryId,
            BrandId = source.BrandId
        };
    }
}
