using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    public partial class WalletPaymentIdempotency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM "WalletTransactions"
                        WHERE "Type" = 4 AND "ReferenceId" IS NOT NULL
                        GROUP BY "ReferenceId" HAVING count(*) > 1) THEN
                        RAISE EXCEPTION 'Duplicate wallet payment references exist; reconcile historical payments before applying WalletPaymentIdempotency';
                    END IF;
                END $$;
                """);

            migrationBuilder.CreateIndex(
                name: "UX_WalletTransactions_PaymentReference",
                table: "WalletTransactions",
                column: "ReferenceId",
                unique: true,
                filter: "\"Type\" = 4 AND \"ReferenceId\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_WalletTransactions_PaymentReference",
                table: "WalletTransactions");
        }
    }
}
