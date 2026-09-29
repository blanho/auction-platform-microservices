namespace PaymentService.Contracts.Requests;

public record AuctionPaymentStatus(Guid AuctionId, string Status, bool IsPaid);
