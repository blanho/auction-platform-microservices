using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DurableJobProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "JobItems",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "ProgressEntries",
                columns: table => new
                {
                    CorrelationId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BatchId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CompletedCount = table.Column<int>(type: "integer", nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    Applied = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressEntries", x => new { x.CorrelationId, x.BatchId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressEntries");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "JobItems");
        }
    }
}
