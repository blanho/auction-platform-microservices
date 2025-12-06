using System.Net;
using System.Net.Http.Json;
using AuctionService.Application.DTOs;
using AuctionService.Domain.Entities;
using AuctionService.IntegrationTests.Fixtures;
using Common.Domain.Enums;

namespace AuctionService.IntegrationTests.Controllers;

[Collection("AuctionApi")]
public class AuctionsControllerTests : IAsyncLifetime
{
    private readonly AuctionApiFactory _factory;
    private readonly HttpClient _client;

    public AuctionsControllerTests(AuctionApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAuctions_WhenNoAuctions_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/auctions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auctions = await response.Content.ReadFromJsonAsync<List<AuctionDto>>();
        auctions.Should().NotBeNull();
        auctions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAuctions_WithAuctions_ReturnsAllAuctions()
    {
        // Arrange
        await SeedTestAuctions();

        // Act
        var response = await _client.GetAsync("/api/auctions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auctions = await response.Content.ReadFromJsonAsync<List<AuctionDto>>();
        auctions.Should().NotBeNull();
        auctions.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetAuctions_WithMakeFilter_ReturnsFilteredAuctions()
    {
        // Arrange
        await SeedTestAuctions();

        // Act
        var response = await _client.GetAsync("/api/auctions?make=Ford");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auctions = await response.Content.ReadFromJsonAsync<List<AuctionDto>>();
        auctions.Should().NotBeNull();
        auctions!.All(a => a.Make == "Ford").Should().BeTrue();
    }

    [Theory]
    [InlineData("orderBy=make")]
    [InlineData("orderBy=new")]
    [InlineData("pageSize=5")]
    public async Task GetAuctions_WithQueryParams_ReturnsValidResponse(string queryParams)
    {
        // Arrange
        await SeedTestAuctions();

        // Act
        var response = await _client.GetAsync($"/api/auctions?{queryParams}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAuctionById_WithValidId_ReturnsAuction()
    {
        // Arrange
        var auctionId = await SeedSingleAuction();

        // Act
        var response = await _client.GetAsync($"/api/auctions/{auctionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        auction.Should().NotBeNull();
        auction!.Id.Should().Be(auctionId);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/auctions/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAuction_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateAuctionDto
        {
            Title = "2023 BMW M3 for Sale",
            Description = "Excellent condition BMW M3",
            Make = "BMW",
            Model = "M3",
            Year = 2023,
            Color = "Blue",
            Mileage = 1000,
            ImageUrl = "https://example.com/bmw.jpg",
            ReservePrice = 50000,
            AuctionEnd = DateTime.UtcNow.AddDays(10)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auctions", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var auction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        auction.Should().NotBeNull();
        auction!.Make.Should().Be("BMW");
        auction.Model.Should().Be("M3");
    }

    [Fact]
    public async Task CreateAuction_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var invalidDto = new CreateAuctionDto
        {
            Title = "", 
            Description = "", 
            Make = "", 
            Model = "",
            Year = 1800, 
            Color = "",
            Mileage = -1,
            ReservePrice = 0,
            AuctionEnd = DateTime.UtcNow.AddDays(-1) 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auctions", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("", "Model", 2020, 100)] 
    [InlineData("Make", "", 2020, 100)] 
    [InlineData("Make", "Model", 1800, 100)] 
    [InlineData("Make", "Model", 2020, -1)] 
    public async Task CreateAuction_WithVariousInvalidData_ReturnsBadRequest(
        string make, string model, int year, int mileage)
    {
        // Arrange
        var invalidDto = new CreateAuctionDto
        {
            Title = "Test Auction",
            Description = "Test Description",
            Make = make,
            Model = model,
            Year = year,
            Color = "Red",
            Mileage = mileage,
            ReservePrice = 1000,
            AuctionEnd = DateTime.UtcNow.AddDays(10)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auctions", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAuction_WithValidData_ReturnsOk()
    {
        // Arrange
        var auctionId = await SeedSingleAuction();
        var updateDto = new UpdateAuctionDto
        {
            Make = "Updated Make",
            Model = "Updated Model",
            Year = 2024,
            Color = "Green",
            Mileage = 5000
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/auctions/{auctionId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateAuctionDto
        {
            Make = "Updated Make"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/auctions/{invalidId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAuction_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var auctionId = await SeedSingleAuction();

        // Act
        var response = await _client.DeleteAsync($"/api/auctions/{auctionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/auctions/{auctionId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/auctions/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task SeedTestAuctions()
    {
        await _factory.SeedDatabaseAsync(ctx =>
        {
            ctx.Auctions.AddRange(
                CreateTestAuction("Ford", "Mustang", 2022, "Red"),
                CreateTestAuction("Ford", "F-150", 2021, "Black"),
                CreateTestAuction("Toyota", "Camry", 2023, "White"),
                CreateTestAuction("Honda", "Accord", 2020, "Silver"),
                CreateTestAuction("BMW", "X5", 2023, "Blue")
            );
        });
    }

    private async Task<Guid> SeedSingleAuction()
    {
        var id = Guid.NewGuid();
        await _factory.SeedDatabaseAsync(ctx =>
        {
            ctx.Auctions.Add(CreateTestAuction("Test", "Car", 2023, "Red", id));
        });
        return id;
    }

    private static Auction CreateTestAuction(
        string make, string model, int year, string color, Guid? id = null)
    {
        return new Auction
        {
            Id = id ?? Guid.NewGuid(),
            ReversePrice = 10000,
            Seller = "test-seller",
            Winner = null,
            SoldAmount = null,
            CurrentHighBid = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AuctionEnd = DateTime.UtcNow.AddDays(10),
            Status = Status.Live,
            Item = new Item
            {
                Make = make,
                Model = model,
                Year = year,
                Color = color,
                Mileage = 10000,
                ImageUrl = $"https://example.com/{make.ToLower()}.jpg"
            }
        };
    }
}
