using AuctionService.Domain.Entities;

namespace AuctionService.Application.Interfaces
{
    public interface IFlashSaleRepository
    {
        Task<FlashSale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<FlashSale?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<FlashSale>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<List<FlashSale>> GetActiveFlashSalesAsync(CancellationToken cancellationToken = default);
        Task<FlashSale?> GetCurrentFlashSaleAsync(CancellationToken cancellationToken = default);
        Task<FlashSale> AddAsync(FlashSale flashSale, CancellationToken cancellationToken = default);
        Task UpdateAsync(FlashSale flashSale, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<FlashSaleItem> AddItemAsync(FlashSaleItem item, CancellationToken cancellationToken = default);
        Task RemoveItemAsync(Guid flashSaleId, Guid auctionId, CancellationToken cancellationToken = default);
    }
}
