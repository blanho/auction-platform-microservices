using Payment.Domain.Entities;

namespace Payment.Application.DTOs.Audit;

public record OrderAuditData
{
    public required Guid OrderId { get; init; }
    public required Guid AuctionId { get; init; }
    public required Guid BuyerId { get; init; }
    public string? BuyerUsername { get; init; }
    public required Guid SellerId { get; init; }
    public string? SellerUsername { get; init; }
    public string? ItemTitle { get; init; }
    public decimal WinningBid { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal? ShippingCost { get; init; }
    public required string Status { get; init; }
    public required string PaymentStatus { get; init; }
    public string? PaymentTransactionId { get; init; }
    public string? TrackingNumber { get; init; }
    public string? ShippingCarrier { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
    public DateTimeOffset? ShippedAt { get; init; }
    public DateTimeOffset? DeliveredAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public static OrderAuditData FromOrder(Order order)
    {
        return new OrderAuditData
        {
            OrderId = order.Id,
            AuctionId = order.AuctionId,
            BuyerId = order.BuyerId,
            BuyerUsername = order.BuyerUsername,
            SellerId = order.SellerId,
            SellerUsername = order.SellerUsername,
            ItemTitle = order.ItemTitle,
            WinningBid = order.WinningBid,
            TotalAmount = order.TotalAmount,
            ShippingCost = order.ShippingCost,
            Status = order.Status.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            PaymentTransactionId = order.PaymentTransactionId,
            TrackingNumber = order.TrackingNumber,
            ShippingCarrier = order.ShippingCarrier,
            PaidAt = order.PaidAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            CreatedAt = order.CreatedAt
        };
    }
}

public record WalletAuditData
{
    public required Guid WalletId { get; init; }
    public required Guid UserId { get; init; }
    public string? Username { get; init; }
    public decimal Balance { get; init; }
    public decimal HeldAmount { get; init; }
    public decimal AvailableBalance { get; init; }
    public string Currency { get; init; } = "USD";
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public static WalletAuditData FromWallet(Wallet wallet)
    {
        return new WalletAuditData
        {
            WalletId = wallet.Id,
            UserId = wallet.UserId,
            Username = wallet.Username,
            Balance = wallet.Balance,
            HeldAmount = wallet.HeldAmount,
            AvailableBalance = wallet.AvailableBalance,
            Currency = wallet.Currency,
            IsActive = wallet.IsActive,
            CreatedAt = wallet.CreatedAt
        };
    }
}

public record WalletTransactionAuditData
{
    public required Guid TransactionId { get; init; }
    public required Guid UserId { get; init; }
    public string? Username { get; init; }
    public required string Type { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceAfter { get; init; }
    public string? Description { get; init; }
    public required string Status { get; init; }
    public Guid? ReferenceId { get; init; }
    public string? ReferenceType { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public static WalletTransactionAuditData FromTransaction(WalletTransaction transaction)
    {
        return new WalletTransactionAuditData
        {
            TransactionId = transaction.Id,
            UserId = transaction.UserId,
            Username = transaction.Username,
            Type = transaction.Type.ToString(),
            Amount = transaction.Amount,
            BalanceAfter = transaction.Balance,
            Description = transaction.Description,
            Status = transaction.Status.ToString(),
            ReferenceId = transaction.ReferenceId,
            ReferenceType = transaction.ReferenceType,
            CreatedAt = transaction.CreatedAt
        };
    }
}
