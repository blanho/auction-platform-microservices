using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bidding.Infrastructure.Migrations
{

    public partial class AddTenantId : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Bids",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Bids",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AutoBids",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AutoBids");
        }
    }
}
