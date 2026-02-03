using BuildingBlocks.Application.Abstractions;

namespace Payment.Application.Errors;

public static class PaymentErrors
{
    public static class Order
    {
        public static Error NotFound => Error.Create("Order.NotFound", "Order not found");
        public static Error NotFoundById(Guid id) => Error.Create("Order.NotFound", $"Order with ID {id} was not found");
        public static Error AlreadyExists => Error.Create("Order.AlreadyExists", "Order already exists");
        public static Error AlreadyExistsForAuction(Guid auctionId) => Error.Create("Order.AlreadyExists", $"Order for auction {auctionId} already exists");
        public static Error AlreadyPaid => Error.Create("Order.AlreadyPaid", "Order is already paid");
        public static Error AlreadyPaidById(Guid id) => Error.Create("Order.AlreadyPaid", $"Order {id} has already been paid");
        public static Error Cancelled => Error.Create("Order.Cancelled", "Order has been cancelled");
        public static Error CancelledById(Guid id) => Error.Create("Order.Cancelled", $"Order {id} has been cancelled");
        public static Error InvalidStatus => Error.Create("Order.InvalidStatus", "Order is in an invalid status for this operation");
        public static Error InvalidStatusWithDetails(string status) => Error.Create("Order.InvalidStatus", $"Order cannot be processed in {status} status");
        public static Error InvalidOrderData => Error.Create("Order.InvalidData", "Required order data is missing: BuyerId, SellerId, BuyerUsername, SellerUsername, ItemTitle, and WinningBid are required");
        public static Error NotPaid => Error.Create("Order.NotPaid", "Order is not paid yet");
        public static Error NotPaidById(Guid id) => Error.Create("Order.NotPaid", $"Order {id} has not been paid yet");
        public static Error AlreadyShipped => Error.Create("Order.AlreadyShipped", "Order is already shipped");
        public static Error AlreadyShippedById(Guid id) => Error.Create("Order.AlreadyShipped", $"Order {id} has already been shipped");
        public static Error AlreadyDelivered => Error.Create("Order.AlreadyDelivered", "Order has already been delivered");
        public static Error NotShipped => Error.Create("Order.NotShipped", "Order has not been shipped yet");
        public static Error CreateFailed(string reason) => Error.Create("Order.CreateFailed", $"Failed to create order: {reason}");
        public static Error UpdateFailed(string reason) => Error.Create("Order.UpdateFailed", $"Failed to update order: {reason}");
    }

    public static class Wallet
    {
        public static Error NotFound => Error.Create("Wallet.NotFound", "Wallet not found");
        public static Error NotFoundForUser(Guid userId) => Error.Create("Wallet.NotFound", $"Wallet for user {userId} was not found");
        public static Error AlreadyExists => Error.Create("Wallet.AlreadyExists", "Wallet already exists for this user");
        public static Error Busy => Error.Create("Wallet.Busy", "Wallet is currently busy. Please try again.");
        public static Error InsufficientBalance => Error.Create("Wallet.InsufficientBalance", "Insufficient wallet balance");
        public static Error InsufficientBalanceWithDetails(decimal required, decimal available) => Error.Create("Wallet.InsufficientBalance", $"Insufficient balance. Required: {required:C}, Available: {available:C}");
        public static Error InsufficientHeldAmount => Error.Create("Wallet.InsufficientHeldAmount", "Insufficient held amount to release");
        public static Error ConcurrencyConflict => Error.Create("Wallet.ConcurrencyConflict", "Wallet was modified by another operation. Please try again.");
        public static Error CreateFailed(string reason) => Error.Create("Wallet.CreateFailed", $"Failed to create wallet: {reason}");
    }

    public static class Payment
    {
        public static Error Failed(string reason) => Error.Create("Payment.Failed", $"Payment failed: {reason}");
        public static Error ProcessingFailed(string reason) => Error.Create("Payment.ProcessingFailed", $"Failed to process payment: {reason}");
        public static Error RefundFailed(string reason) => Error.Create("Payment.RefundFailed", $"Failed to refund payment: {reason}");
        public static Error InvalidAmount => Error.Create("Payment.InvalidAmount", "Invalid payment amount");
    }

    public static class Stripe
    {
        public static Error SessionCreationFailed(string reason) => Error.Create("Stripe.SessionCreationFailed", $"Failed to create Stripe session: {reason}");
        public static Error WebhookProcessingFailed(string reason) => Error.Create("Stripe.WebhookProcessingFailed", $"Failed to process Stripe webhook: {reason}");
        public static Error InvalidWebhookSignature => Error.Create("Stripe.InvalidWebhookSignature", "Invalid webhook signature");
    }

    public static class Transaction
    {
        public static Error NotFound => Error.Create("Transaction.NotFound", "Transaction not found");
        public static Error Failed(string reason) => Error.Create("Transaction.Failed", $"Transaction failed: {reason}");
    }
}
