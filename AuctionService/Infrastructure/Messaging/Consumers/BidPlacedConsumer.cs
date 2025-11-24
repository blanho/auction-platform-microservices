using AuctionService.Application.Interfaces;
using Common.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionService.Infrastructure.Messaging.Consumers
{
    public class BidPlacedConsumer : IConsumer<BidPlacedEvent>
    {
        private readonly IAuctionRepository _repository;
        private readonly ILogger<BidPlacedConsumer> _logger;
        public BidPlacedConsumer(IAuctionRepository repository, ILogger<BidPlacedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BidPlacedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Consuming BidPlacedEvent for auction {AuctionId}", message.AuctionId);

            var auction = await _repository.GetByIdAsync(message.AuctionId);
            if (auction == null)
            {
                _logger.LogWarning("Auction {AuctionId} not found", message.AuctionId);
                return;
            }
            if (auction.CurrentHighBid == null || message.BidStatus.Contains("Accepted")
                && message.BidAmount > auction.CurrentHighBid)
            {
                auction.CurrentHighBid = message.BidAmount;
                await _repository.UpdateAsync(auction);
            }
        }
    }
}
