using AuctionService.DTOs;

namespace AuctionService.Interfaces
{
    public interface IAuctionService
    {
        Task<List<AuctionDto>> GetAllAuctionsAsync(CancellationToken cancellationToken);
        Task<AuctionDto> GetAuctionByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<AuctionDto> CreateAuctionAsync(CreateAuctionDto dto, string seller, CancellationToken cancellationToken);
        Task<bool> UpdateAuctionAsync(Guid id, UpdateAuctionDto dto, CancellationToken cancellationToken);
        Task<bool> DeleteAuctionAsync(Guid id, CancellationToken cancellationToken);
    }
}
