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
                Seller = "bob",
                ReversePrice = 20000,
                AuctionEnd = AuctionEndDate,
                Status = Status.Live,
                IsFeatured = true,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333001"),
                    Title = "2018 Ford Mustang GT",
                    Description = "Powerful V8 muscle car in excellent condition",
                    Make = "Ford",
                    Model = "Mustang",
                    Year = 2018,
                    Color = "Red",
                    Mileage = 45000,
                    CategoryId = CategorySeedData.VehiclesId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222002"),
                Seller = "alice",
                ReversePrice = 15000,
                AuctionEnd = AuctionEndDate.AddDays(3),
                Status = Status.Live,
                IsFeatured = true,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333002"),
                    Title = "2020 Tesla Model 3",
                    Description = "Electric sedan with autopilot feature",
                    Make = "Tesla",
                    Model = "Model 3",
                    Year = 2020,
                    Color = "White",
                    Mileage = 30000,
                    CategoryId = CategorySeedData.VehiclesId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222003"),
                Seller = "tom",
                ReversePrice = 1500,
                AuctionEnd = AuctionEndDate.AddDays(5),
                Status = Status.Live,
                IsFeatured = true,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333003"),
                    Title = "MacBook Pro 16-inch M2",
                    Description = "Latest MacBook Pro with M2 chip, 32GB RAM",
                    Make = "Apple",
                    Model = "MacBook Pro",
                    Year = 2023,
                    Color = "Space Gray",
                    Mileage = 0,
                    CategoryId = CategorySeedData.ElectronicsId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222004"),
                Seller = "bob",
                ReversePrice = 800,
                AuctionEnd = AuctionEndDate.AddDays(2),
                Status = Status.Live,
                IsFeatured = false,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333004"),
                    Title = "Sony PlayStation 5",
                    Description = "Gaming console with 2 controllers",
                    Make = "Sony",
                    Model = "PS5",
                    Year = 2023,
                    Color = "White",
                    Mileage = 0,
                    CategoryId = CategorySeedData.ElectronicsId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222005"),
                Seller = "alice",
                ReversePrice = 5000,
                AuctionEnd = AuctionEndDate.AddDays(10),
                Status = Status.Live,
                IsFeatured = true,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333005"),
                    Title = "Vintage Rolex Submariner",
                    Description = "Classic 1980s Rolex watch in mint condition",
                    Make = "Rolex",
                    Model = "Submariner",
                    Year = 1985,
                    Color = "Gold",
                    Mileage = 0,
                    CategoryId = CategorySeedData.JewelryWatchesId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222006"),
                Seller = "tom",
                ReversePrice = 300,
                AuctionEnd = AuctionEndDate.AddDays(4),
                Status = Status.Live,
                IsFeatured = false,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333006"),
                    Title = "Nike Air Jordan 1 Retro",
                    Description = "Limited edition sneakers, size 10",
                    Make = "Nike",
                    Model = "Air Jordan 1",
                    Year = 2024,
                    Color = "Black/Red",
                    Mileage = 0,
                    CategoryId = CategorySeedData.FashionId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222007"),
                Seller = "alice",
                ReversePrice = 2000,
                AuctionEnd = AuctionEndDate.AddDays(8),
                Status = Status.Live,
                IsFeatured = true,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333007"),
                    Title = "Original Picasso Sketch",
                    Description = "Authenticated sketch from 1960s collection",
                    Make = "Picasso",
                    Model = "Sketch",
                    Year = 1965,
                    Color = "Black/White",
                    Mileage = 0,
                    CategoryId = CategorySeedData.ArtCollectiblesId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222008"),
                Seller = "bob",
                ReversePrice = 1200,
                AuctionEnd = AuctionEndDate.AddDays(6),
                Status = Status.Live,
                IsFeatured = false,
                CreatedAt = SeedDate,
                Item = new Item
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333008"),
                    Title = "Herman Miller Aeron Chair",
                    Description = "Ergonomic office chair, fully adjustable",
                    Make = "Herman Miller",
                    Model = "Aeron",
                    Year = 2022,
                    Color = "Black",
                    Mileage = 0,
                    CategoryId = CategorySeedData.HomeGardenId,
                    CreatedAt = SeedDate,
                    Files = new List<ItemFileInfo>()
                }
            }
        };
    }
}
