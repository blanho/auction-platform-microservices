using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoBid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoBids",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Bidder = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MaxAmount = table.Column<int>(type: "integer", nullable: false),
                    CurrentBidAmount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastBidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoBids", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutoBids_AuctionId",
                table: "AutoBids",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_AutoBids_AuctionId_Bidder",
                table: "AutoBids",
                columns: new[] { "AuctionId", "Bidder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AutoBids_Bidder",
                table: "AutoBids",
                column: "Bidder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoBids");
        }
    }
}
