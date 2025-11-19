using Payment.Application.DTOs;

namespace Payment.Application.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        long amountInCents,
        string currency,
        string customerId,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    Task<PaymentIntentResult?> GetPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);

    Task<PaymentIntentResult> ConfirmPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);

    Task<PaymentIntentResult> CancelPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);

    Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerResult> CreateCustomerAsync(
        string email,
        string name,
        CancellationToken cancellationToken = default);

    Task<CustomerResult?> GetCustomerByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<RefundResult> CreateRefundAsync(
        string paymentIntentId,
        long? amountInCents = null,
        CancellationToken cancellationToken = default);

    Task HandleWebhookAsync(
        string json,
        string signature,
        CancellationToken cancellationToken = default);
}
