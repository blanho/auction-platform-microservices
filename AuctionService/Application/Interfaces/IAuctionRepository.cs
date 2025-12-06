using AuctionService.Domain.Entities;
using Common.Domain.Enums;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Interfaces;

public interface IAuctionRepository : IRepository<Auction>
{
    Task<List<Auction>> GetFinishedAuctionsAsync(CancellationToken cancellationToken = default);
    
    Task<List<Auction>> GetAuctionsToAutoDeactivateAsync(CancellationToken cancellationToken = default);
    
    Task<List<Auction>> GetAuctionsForExportAsync(
        Status? status = null,
        string? seller = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default);
}
