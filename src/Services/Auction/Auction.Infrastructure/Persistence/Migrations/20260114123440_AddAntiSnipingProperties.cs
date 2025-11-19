using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auctions.Infrastructure.Persistence.Migrations
{

    public partial class AddAntiSnipingProperties : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AntiSnipingEnabled",
                table: "Auctions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AntiSnipingExtensionMinutes",
                table: "Auctions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AntiSnipingWindowMinutes",
                table: "Auctions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxExtensions",
                table: "Auctions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimesExtended",
                table: "Auctions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AntiSnipingEnabled",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "AntiSnipingExtensionMinutes",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "AntiSnipingWindowMinutes",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "MaxExtensions",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "TimesExtended",
                table: "Auctions");
        }
    }
}
