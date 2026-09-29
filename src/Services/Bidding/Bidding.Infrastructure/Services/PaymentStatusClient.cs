using Bidding.Application.Interfaces;
using MassTransit;
using PaymentService.Contracts.Requests;

namespace Bidding.Infrastructure.Services;

public class PaymentStatusClient(IRequestClient<GetBuyerAuctionPaymentStatuses> client) : IPaymentStatusClient
{
    public async Task<IReadOnlyDictionary<Guid, Application.Interfaces.AuctionPaymentStatus>> GetForBuyerAsync(
        Guid buyerId, CancellationToken cancellationToken)
    {
        var response = await client.GetResponse<BuyerAuctionPaymentStatuses>(
            new GetBuyerAuctionPaymentStatuses(buyerId), cancellationToken);
        return response.Message.Items.ToDictionary(x => x.AuctionId,
            x => new Application.Interfaces.AuctionPaymentStatus(x.Status, x.IsPaid));
    }
}
