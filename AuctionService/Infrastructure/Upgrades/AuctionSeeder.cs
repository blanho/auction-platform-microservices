using AuctionService.Domain.Entities;
using AuctionService.Infrastructure.Data;
using Common.Core.Constants;

namespace AuctionService.Infrastructure.Upgrades;

/// <summary>
/// Service to seed initial auction data for development/testing.
/// </summary>
public static class AuctionSeeder
{
    public static async Task SeedAuctionsAsync(AuctionDbContext context)
    {
        // Check if data already exists
        if (context.Auctions.Any())
        {
            return;
        }

        var auctions = new List<Auction>
        {
            new Auction
            {
                Id = Guid.NewGuid(),
                ReversePrice = 50000,
                Seller = "John Doe",
                Winner = null,
                SoldAmount = null,
                CurrentHighBid = null,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = SystemGuids.System,
                AuctionEnd = DateTimeOffset.UtcNow.AddDays(7),
                Status = Status.Live,
                IsDeleted = false,
                Item = new Item
                {
                    Id = Guid.NewGuid(),
                    Make = "Toyota",
                    Model = "Camry",
                    Year = 2020,
                    Color = "Silver",
                    Mileage = 45000,
                    ImageUrl = "https://via.placeholder.com/300",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = SystemGuids.System,
                    IsDeleted = false
                }
            },
            new Auction
            {
                Id = Guid.NewGuid(),
                ReversePrice = 75000,
                Seller = "Jane Smith",
                Winner = null,
                SoldAmount = null,
                CurrentHighBid = null,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = SystemGuids.System,
                AuctionEnd = DateTimeOffset.UtcNow.AddDays(10),
                Status = Status.Live,
                IsDeleted = false,
                Item = new Item
                {
                    Id = Guid.NewGuid(),
                    Make = "Honda",
                    Model = "Civic",
                    Year = 2021,
                    Color = "Blue",
                    Mileage = 32000,
                    ImageUrl = "https://via.placeholder.com/300",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = SystemGuids.System,
                    IsDeleted = false
                }
            },
            new Auction
            {
                Id = Guid.NewGuid(),
                ReversePrice = 120000,
                Seller = "Bob Johnson",
                Winner = null,
                SoldAmount = null,
                CurrentHighBid = null,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = SystemGuids.System,
                AuctionEnd = DateTimeOffset.UtcNow.AddDays(5),
                Status = Status.Live,
                IsDeleted = false,
                Item = new Item
                {
                    Id = Guid.NewGuid(),
                    Make = "BMW",
                    Model = "X5",
                    Year = 2022,
                    Color = "Black",
                    Mileage = 15000,
                    ImageUrl = "https://via.placeholder.com/300",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = SystemGuids.System,
                    IsDeleted = false
                }
            }
        };

        await context.Auctions.AddRangeAsync(auctions);
        await context.SaveChangesAsync();
    }
}
