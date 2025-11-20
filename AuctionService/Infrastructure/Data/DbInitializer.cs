using AuctionService.Infrastructure.Data.SeedData;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void InitDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            ApplyMigrations(context);
            SeedData(context);
        }

        private static void ApplyMigrations(AuctionDbContext context)
        {
            try
            {
                context.Database.Migrate();
                Console.WriteLine("Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying migrations: {ex.Message}");
                throw;
            }
        }

        private static void SeedData(AuctionDbContext context)
        {
            if (context.Auctions.Any())
            {
                return;
            }

            try
            {
                var auctions = AuctionSeedData.GetAuctions();
                context.Auctions.AddRange(auctions);
                context.SaveChanges();

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
