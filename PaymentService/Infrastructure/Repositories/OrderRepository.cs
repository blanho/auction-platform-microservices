using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly PaymentDbContext _context;

    public OrderRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Order> GetByIdAsync(Guid id)
    {
        return await _context.Orders.FindAsync(id);
    }

    public async Task<Order> GetByAuctionIdAsync(Guid auctionId)
    {
        return await _context.Orders.FirstOrDefaultAsync(o => o.AuctionId == auctionId);
    }

    public async Task<IEnumerable<Order>> GetByBuyerUsernameAsync(string username, int page = 1, int pageSize = 10)
    {
        return await _context.Orders
            .Where(o => o.BuyerUsername == username)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetBySellerUsernameAsync(string username, int page = 1, int pageSize = 10)
    {
        return await _context.Orders
            .Where(o => o.SellerUsername == username)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Order> AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        order.UpdatedAt = DateTimeOffset.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<int> GetCountByBuyerUsernameAsync(string username)
    {
        return await _context.Orders.CountAsync(o => o.BuyerUsername == username);
    }

    public async Task<int> GetCountBySellerUsernameAsync(string username)
    {
        return await _context.Orders.CountAsync(o => o.SellerUsername == username);
    }
}
