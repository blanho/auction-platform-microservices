using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Storage.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RetrySafeReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReportRequestId",
                table: "StoredFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_OwnerId_ReportRequestId",
                table: "StoredFiles",
                columns: new[] { "OwnerId", "ReportRequestId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoredFiles_OwnerId_ReportRequestId",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "ReportRequestId",
                table: "StoredFiles");
        }
    }
}
