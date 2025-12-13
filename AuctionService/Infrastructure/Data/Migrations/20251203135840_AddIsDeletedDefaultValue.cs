using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing NULL values to false first
            migrationBuilder.Sql(@"UPDATE ""Auctions"" SET ""IsDeleted"" = false WHERE ""IsDeleted"" IS NULL;");
            migrationBuilder.Sql(@"UPDATE ""Items"" SET ""IsDeleted"" = false WHERE ""IsDeleted"" IS NULL;");
            
            // Add default value constraint
            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Auctions",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Items",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Auctions",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Items",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);
        }
    }
}
