using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BuyNowOrderCompensation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_AuctionId",
                table: "Orders");

            migrationBuilder.AddColumn<bool>(
                name: "AwaitingBuyNowCompletion",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "BuyNowCorrelationId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BuyNowOrderAttempt",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cancelled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyNowOrderAttempt", x => x.CorrelationId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AuctionId",
                table: "Orders",
                column: "AuctionId",
                unique: true,
                filter: "\"Status\" <> 7");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuyNowOrderAttempt");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AuctionId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AwaitingBuyNowCompletion",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BuyNowCorrelationId",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AuctionId",
                table: "Orders",
                column: "AuctionId",
                unique: true);
        }
    }
}
