namespace Auctions.Domain.Entities;

public sealed record CreateItemParams
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string? Condition { get; init; }
    public int? YearManufactured { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public Guid? BrandId { get; init; }
    public string? BrandName { get; init; }
}
