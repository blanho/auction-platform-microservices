using Stripe;
using Stripe.Checkout;

namespace UtilityService.Services;

public interface IStripePaymentService
{
    Task<PaymentIntent> CreatePaymentIntentAsync(long amountInCents, string currency, string customerId, Dictionary<string, string>? metadata = null);
    Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);
    Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId);
    Task<PaymentIntent> CancelPaymentIntentAsync(string paymentIntentId);
    Task<Session> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request);
    Task<Customer> CreateCustomerAsync(string email, string name);
    Task<Customer?> GetCustomerByEmailAsync(string email);
    Task<Refund> CreateRefundAsync(string paymentIntentId, long? amountInCents = null);
    Task HandleWebhookAsync(string json, string stripeSignature);
}

public class CreateCheckoutSessionRequest
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public long AmountInCents { get; set; }
    public string Currency { get; set; } = "usd";
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class StripePaymentService : IStripePaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly string _webhookSecret;

    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var secretKey = _configuration["Stripe:SecretKey"] 
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured");
        _webhookSecret = _configuration["Stripe:WebhookSecret"] ?? string.Empty;
        
        StripeConfiguration.ApiKey = secretKey;
    }

    public async Task<PaymentIntent> CreatePaymentIntentAsync(
        long amountInCents, 
        string currency, 
        string customerId,
        Dictionary<string, string>? metadata = null)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = currency.ToLower(),
            Customer = customerId,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
            Metadata = metadata,
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);
        
        _logger.LogInformation("Created PaymentIntent {PaymentIntentId} for {Amount} {Currency}", 
            paymentIntent.Id, amountInCents, currency);
        
        return paymentIntent;
    }

    public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        return await service.GetAsync(paymentIntentId);
    }

    public async Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        return await service.ConfirmAsync(paymentIntentId);
    }

    public async Task<PaymentIntent> CancelPaymentIntentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        return await service.CancelAsync(paymentIntentId);
    }

    public async Task<Session> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request)
    {
        var lineItems = new List<SessionLineItemOptions>
        {
            new()
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = request.AmountInCents,
                    Currency = request.Currency.ToLower(),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = request.ProductName,
                        Description = request.ProductDescription,
                        Images = string.IsNullOrEmpty(request.ProductImageUrl) 
                            ? null 
                            : new List<string> { request.ProductImageUrl },
                    },
                },
                Quantity = 1,
            },
        };

        var options = new SessionCreateOptions
        {
            Customer = string.IsNullOrEmpty(request.CustomerId) ? null : request.CustomerId,
            CustomerEmail = string.IsNullOrEmpty(request.CustomerId) ? request.CustomerEmail : null,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            Metadata = request.Metadata,
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = request.Metadata,
            },
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        
        _logger.LogInformation("Created Checkout Session {SessionId}", session.Id);
        
        return session;
    }

    public async Task<Customer> CreateCustomerAsync(string email, string name)
    {
        var options = new CustomerCreateOptions
        {
            Email = email,
            Name = name,
        };

        var service = new CustomerService();
        var customer = await service.CreateAsync(options);
        
        _logger.LogInformation("Created Stripe Customer {CustomerId} for {Email}", customer.Id, email);
        
        return customer;
    }

    public async Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        var service = new CustomerService();
        var options = new CustomerListOptions
        {
            Email = email,
            Limit = 1,
        };

        var customers = await service.ListAsync(options);
        return customers.Data.FirstOrDefault();
    }

    public async Task<Refund> CreateRefundAsync(string paymentIntentId, long? amountInCents = null)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
            Amount = amountInCents,
        };

        var service = new RefundService();
        var refund = await service.CreateAsync(options);
        
        _logger.LogInformation("Created Refund {RefundId} for PaymentIntent {PaymentIntentId}", 
            refund.Id, paymentIntentId);
        
        return refund;
    }

    public async Task HandleWebhookAsync(string json, string stripeSignature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _webhookSecret);
            
            _logger.LogInformation("Processing Stripe webhook event: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentIntentSucceeded(stripeEvent);
                    break;
                case "payment_intent.payment_failed":
                    await HandlePaymentIntentFailed(stripeEvent);
                    break;
                case "checkout.session.completed":
                    await HandleCheckoutSessionCompleted(stripeEvent);
                    break;
                default:
                    _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook error");
            throw;
        }
    }

    private Task HandlePaymentIntentSucceeded(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        _logger.LogInformation("PaymentIntent succeeded: {PaymentIntentId}", paymentIntent?.Id);
        return Task.CompletedTask;
    }

    private Task HandlePaymentIntentFailed(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        _logger.LogWarning("PaymentIntent failed: {PaymentIntentId}", paymentIntent?.Id);
        return Task.CompletedTask;
    }

    private Task HandleCheckoutSessionCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        _logger.LogInformation("Checkout session completed: {SessionId}", session?.Id);
        return Task.CompletedTask;
    }
}
