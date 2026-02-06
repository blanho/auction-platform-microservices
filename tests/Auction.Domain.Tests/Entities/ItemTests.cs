using Auction.Domain.Tests.Builders;
using Auctions.Domain.Entities;

namespace Auction.Domain.Tests.Entities;

public class ItemTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateItem()
    {
        var title = "Test Item";
        var description = "Test Description";
        var condition = "New";
        var yearManufactured = 2024;

        var item = ItemBuilder.Default()
            .WithTitle(title)
            .WithDescription(description)
            .WithCondition(condition)
            .WithYearManufactured(yearManufactured)
            .Build();

        item.Should().NotBeNull();
        item.Id.Should().NotBeEmpty();
        item.Title.Should().Be(title);
        item.Description.Should().Be(description);
        item.Condition.Should().Be(condition);
        item.YearManufactured.Should().Be(yearManufactured);
    }

    [Fact]
    public void Create_WithCategoryAndBrand_ShouldSetIds()
    {
        var categoryId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var item = ItemBuilder.Default()
            .WithCategoryId(categoryId)
            .WithBrandId(brandId)
            .Build();

        item.CategoryId.Should().Be(categoryId);
        item.BrandId.Should().Be(brandId);
    }

    [Fact]
    public void Create_ShouldInitializeEmptyCollections()
    {
        var item = ItemBuilder.Default().Build();

        item.Files.Should().NotBeNull();
        item.Files.Should().BeEmpty();
        item.Attributes.Should().NotBeNull();
        item.Attributes.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateAllFields()
    {
        var item = ItemBuilder.Default().Build();
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var newCondition = "Used";
        var newYear = 2020;

        item.UpdateDetails(newTitle, newDescription, newCondition, newYear);

        item.Title.Should().Be(newTitle);
        item.Description.Should().Be(newDescription);
        item.Condition.Should().Be(newCondition);
        item.YearManufactured.Should().Be(newYear);
    }

    [Fact]
    public void UpdateTitle_ShouldOnlyUpdateTitle()
    {
        var originalDescription = "Original Description";
        var item = ItemBuilder.Default()
            .WithDescription(originalDescription)
            .Build();
        var newTitle = "New Title";

        item.UpdateTitle(newTitle);

        item.Title.Should().Be(newTitle);
        item.Description.Should().Be(originalDescription);
    }

    [Fact]
    public void UpdateDescription_ShouldOnlyUpdateDescription()
    {
        var originalTitle = "Original Title";
        var item = ItemBuilder.Default()
            .WithTitle(originalTitle)
            .Build();
        var newDescription = "New Description";

        item.UpdateDescription(newDescription);

        item.Description.Should().Be(newDescription);
        item.Title.Should().Be(originalTitle);
    }

    [Fact]
    public void UpdateCondition_ShouldUpdateCondition()
    {
        var item = ItemBuilder.Default()
            .WithCondition("New")
            .Build();

        item.UpdateCondition("Used");

        item.Condition.Should().Be("Used");
    }

    [Fact]
    public void UpdateCondition_ToNull_ShouldSetNull()
    {
        var item = ItemBuilder.Default()
            .WithCondition("New")
            .Build();

        item.UpdateCondition(null);

        item.Condition.Should().BeNull();
    }

    [Fact]
    public void UpdateYearManufactured_ShouldUpdateYear()
    {
        var item = ItemBuilder.Default()
            .WithYearManufactured(2020)
            .Build();

        item.UpdateYearManufactured(2024);

        item.YearManufactured.Should().Be(2024);
    }

    [Fact]
    public void AddFile_ShouldAddMediaFileToCollection()
    {
        var item = ItemBuilder.Default().Build();
        var mediaFile = new MediaFile
        {
            FileId = Guid.NewGuid(),
            FileType = "image/jpeg",
            DisplayOrder = 1,
            IsPrimary = true
        };

        item.AddFile(mediaFile);

        item.Files.Should().ContainSingle();
        item.Files[0].FileId.Should().Be(mediaFile.FileId);
    }

    [Fact]
    public void SetAttribute_ShouldAddOrUpdateAttribute()
    {
        var item = ItemBuilder.Default().Build();

        item.SetAttribute("Color", "Red");
        item.SetAttribute("Size", "Large");

        item.Attributes.Should().HaveCount(2);
        item.Attributes["Color"].Should().Be("Red");
        item.Attributes["Size"].Should().Be("Large");
    }

    [Fact]
    public void SetAttribute_WithExistingKey_ShouldUpdateValue()
    {
        var item = ItemBuilder.Default().Build();
        item.SetAttribute("Color", "Red");

        item.SetAttribute("Color", "Blue");

        item.Attributes["Color"].Should().Be("Blue");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1900)]
    [InlineData(2024)]
    public void Create_WithVariousYears_ShouldAcceptAll(int? year)
    {
        var item = ItemBuilder.Default()
            .WithYearManufactured(year)
            .Build();

        item.YearManufactured.Should().Be(year);
    }
}
