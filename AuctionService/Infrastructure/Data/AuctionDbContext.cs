using AuctionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Infrastructure.Data
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options)
        {

        }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Item> Items { get; set; }
    }
}
