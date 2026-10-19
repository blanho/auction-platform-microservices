using Bidding.Application.DTOs;
using Bidding.Application.Interfaces;
using Bidding.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;

namespace Bidding.Application.Services;

public sealed class BidWriter(IBidRepository repository, IUnitOfWork unitOfWork, IAuditPublisher auditPublisher)
    : IBidWriter
{
    public async Task CreateAsync(Bid bid, CancellationToken cancellationToken)
    {
        await repository.CreateAsync(bid, cancellationToken);
        await auditPublisher.PublishAsync(bid.Id, BidAuditData.FromBid(bid), AuditAction.Created,
            cancellationToken: cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
