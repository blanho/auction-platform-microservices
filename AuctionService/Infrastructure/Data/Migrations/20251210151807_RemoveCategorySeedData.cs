using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuctionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategorySeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111001"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111002"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111003"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111004"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111005"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111006"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111007"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111008"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111009"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111010"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111011"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "Icon", "ImageUrl", "IsActive", "Name", "ParentCategoryId", "Slug", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111001"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), "Cars, motorcycles, boats and other vehicles", 1, "fa-car", null, true, "Vehicles", null, "vehicles", null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("11111111-1111-1111-1111-111111111002"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), "Computers, phones, gadgets and electronics", 2, "fa-laptop", null, true, "Electronics", null, "electronics", null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("11111111-1111-1111-1111-111111111003"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), "Art, antiques, coins and collectible items", 3, "fa-palette", null, true, "Art & Collectibles", null, "art-collectibles", null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("11111111-1111-1111-1111-111111111004"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), "Clothing, shoes, accessories and jewelry", 4, "fa-shirt", null, true, "Fashion", null, "fashion", null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("11111111-1111-1111-1111-111111111005"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), "Furniture, appliances and home decor", 5, "fa-house", null, true, "Home & Garden", null, "home-garden", null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("11111111-1111-1111-1111-111111111006"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), "Sports equipment and outdoor gear", 6, "fa-futbol", null, true, "Sports & Outdoors", null, "sports-outdoors", null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("11111111-1111-1111-1111-111111111007"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), "Fine jewelry, watches and accessories", 7, "fa-gem", null, true, "Jewelry & Watches", null, "jewelry-watches", null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("11111111-1111-1111-1111-111111111008"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), "Properties, land and real estate", 8, "fa-building", null, true, "Real Estate", null, "real-estate", null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("11111111-1111-1111-1111-111111111009"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), "Books, music, movies and media", 9, "fa-book", null, true, "Books & Media", null, "books-media", null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("11111111-1111-1111-1111-111111111010"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), "Toys, board games and video games", 10, "fa-gamepad", null, true, "Toys & Games", null, "toys-games", null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("11111111-1111-1111-1111-111111111011"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), "Other items and miscellaneous", 99, "fa-box", null, true, "Other", null, "other", null, new Guid("00000000-0000-0000-0000-000000000000") }
                });
        }
    }
}
