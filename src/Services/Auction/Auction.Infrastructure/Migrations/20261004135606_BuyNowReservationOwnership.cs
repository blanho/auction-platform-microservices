using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auctions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BuyNowReservationOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BuyNowBuyerId",
                table: "Auctions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BuyNowCorrelationId",
                table: "Auctions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BuyNowOrderId",
                table: "Auctions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "CancelledBuyNowAttempts",
                table: "Auctions",
                type: "uuid[]",
                nullable: false,
                defaultValueSql: "'{}'::uuid[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyNowBuyerId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "BuyNowCorrelationId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "BuyNowOrderId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "CancelledBuyNowAttempts",
                table: "Auctions");
        }
    }
}
