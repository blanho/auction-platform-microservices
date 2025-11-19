
using System;
using Analytics.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Analytics.Api.Data.Migrations
{
    [DbContext(typeof(AnalyticsDbContext))]
    [Migration("20260121145333_InitialCreate")]
    partial class InitialCreate
    {

        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("public")
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Analytics.Api.Entities.AuditLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Action")
                        .HasColumnType("integer");

                    b.Property<string>("ChangedProperties")
                        .HasColumnType("jsonb");

                    b.Property<string>("CorrelationId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("EntityId")
                        .HasColumnType("uuid");

                    b.Property<string>("EntityType")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("IpAddress")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Metadata")
                        .HasColumnType("jsonb");

                    b.Property<string>("NewValues")
                        .HasColumnType("jsonb");

                    b.Property<string>("OldValues")
                        .HasColumnType("jsonb");

                    b.Property<string>("ServiceName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<string>("Username")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("EntityId");

                    b.HasIndex("EntityType");

                    b.HasIndex("ServiceName");

                    b.HasIndex("Timestamp");

                    b.HasIndex("UserId");

                    b.HasIndex("EntityType", "EntityId");

                    b.HasIndex("ServiceName", "Timestamp");

                    b.ToTable("AuditLogs", "public");
                });

            modelBuilder.Entity("Analytics.Api.Entities.FactAuction", b =>
                {
                    b.Property<Guid>("EventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuctionId")
                        .HasColumnType("uuid");

                    b.Property<decimal?>("BuyNowPrice")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<Guid?>("CategoryId")
                        .HasColumnType("uuid");

                    b.Property<string>("CategoryName")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.PrimitiveCollection<string[]>("CategoryPath")
                        .HasColumnType("text[]");

                    b.Property<string>("Condition")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("character varying(3)");

                    b.Property<DateOnly>("DateKey")
                        .HasColumnType("date");

                    b.Property<decimal?>("DurationHours")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)");

                    b.Property<DateTimeOffset?>("EndedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("EventTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<short>("EventVersion")
                        .HasColumnType("smallint");

                    b.Property<decimal?>("FinalPrice")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<bool>("HadBuyNow")
                        .HasColumnType("boolean");

                    b.Property<bool>("HadReserve")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset>("IngestedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("ReserveMet")
                        .HasColumnType("boolean");

                    b.Property<decimal?>("ReservePrice")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<Guid>("SellerId")
                        .HasColumnType("uuid");

                    b.Property<string>("SellerUsername")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<bool>("Sold")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("StartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("StartingPrice")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<short>("TimesExtended")
                        .HasColumnType("smallint");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int>("TotalBids")
                        .HasColumnType("integer");

                    b.Property<int>("UniqueBidders")
                        .HasColumnType("integer");

                    b.Property<bool>("UsedBuyNow")
                        .HasColumnType("boolean");

                    b.Property<Guid?>("WinnerId")
                        .HasColumnType("uuid");

                    b.Property<string>("WinnerUsername")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("EventId");

                    b.HasIndex("EventTime")
                        .HasDatabaseName("ix_fact_auctions_time");

                    b.HasIndex("AuctionId", "EventTime")
                        .HasDatabaseName("ix_fact_auctions_id_time");

                    b.HasIndex("EventType", "DateKey")
                        .HasDatabaseName("ix_fact_auctions_type_date");

                    b.HasIndex("SellerId", "EventTime")
                        .HasDatabaseName("ix_fact_auctions_seller_time");

                    b.ToTable("fact_auctions", "analytics");
                });

            modelBuilder.Entity("Analytics.Api.Entities.FactBid", b =>
                {
                    b.Property<Guid>("EventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuctionId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("BidAmount")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<string>("BidStatus")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<Guid>("BidderId")
                        .HasColumnType("uuid");

                    b.Property<string>("BidderUsername")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateOnly>("DateKey")
                        .HasColumnType("date");

                    b.Property<DateTimeOffset>("EventTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<short>("EventVersion")
                        .HasColumnType("smallint");

                    b.Property<DateTimeOffset>("IngestedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("EventId");

                    b.HasIndex("DateKey")
                        .HasDatabaseName("ix_fact_bids_date");

                    b.HasIndex("EventTime")
                        .HasDatabaseName("ix_fact_bids_time");

                    b.HasIndex("AuctionId", "EventTime")
                        .HasDatabaseName("ix_fact_bids_auction_time");

                    b.HasIndex("BidderId", "EventTime")
                        .HasDatabaseName("ix_fact_bids_bidder_time");

                    b.ToTable("fact_bids", "analytics");
                });

            modelBuilder.Entity("Analytics.Api.Entities.FactPayment", b =>
                {
                    b.Property<Guid>("EventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuctionId")
                        .HasColumnType("uuid");

                    b.Property<string>("AuctionTitle")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<Guid>("BuyerId")
                        .HasColumnType("uuid");

                    b.Property<string>("BuyerUsername")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateOnly>("DateKey")
                        .HasColumnType("date");

                    b.Property<DateTimeOffset?>("DeliveredAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("EventTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<short>("EventVersion")
                        .HasColumnType("smallint");

                    b.Property<DateTimeOffset>("IngestedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsPaid")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsRefunded")
                        .HasColumnType("boolean");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("PaidAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("SellerId")
                        .HasColumnType("uuid");

                    b.Property<string>("SellerUsername")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTimeOffset?>("ShippedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ShippingCarrier")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<decimal>("TotalAmount")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<string>("TrackingNumber")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("TransactionId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("EventId");

                    b.HasIndex("EventTime")
                        .HasDatabaseName("ix_fact_payments_time");

                    b.HasIndex("DateKey", "Status")
                        .HasDatabaseName("ix_fact_payments_date_status");

                    b.HasIndex("OrderId", "EventTime")
                        .HasDatabaseName("ix_fact_payments_order_time");

                    b.HasIndex("SellerId", "EventTime")
                        .HasDatabaseName("ix_fact_payments_seller_time");

                    b.ToTable("fact_payments", "analytics");
                });

            modelBuilder.Entity("Analytics.Api.Entities.PlatformSetting", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Category")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DataType")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Description")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<bool>("IsSystem")
                        .HasColumnType("boolean");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("LastModifiedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ValidationRules")
                        .HasMaxLength(500)
                        .HasColumnType("jsonb");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)");

                    b.HasKey("Id");

                    b.HasIndex("Category");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.ToTable("PlatformSettings", "public");
                });

            modelBuilder.Entity("Analytics.Api.Entities.Report", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AdminNotes")
                        .HasColumnType("text");

                    b.Property<Guid?>("AuctionId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<DateTimeOffset?>("EscalatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Priority")
                        .HasColumnType("integer");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("ReportedUsername")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("ReporterUsername")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Resolution")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<DateTimeOffset?>("ResolvedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ResolvedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("Priority");

                    b.HasIndex("ReportedUsername");

                    b.HasIndex("Status");

                    b.HasIndex("Type");

                    b.ToTable("Reports", "public");
                });
#pragma warning restore 612, 618
        }
    }
}
