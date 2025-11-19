using Auctions.Domain.Entities;
using BuildingBlocks.Domain.Enums;

namespace Auctions.Infrastructure.Persistence.Seeding.SeedData;

public static class AuctionSeedData
{
    private static readonly DateTimeOffset SeedDate = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset AuctionEndDate = DateTimeOffset.UtcNow.AddDays(7);

    public static List<Auction> GetAuctions()
    {
        return new List<Auction>
        {
            Auction.CreateForSeeding(
                id: Guid.Parse("22222222-2222-2222-2222-222222222001"),
                sellerUsername: "bob",
                item: CreateItem(
                    id: Guid.Parse("33333333-3333-3333-3333-333333333001"),
                    title: "2018 Ford Mustang GT",
                    description: "Powerful V8 muscle car in excellent condition",
                    condition: "Excellent",
                    yearManufactured: 2018,
                    categoryId: CategorySeedData.VehiclesId,
                    attributes: new() { { "Make", "Ford" }, { "Model", "Mustang" }, { "Color", "Red" }, { "Mileage", "45000" } }),
                reservePrice: 20000m,
                auctionEnd: AuctionEndDate,
                status: Status.Live,
                isFeatured: true,
                createdAt: SeedDate),

            Auction.CreateForSeeding(
                id: Guid.Parse("22222222-2222-2222-2222-222222222002"),
                sellerUsername: "alice",
                item: CreateItem(
                    id: Guid.Parse("33333333-3333-3333-3333-333333333002"),
                    title: "2020 Tesla Model 3",
                    description: "Electric sedan with autopilot feature",
                    condition: "Excellent",
                    yearManufactured: 2020,
                    categoryId: CategorySeedData.VehiclesId,
                    attributes: new() { { "Make", "Tesla" }, { "Model", "Model 3" }, { "Color", "White" }, { "Mileage", "30000" } }),
                reservePrice: 15000m,
                auctionEnd: AuctionEndDate.AddDays(3),
                status: Status.Live,
                isFeatured: true,
                createdAt: SeedDate),

            Auction.CreateForSeeding(
                id: Guid.Parse("22222222-2222-2222-2222-222222222003"),
                sellerUsername: "tom",
                item: CreateItem(
                    id: Guid.Parse("33333333-3333-3333-3333-333333333003"),
                    title: "MacBook Pro 16-inch M2",
                    description: "Latest MacBook Pro with M2 chip, 32GB RAM",
                    condition: "New",
                    yearManufactured: 2023,
                    categoryId: CategorySeedData.ElectronicsId,
                    attributes: new() { { "Brand", "Apple" }, { "Model", "MacBook Pro" }, { "Color", "Space Gray" }, { "RAM", "32GB" } }),
                reservePrice: 1500m,
                auctionEnd: AuctionEndDate.AddDays(5),
                status: Status.Live,
                isFeatured: true,
                createdAt: SeedDate),

            Auction.CreateForSeeding(
                id: Guid.Parse("22222222-2222-2222-2222-222222222004"),
                sellerUsername: "bob",
                item: CreateItem(
                    id: Guid.Parse("33333333-3333-3333-3333-333333333004"),
                    title: "Sony PlayStation 5",
                    description: "Gaming console with 2 controllers",
                    condition: "Like New",
                    yearManufactured: 2023,
                    categoryId: CategorySeedData.ElectronicsId,
                    attributes: new() { { "Brand", "Sony" }, { "Model", "PS5" }, { "Color", "White" }, { "Storage", "1TB" } }),
                reservePrice: 800m,
                auctionEnd: AuctionEndDate.AddDays(2),
                status: Status.Live,
                isFeatured: false,
                createdAt: SeedDate),

            Auction.CreateForSeeding(
                id: Guid.Parse("22222222-2222-2222-2222-222222222005"),
                sellerUsername: "alice",
                item: CreateItem(
                    id: Guid.Parse("33333333-3333-3333-3333-333333333005"),
                    title: "Vintage Rolex Submariner",
                    description: "Classic 1980s Rolex watch in mint condition",
                    condition: "Vintage - Mint",
                    yearManufactured: 1985,
                    categoryId: CategorySeedData.JewelryWatchesId,
                    attributes: new() { { "Brand", "Rolex" }, { "Model", "Submariner" }, { "Material", "Gold" } }),
                reservePrice: 5000m,
                auctionEnd: AuctionEndDate.AddDays(10),
                status: Status.Live,
                isFeatured: true,
                createdAt: SeedDate),

            Auction.CreateForSeeding(
                id: Guid.Parse("22222222-2222-2222-2222-222222222006"),
                sellerUsername: "tom",
                item: CreateItem(
                    id: Guid.Parse("33333333-3333-3333-3333-333333333006"),
                    title: "Nike Air Jordan 1 Retro",
                    description: "Limited edition sneakers, size 10",
                    condition: "New with Tags",
                    yearManufactured: 2024,
                    categoryId: CategorySeedData.FashionId,
                    attributes: new() { { "Brand", "Nike" }, { "Model", "Air Jordan 1" }, { "Color", "Black/Red" }, { "Size", "10" } }),
                reservePrice: 300m,
                auctionEnd: AuctionEndDate.AddDays(4),
                status: Status.Live,
                isFeatured: false,
                createdAt: SeedDate),

            Auction.CreateForSeeding(
                id: Guid.Parse("22222222-2222-2222-2222-222222222007"),
                sellerUsername: "alice",
                item: CreateItem(
                    id: Guid.Parse("33333333-3333-3333-3333-333333333007"),
                    title: "Original Picasso Sketch",
                    description: "Authenticated sketch from 1960s collection",
                    condition: "Vintage - Good",
                    yearManufactured: 1965,
                    categoryId: CategorySeedData.ArtCollectiblesId,
                    attributes: new() { { "Artist", "Picasso" }, { "Medium", "Pencil on Paper" }, { "Authenticated", "Yes" } }),
                reservePrice: 2000m,
                auctionEnd: AuctionEndDate.AddDays(8),
                status: Status.Live,
                isFeatured: true,
                createdAt: SeedDate),

            Auction.CreateForSeeding(
                id: Guid.Parse("22222222-2222-2222-2222-222222222008"),
                sellerUsername: "bob",
                item: CreateItem(
                    id: Guid.Parse("33333333-3333-3333-3333-333333333008"),
                    title: "Herman Miller Aeron Chair",
                    description: "Ergonomic office chair, fully adjustable",
                    condition: "Like New",
                    yearManufactured: 2022,
                    categoryId: CategorySeedData.HomeGardenId,
                    attributes: new() { { "Brand", "Herman Miller" }, { "Model", "Aeron" }, { "Color", "Black" }, { "Size", "Medium" } }),
                reservePrice: 1200m,
                auctionEnd: AuctionEndDate.AddDays(6),
                status: Status.Live,
                isFeatured: false,
                createdAt: SeedDate)
        };
    }

    private static Item CreateItem(
        Guid id,
        string title,
        string description,
        string? condition,
        int? yearManufactured,
        Guid? categoryId,
        Dictionary<string, string>? attributes = null)
    {
        return new Item
        {
            Id = id,
            Title = title,
            Description = description,
            Condition = condition,
            YearManufactured = yearManufactured,
            CategoryId = categoryId,
            Attributes = attributes ?? new Dictionary<string, string>(),
            CreatedAt = SeedDate,
            Files = new List<ItemFileInfo>()
        };
    }
}

