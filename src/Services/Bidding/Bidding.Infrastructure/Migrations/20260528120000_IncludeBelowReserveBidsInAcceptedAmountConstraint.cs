using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bidding.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IncludeBelowReserveBidsInAcceptedAmountConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bids_AuctionId_Amount",
                table: "Bids");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_AuctionId_Amount",
                table: "Bids",
                columns: new[] { "AuctionId", "Amount" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"Status\" IN (1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bids_AuctionId_Amount",
                table: "Bids");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_AuctionId_Amount",
                table: "Bids",
                columns: new[] { "AuctionId", "Amount" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"Status\" = 1");
        }
    }
}
