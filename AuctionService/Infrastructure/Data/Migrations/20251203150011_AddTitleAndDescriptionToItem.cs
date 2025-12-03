using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleAndDescriptionToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Items",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Items");
        }
    }
}
