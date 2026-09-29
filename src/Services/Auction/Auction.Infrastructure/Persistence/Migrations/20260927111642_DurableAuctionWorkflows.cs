using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auctions.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DurableAuctionWorkflows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuctionWorkflowReceipt",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Succeeded = table.Column<int>(type: "integer", nullable: false),
                    Failed = table.Column<int>(type: "integer", nullable: false),
                    ErrorsJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionWorkflowReceipt", x => x.Key);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuctionWorkflowReceipt_CorrelationId",
                table: "AuctionWorkflowReceipt",
                column: "CorrelationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuctionWorkflowReceipt");
        }
    }
}
