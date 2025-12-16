using AuctionService.Domain.Entities;
using Common.Domain.Enums;

namespace AuctionService.Infrastructure.Data.Seeding.SeedData;

public static class AuctionSeedData
{
    private static readonly DateTimeOffset SeedDate = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset AuctionEndDate = DateTimeOffset.UtcNow.AddDays(7);

    public static List<Auction> GetAuctions()
    {
        return new List<Auction>
        {
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222001"),
                SellerUsername = "bob",
                ReservePrice = 20000m,
                Currency = "USD",
                AuctionEnd = AuctionEndDate,
                Status = Status.Live,
                IsFeatured = true,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333001"),
                    Title = "2018 Ford Mustang GT",
                    Description = "Powerful V8 muscle car in excellent condition",
                    Condition = "Excellent",
                    YearManufactured = 2018,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Make", "Ford" },
                        { "Model", "Mustang" },
                        { "Color", "Red" },
                        { "Mileage", "45000" }
                    },
                    CategoryId = CategorySeedData.VehiclesId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222002"),
                SellerUsername = "alice",
                ReservePrice = 15000m,
                Currency = "USD",
                AuctionEnd = AuctionEndDate.AddDays(3),
                Status = Status.Live,
                IsFeatured = true,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333002"),
                    Title = "2020 Tesla Model 3",
                    Description = "Electric sedan with autopilot feature",
                    Condition = "Excellent",
                    YearManufactured = 2020,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Make", "Tesla" },
                        { "Model", "Model 3" },
                        { "Color", "White" },
                        { "Mileage", "30000" }
                    },
                    CategoryId = CategorySeedData.VehiclesId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222003"),
                SellerUsername = "tom",
                ReservePrice = 1500m,
                Currency = "USD",
                AuctionEnd = AuctionEndDate.AddDays(5),
                Status = Status.Live,
                IsFeatured = true,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333003"),
                    Title = "MacBook Pro 16-inch M2",
                    Description = "Latest MacBook Pro with M2 chip, 32GB RAM",
                    Condition = "New",
                    YearManufactured = 2023,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Brand", "Apple" },
                        { "Model", "MacBook Pro" },
                        { "Color", "Space Gray" },
                        { "RAM", "32GB" }
                    },
                    CategoryId = CategorySeedData.ElectronicsId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222004"),
                SellerUsername = "bob",
                ReservePrice = 800m,
                Currency = "USD",
                AuctionEnd = AuctionEndDate.AddDays(2),
                Status = Status.Live,
                IsFeatured = false,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333004"),
                    Title = "Sony PlayStation 5",
                    Description = "Gaming console with 2 controllers",
                    Condition = "Like New",
                    YearManufactured = 2023,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Brand", "Sony" },
                        { "Model", "PS5" },
                        { "Color", "White" },
                        { "Storage", "1TB" }
                    },
                    CategoryId = CategorySeedData.ElectronicsId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222005"),
                SellerUsername = "alice",
                ReservePrice = 5000m,
                Currency = "USD",
                AuctionEnd = AuctionEndDate.AddDays(10),
                Status = Status.Live,
                IsFeatured = true,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333005"),
                    Title = "Vintage Rolex Submariner",
                    Description = "Classic 1980s Rolex watch in mint condition",
                    Condition = "Vintage - Mint",
                    YearManufactured = 1985,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Brand", "Rolex" },
                        { "Model", "Submariner" },
                        { "Material", "Gold" }
                    },
                    CategoryId = CategorySeedData.JewelryWatchesId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222006"),
                SellerUsername = "tom",
                ReservePrice = 300m,
                Currency = "USD",
                AuctionEnd = AuctionEndDate.AddDays(4),
                Status = Status.Live,
                IsFeatured = false,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333006"),
                    Title = "Nike Air Jordan 1 Retro",
                    Description = "Limited edition sneakers, size 10",
                    Condition = "New with Tags",
                    YearManufactured = 2024,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Brand", "Nike" },
                        { "Model", "Air Jordan 1" },
                        { "Color", "Black/Red" },
                        { "Size", "10" }
                    },
                    CategoryId = CategorySeedData.FashionId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222007"),
                SellerUsername = "alice",
                ReservePrice = 2000m,
                Currency = "USD",
                AuctionEnd = AuctionEndDate.AddDays(8),
                Status = Status.Live,
                IsFeatured = true,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333007"),
                    Title = "Original Picasso Sketch",
                    Description = "Authenticated sketch from 1960s collection",
                    Condition = "Vintage - Good",
                    YearManufactured = 1965,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Artist", "Picasso" },
                        { "Medium", "Pencil on Paper" },
                        { "Authenticated", "Yes" }
                    },
                    CategoryId = CategorySeedData.ArtCollectiblesId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222008"),
                SellerUsername = "bob",
                ReservePrice = 1200m,
                Currency = "USD",
                AuctionEnd = AuctionEndDate.AddDays(6),
                Status = Status.Live,
                IsFeatured = false,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333008"),
                    Title = "Herman Miller Aeron Chair",
                    Description = "Ergonomic office chair, fully adjustable",
                    Condition = "Like New",
                    YearManufactured = 2022,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Brand", "Herman Miller" },
                        { "Model", "Aeron" },
                        { "Color", "Black" },
                        { "Size", "Medium" }
                    },
                    CategoryId = CategorySeedData.HomeGardenId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            }
        };
    }
}
