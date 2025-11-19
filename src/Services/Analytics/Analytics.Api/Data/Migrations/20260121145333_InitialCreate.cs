using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Api.Data.Migrations
{

    public partial class InitialCreate : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ServiceName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fact_auctions",
                schema: "analytics",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IngestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    WinnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    DateKey = table.Column<DateOnly>(type: "date", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SellerUsername = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WinnerUsername = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CategoryName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CategoryPath = table.Column<string[]>(type: "text[]", nullable: true),
                    StartingPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReservePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FinalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BuyNowPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalBids = table.Column<int>(type: "integer", nullable: false),
                    UniqueBidders = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DurationHours = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Sold = table.Column<bool>(type: "boolean", nullable: false),
                    HadReserve = table.Column<bool>(type: "boolean", nullable: false),
                    ReserveMet = table.Column<bool>(type: "boolean", nullable: false),
                    HadBuyNow = table.Column<bool>(type: "boolean", nullable: false),
                    UsedBuyNow = table.Column<bool>(type: "boolean", nullable: false),
                    TimesExtended = table.Column<short>(type: "smallint", nullable: false),
                    Condition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    EventType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EventVersion = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fact_auctions", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "fact_bids",
                schema: "analytics",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IngestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BidderId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateKey = table.Column<DateOnly>(type: "date", nullable: false),
                    BidderUsername = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BidStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EventVersion = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fact_bids", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "fact_payments",
                schema: "analytics",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IngestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: true),
                    DateKey = table.Column<DateOnly>(type: "date", nullable: false),
                    BuyerUsername = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SellerUsername = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AuctionTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TrackingNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShippingCarrier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    IsRefunded = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ShippedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EventType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EventVersion = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fact_payments", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "PlatformSettings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    DataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ValidationRules = table.Column<string>(type: "jsonb", maxLength: 500, nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterUsername = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReportedUsername = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Resolution = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResolvedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    EscalatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityId",
                schema: "public",
                table: "AuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                schema: "public",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                schema: "public",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ServiceName",
                schema: "public",
                table: "AuditLogs",
                column: "ServiceName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ServiceName_Timestamp",
                schema: "public",
                table: "AuditLogs",
                columns: new[] { "ServiceName", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                schema: "public",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                schema: "public",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ix_fact_auctions_id_time",
                schema: "analytics",
                table: "fact_auctions",
                columns: new[] { "AuctionId", "EventTime" });

            migrationBuilder.CreateIndex(
                name: "ix_fact_auctions_seller_time",
                schema: "analytics",
                table: "fact_auctions",
                columns: new[] { "SellerId", "EventTime" });

            migrationBuilder.CreateIndex(
                name: "ix_fact_auctions_time",
                schema: "analytics",
                table: "fact_auctions",
                column: "EventTime");

            migrationBuilder.CreateIndex(
                name: "ix_fact_auctions_type_date",
                schema: "analytics",
                table: "fact_auctions",
                columns: new[] { "EventType", "DateKey" });

            migrationBuilder.CreateIndex(
                name: "ix_fact_bids_auction_time",
                schema: "analytics",
                table: "fact_bids",
                columns: new[] { "AuctionId", "EventTime" });

            migrationBuilder.CreateIndex(
                name: "ix_fact_bids_bidder_time",
                schema: "analytics",
                table: "fact_bids",
                columns: new[] { "BidderId", "EventTime" });

            migrationBuilder.CreateIndex(
                name: "ix_fact_bids_date",
                schema: "analytics",
                table: "fact_bids",
                column: "DateKey");

            migrationBuilder.CreateIndex(
                name: "ix_fact_bids_time",
                schema: "analytics",
                table: "fact_bids",
                column: "EventTime");

            migrationBuilder.CreateIndex(
                name: "ix_fact_payments_date_status",
                schema: "analytics",
                table: "fact_payments",
                columns: new[] { "DateKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_fact_payments_order_time",
                schema: "analytics",
                table: "fact_payments",
                columns: new[] { "OrderId", "EventTime" });

            migrationBuilder.CreateIndex(
                name: "ix_fact_payments_seller_time",
                schema: "analytics",
                table: "fact_payments",
                columns: new[] { "SellerId", "EventTime" });

            migrationBuilder.CreateIndex(
                name: "ix_fact_payments_time",
                schema: "analytics",
                table: "fact_payments",
                column: "EventTime");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformSettings_Category",
                schema: "public",
                table: "PlatformSettings",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformSettings_Key",
                schema: "public",
                table: "PlatformSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_CreatedAt",
                schema: "public",
                table: "Reports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Priority",
                schema: "public",
                table: "Reports",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportedUsername",
                schema: "public",
                table: "Reports",
                column: "ReportedUsername");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Status",
                schema: "public",
                table: "Reports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Type",
                schema: "public",
                table: "Reports",
                column: "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "fact_auctions",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "fact_bids",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "fact_payments",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "PlatformSettings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Reports",
                schema: "public");
        }
    }
}
