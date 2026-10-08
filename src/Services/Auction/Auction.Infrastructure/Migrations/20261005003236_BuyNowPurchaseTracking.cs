using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auctions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BuyNowPurchaseTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuyNowPurchase",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Buyer = table.Column<string>(type: "text", nullable: false),
                    Seller = table.Column<string>(type: "text", nullable: false),
                    ItemTitle = table.Column<string>(type: "text", nullable: false),
                    BuyNowPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyNowPurchase", x => x.CorrelationId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuyNowPurchase_AuctionId_BuyerId_CreatedAt",
                table: "BuyNowPurchase",
                columns: new[] { "AuctionId", "BuyerId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuyNowPurchase");
        }
    }
}
