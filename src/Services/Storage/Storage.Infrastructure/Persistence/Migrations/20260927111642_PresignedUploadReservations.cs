using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Storage.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PresignedUploadReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UploadExpiresAt",
                table: "StoredFiles",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadExpiresAt",
                table: "StoredFiles");
        }
    }
}
