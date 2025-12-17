using Stripe;
using Stripe.Checkout;

namespace PaymentService.Application.Interfaces;

public interface IStripePaymentService
{
    Task<PaymentIntent> CreatePaymentIntentAsync(
        long amountInCents, 
        string currency, 
        string customerId, 
        Dictionary<string, string> metadata = null,
        CancellationToken cancellationToken = default);
    
    Task<PaymentIntent> GetPaymentIntentAsync(
        string paymentIntentId, 
        CancellationToken cancellationToken = default);
    
    Task<PaymentIntent> ConfirmPaymentIntentAsync(
        string paymentIntentId, 
        CancellationToken cancellationToken = default);
    
    Task<PaymentIntent> CancelPaymentIntentAsync(
        string paymentIntentId, 
        CancellationToken cancellationToken = default);
    
    Task<Session> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<Customer> CreateCustomerAsync(
        string email, 
        string name, 
        CancellationToken cancellationToken = default);
    
    Task<Customer> GetCustomerByEmailAsync(
        string email, 
        CancellationToken cancellationToken = default);
    
    Task<Refund> CreateRefundAsync(
        string paymentIntentId, 
        long? amountInCents = null, 
        CancellationToken cancellationToken = default);
    
    Task HandleWebhookAsync(
        string json, 
        string stripeSignature, 
        CancellationToken cancellationToken = default);
}

public class CreateCheckoutSessionRequest
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public long AmountInCents { get; set; }
    public string Currency { get; set; } = "usd";
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; }
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}
