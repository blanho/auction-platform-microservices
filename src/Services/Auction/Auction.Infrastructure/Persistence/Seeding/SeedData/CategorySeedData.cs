namespace Auctions.Infrastructure.Persistence.Seeding.SeedData;

public static class CategorySeedData
{
    private static readonly DateTimeOffset SeedDate = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static Guid VehiclesId => Guid.Parse("11111111-1111-1111-1111-111111111001");
    public static Guid ElectronicsId => Guid.Parse("11111111-1111-1111-1111-111111111002");
    public static Guid ArtCollectiblesId => Guid.Parse("11111111-1111-1111-1111-111111111003");
    public static Guid FashionId => Guid.Parse("11111111-1111-1111-1111-111111111004");
    public static Guid HomeGardenId => Guid.Parse("11111111-1111-1111-1111-111111111005");
    public static Guid SportsOutdoorsId => Guid.Parse("11111111-1111-1111-1111-111111111006");
    public static Guid JewelryWatchesId => Guid.Parse("11111111-1111-1111-1111-111111111007");
    public static Guid RealEstateId => Guid.Parse("11111111-1111-1111-1111-111111111008");
    public static Guid BooksMediaId => Guid.Parse("11111111-1111-1111-1111-111111111009");
    public static Guid ToysGamesId => Guid.Parse("11111111-1111-1111-1111-111111111010");
    public static Guid OtherId => Guid.Parse("11111111-1111-1111-1111-111111111011");

    public static List<Domain.Entities.Category> GetCategories()
    {
        return new List<Domain.Entities.Category>
        {
            new()
            {
                Id = VehiclesId,
                Name = "Vehicles",
                Slug = "vehicles",
                Icon = "fa-car",
                Description = "Cars, motorcycles, boats and other vehicles",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = SeedDate
            },
            new()
            {
                Id = ElectronicsId,
                Name = "Electronics",
                Slug = "electronics",
                Icon = "fa-laptop",
                Description = "Computers, phones, gadgets and electronics",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = SeedDate
            },
            new()
            {
                Id = ArtCollectiblesId,
                Name = "Art and Collectibles",
                Slug = "art-collectibles",
                Icon = "fa-palette",
                Description = "Art, antiques, coins and collectible items",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = SeedDate
            },
            new()
            {
                Id = FashionId,
                Name = "Fashion",
                Slug = "fashion",
                Icon = "fa-shirt",
                Description = "Clothing, shoes, accessories and jewelry",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = SeedDate
            },
            new()
            {
                Id = HomeGardenId,
                Name = "Home & Garden",
                Slug = "home-garden",
                Icon = "fa-house",
                Description = "Furniture, appliances and home decor",
                DisplayOrder = 5,
                IsActive = true,
                CreatedAt = SeedDate
            },
            new()
            {
                Id = SportsOutdoorsId,
                Name = "Sports & Outdoors",
                Slug = "sports-outdoors",
                Icon = "fa-futbol",
                Description = "Sports equipment and outdoor gear",
                DisplayOrder = 6,
                IsActive = true,
                CreatedAt = SeedDate
            },
            new()
            {
                Id = JewelryWatchesId,
                Name = "Jewelry & Watches",
                Slug = "jewelry-watches",
                Icon = "fa-gem",
                Description = "Fine jewelry, watches and accessories",
                DisplayOrder = 7,
                IsActive = true,
                CreatedAt = SeedDate
            },
            new()
            {
                Id = RealEstateId,
                Name = "Real Estate",
                Slug = "real-estate",
                Icon = "fa-building",
                Description = "Properties, land and real estate",
                DisplayOrder = 8,
                IsActive = true,
                CreatedAt = SeedDate
            },
            new()
            {
                Id = BooksMediaId,
                Name = "Books & Media",
                Slug = "books-media",
                Icon = "fa-book",
                Description = "Books, music, movies and media",
                DisplayOrder = 9,
                IsActive = true,
                CreatedAt = SeedDate
            },
            new()
            {
                Id = ToysGamesId,
                Name = "Toys & Games",
                Slug = "toys-games",
                Icon = "fa-gamepad",
                Description = "Toys, board games and video games",
                DisplayOrder = 10,
                IsActive = true,
                CreatedAt = SeedDate
            },
            new()
            {
                Id = OtherId,
                Name = "Other",
                Slug = "other",
                Icon = "fa-box",
                Description = "Other items and miscellaneous",
                DisplayOrder = 99,
                IsActive = true,
                CreatedAt = SeedDate
            }
        };
    }
}

