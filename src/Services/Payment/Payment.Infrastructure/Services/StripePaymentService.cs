using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Web.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Stripe;
using Stripe.Checkout;

namespace Payment.Infrastructure.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _webhookSecret;

    public StripePaymentService(
        IConfiguration configuration,
        ILogger<StripePaymentService> logger,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _logger = logger;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;

        var secretKey = _configuration["Stripe:SecretKey"]
            ?? throw new ConfigurationException("Stripe:SecretKey is not configured");
        _webhookSecret = _configuration["Stripe:WebhookSecret"] 
            ?? throw new ConfigurationException(
                "Stripe:WebhookSecret must be configured. " +
                "Get your webhook signing secret from the Stripe Dashboard: " +
                "https://dashboard.stripe.com/webhooks");
        
        if (_webhookSecret.Length < 20 || !_webhookSecret.StartsWith("whsec_"))
        {
            throw new ConfigurationException(
                "Stripe:WebhookSecret appears to be invalid. " +
                "It should start with 'whsec_' and be obtained from the Stripe Dashboard.");
        }

        StripeConfiguration.ApiKey = secretKey;
    }

    public async Task<PaymentIntent> CreatePaymentIntentAsync(
        long amountInCents,
        string currency,
        string customerId,
        Dictionary<string, string> metadata = null,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = metadata?.GetValueOrDefault("orderId") ?? Guid.NewGuid().ToString();

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

        var requestOptions = new RequestOptions
        {
            IdempotencyKey = $"pi-{idempotencyKey}"
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options, requestOptions, cancellationToken);

        _logger.LogInformation("Created PaymentIntent {PaymentIntentId} for {Amount} {Currency} with idempotency key {IdempotencyKey}",
            paymentIntent.Id, amountInCents, currency, idempotencyKey);

        return paymentIntent;
    }

    public async Task<PaymentIntent> GetPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();
        return await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
    }

    public async Task<PaymentIntent> ConfirmPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();
        return await service.ConfirmAsync(paymentIntentId, cancellationToken: cancellationToken);
    }

    public async Task<PaymentIntent> CancelPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();
        return await service.CancelAsync(paymentIntentId, cancellationToken: cancellationToken);
    }

    public async Task<Session> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
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
        var idempotencyKey = request.Metadata?.GetValueOrDefault("orderId") ?? Guid.NewGuid().ToString();
        var requestOptions = new RequestOptions
        {
            IdempotencyKey = $"cs-{idempotencyKey}"
        };
        var session = await service.CreateAsync(options, requestOptions, cancellationToken);

        _logger.LogInformation("Created Checkout Session {SessionId} with idempotency key {IdempotencyKey}", 
            session.Id, idempotencyKey);

        return session;
    }

    public async Task<Customer> CreateCustomerAsync(
        string email,
        string name,
        CancellationToken cancellationToken = default)
    {
        var options = new CustomerCreateOptions
        {
            Email = email,
            Name = name,
        };

        var service = new CustomerService();
        var customer = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation("Created Stripe Customer {CustomerId} for {Email}", customer.Id, email);

        return customer;
    }

    public async Task<Customer> GetCustomerByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var service = new CustomerService();
        var options = new CustomerListOptions
        {
            Email = email,
            Limit = 1,
        };

        var customers = await service.ListAsync(options, cancellationToken: cancellationToken);
        return customers.Data.FirstOrDefault();
    }

    public async Task<Refund> CreateRefundAsync(
        string paymentIntentId,
        long? amountInCents = null,
        CancellationToken cancellationToken = default)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
            Amount = amountInCents,
        };

        var requestOptions = new RequestOptions
        {
            IdempotencyKey = $"refund-{paymentIntentId}-{amountInCents ?? 0}"
        };

        var service = new RefundService();
        var refund = await service.CreateAsync(options, requestOptions, cancellationToken);

        _logger.LogInformation("Created Refund {RefundId} for PaymentIntent {PaymentIntentId} with idempotency key",
            refund.Id, paymentIntentId);

        return refund;
    }

    public async Task HandleWebhookAsync(
        string json,
        string stripeSignature,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _webhookSecret);

            _logger.LogInformation("Processing Stripe webhook event: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentIntentSucceeded(stripeEvent, cancellationToken);
                    break;
                case "payment_intent.payment_failed":
                    await HandlePaymentIntentFailed(stripeEvent, cancellationToken);
                    break;
                case "checkout.session.completed":
                    await HandleCheckoutSessionCompleted(stripeEvent, cancellationToken);
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

    private async Task HandlePaymentIntentSucceeded(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent) return;

        _logger.LogInformation("PaymentIntent succeeded: {PaymentIntentId}", paymentIntent.Id);

        if (paymentIntent.Metadata.TryGetValue("orderId", out var orderIdStr) &&
            Guid.TryParse(orderIdStr, out var orderId))
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                order.CompletePayment(paymentIntent.Id);
                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Order {OrderId} marked as paid", orderId);
            }
        }
    }

    private async Task HandlePaymentIntentFailed(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent) return;

        _logger.LogWarning("PaymentIntent failed: {PaymentIntentId}", paymentIntent.Id);

        if (paymentIntent.Metadata.TryGetValue("orderId", out var orderIdStr) &&
            Guid.TryParse(orderIdStr, out var orderId))
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                order.ChangeStatus(OrderStatus.Cancelled);
                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Order {OrderId} marked as payment failed", orderId);
            }
        }
    }

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not Session session) return;

        _logger.LogInformation("Checkout session completed: {SessionId}", session.Id);

        if (session.Metadata.TryGetValue("orderId", out var orderIdStr) &&
            Guid.TryParse(orderIdStr, out var orderId))
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                order.CompletePayment(session.PaymentIntentId);
                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Order {OrderId} marked as paid via checkout session", orderId);
            }
        }
    }
}
