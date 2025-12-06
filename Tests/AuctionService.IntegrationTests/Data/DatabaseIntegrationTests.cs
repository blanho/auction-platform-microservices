using AuctionService.Domain.Entities;
using AuctionService.Infrastructure.Data;
using AuctionService.IntegrationTests.Fixtures;
using Common.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Data;

[Collection("AuctionApi")]
public class DatabaseIntegrationTests : IAsyncLifetime
{
    private readonly AuctionApiFactory _factory;

    public DatabaseIntegrationTests(AuctionApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CanConnectToDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        var canConnect = await context.Database.CanConnectAsync();

        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task CanCreateAndRetrieveAuction()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            ReversePrice = 15000,
            Seller = "test-seller",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AuctionEnd = DateTime.UtcNow.AddDays(7),
            Status = Status.Live,
            Item = new Item
            {
                Make = "Tesla",
                Model = "Model 3",
                Year = 2023,
                Color = "White",
                Mileage = 5000,
                ImageUrl = "https://example.com/tesla.jpg"
            }
        };

        context.Auctions.Add(auction);
        await context.SaveChangesAsync();

        var retrieved = await context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == auction.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Item.Make.Should().Be("Tesla");
        retrieved.Item.Model.Should().Be("Model 3");
    }

    [Fact]
    public async Task CanUpdateAuction()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            ReversePrice = 10000,
            Seller = "test-seller",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AuctionEnd = DateTime.UtcNow.AddDays(5),
            Status = Status.Live,
            Item = new Item
            {
                Make = "Honda",
                Model = "Civic",
                Year = 2022,
                Color = "Blue",
                Mileage = 20000,
                ImageUrl = "https://example.com/honda.jpg"
            }
        };

        context.Auctions.Add(auction);
        await context.SaveChangesAsync();

        auction.Item.Color = "Red";
        auction.ReversePrice = 12000;
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var updated = await context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == auction.Id);

        updated.Should().NotBeNull();
        updated!.Item.Color.Should().Be("Red");
        updated.ReversePrice.Should().Be(12000);
    }

    [Fact]
    public async Task CanDeleteAuction()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        var auctionId = Guid.NewGuid();
        var auction = new Auction
        {
            Id = auctionId,
            ReversePrice = 8000,
            Seller = "test-seller",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AuctionEnd = DateTime.UtcNow.AddDays(3),
            Status = Status.Live,
            Item = new Item
            {
                Make = "Nissan",
                Model = "Altima",
                Year = 2021,
                Color = "Gray",
                Mileage = 30000,
                ImageUrl = "https://example.com/nissan.jpg"
            }
        };

        context.Auctions.Add(auction);
        await context.SaveChangesAsync();

        context.Auctions.Remove(auction);
        await context.SaveChangesAsync();

        var deleted = await context.Auctions.FindAsync(auctionId);

        deleted.Should().BeNull();
    }

    [Fact]
    public async Task CanQueryAuctionsByStatus()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        var liveAuction1 = CreateAuction(Status.Live);
        var liveAuction2 = CreateAuction(Status.Live);
        var finishedAuction = CreateAuction(Status.Finished);

        context.Auctions.AddRange(liveAuction1, liveAuction2, finishedAuction);
        await context.SaveChangesAsync();

        var liveAuctions = await context.Auctions
            .Where(a => a.Status == Status.Live)
            .ToListAsync();

        liveAuctions.Should().HaveCount(2);
        liveAuctions.All(a => a.Status == Status.Live).Should().BeTrue();
    }

    private static Auction CreateAuction(Status status)
    {
        return new Auction
        {
            Id = Guid.NewGuid(),
            ReversePrice = 10000,
            Seller = "test-seller",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AuctionEnd = DateTime.UtcNow.AddDays(7),
            Status = status,
            Item = new Item
            {
                Make = "Test",
                Model = "Car",
                Year = 2023,
                Color = "Black",
                Mileage = 10000,
                ImageUrl = "https://example.com/test.jpg"
            }
        };
    }
}
