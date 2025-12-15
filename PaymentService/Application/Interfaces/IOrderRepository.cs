using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task<Order> GetByAuctionIdAsync(Guid auctionId);
    Task<IEnumerable<Order>> GetByBuyerUsernameAsync(string username, int page = 1, int pageSize = 10);
    Task<IEnumerable<Order>> GetBySellerUsernameAsync(string username, int page = 1, int pageSize = 10);
    Task<Order> AddAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task<int> GetCountByBuyerUsernameAsync(string username);
    Task<int> GetCountBySellerUsernameAsync(string username);
}
