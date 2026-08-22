using Payment.Application.DTOs;

namespace Payment.Application.Interfaces;

public interface IPaymentGateway
{
    Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);

    Task HandleWebhookAsync(
        string json,
        string signature,
        CancellationToken cancellationToken = default);
}
