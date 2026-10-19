using Bidding.Domain.Entities;

namespace Bidding.Application.Interfaces;

public interface IBidWriter
{
    Task CreateAsync(Bid bid, CancellationToken cancellationToken);
}
