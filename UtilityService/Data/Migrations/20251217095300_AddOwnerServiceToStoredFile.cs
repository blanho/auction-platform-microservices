using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerServiceToStoredFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerService",
                table: "StoredFiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_OwnerService",
                table: "StoredFiles",
                column: "OwnerService");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_OwnerService_EntityId",
                table: "StoredFiles",
                columns: new[] { "OwnerService", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoredFiles_OwnerService_EntityId",
                table: "StoredFiles");

            migrationBuilder.DropIndex(
                name: "IX_StoredFiles_OwnerService",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "OwnerService",
                table: "StoredFiles");
        }
    }
}
