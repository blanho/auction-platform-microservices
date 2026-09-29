using AuctionService.Contracts.Events;

namespace Auctions.Infrastructure.Messaging.Consumers;

public record ImportTotals(int Succeeded, int Failed, List<ImportRowErrorPayload> Errors);
