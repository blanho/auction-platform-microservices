using AuctionService.Application.DTOs;

namespace AuctionService.Application.Interfaces
{
    public interface IAuctionService
    {
        Task<AuctionDto> CreateAuctionAsync(CreateAuctionDto dto, string seller, CancellationToken cancellationToken);
        Task<bool> UpdateAuctionAsync(Guid id, UpdateAuctionDto dto, CancellationToken cancellationToken);
        Task<bool> DeleteAuctionAsync(Guid id, CancellationToken cancellationToken);
    }
}
