using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Infrastructure.Configuration;
using BuildingBlocks.Infrastructure.Repository;
using Stripe;
using Stripe.Checkout;

namespace Payment.Infrastructure.Services;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly IStripeServiceFactory _stripeFactory;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly StripeOptions _options;
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(
        IStripeServiceFactory stripeFactory,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IOptions<StripeOptions> options,
        ILogger<StripePaymentGateway> logger)
    {
        _stripeFactory = stripeFactory;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        long amountInCents,
        string currency,
        string customerId,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
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

        var service = _stripeFactory.CreatePaymentIntentService();
        var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation("Created PaymentIntent {PaymentIntentId} for {Amount} {Currency}",
            paymentIntent.Id, amountInCents, currency);

        return MapToPaymentIntentResult(paymentIntent);
    }

    public async Task<PaymentIntentResult?> GetPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        var service = _stripeFactory.CreatePaymentIntentService();
        var paymentIntent = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
        return paymentIntent != null ? MapToPaymentIntentResult(paymentIntent) : null;
    }

    public async Task<PaymentIntentResult> ConfirmPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        var service = _stripeFactory.CreatePaymentIntentService();
        var paymentIntent = await service.ConfirmAsync(paymentIntentId, cancellationToken: cancellationToken);
        return MapToPaymentIntentResult(paymentIntent);
    }

    public async Task<PaymentIntentResult> CancelPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        var service = _stripeFactory.CreatePaymentIntentService();
        var paymentIntent = await service.CancelAsync(paymentIntentId, cancellationToken: cancellationToken);
        return MapToPaymentIntentResult(paymentIntent);
    }

    public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
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

        var service = _stripeFactory.CreateSessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation("Created Checkout Session {SessionId}", session.Id);

        return MapToCheckoutSessionResult(session);
    }

    public async Task<CustomerResult> CreateCustomerAsync(
        string email,
        string name,
        CancellationToken cancellationToken = default)
    {
        var options = new CustomerCreateOptions
        {
            Email = email,
            Name = name,
        };

        var service = _stripeFactory.CreateCustomerService();
        var customer = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation("Created Stripe Customer {CustomerId} for {Email}", customer.Id, email);

        return MapToCustomerResult(customer);
    }

    public async Task<CustomerResult?> GetCustomerByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var service = _stripeFactory.CreateCustomerService();
        var options = new CustomerListOptions
        {
            Email = email,
            Limit = 1,
        };

        var customers = await service.ListAsync(options, cancellationToken: cancellationToken);
        var customer = customers.Data.FirstOrDefault();
        return customer != null ? MapToCustomerResult(customer) : null;
    }

    public async Task<RefundResult> CreateRefundAsync(
        string paymentIntentId,
        long? amountInCents = null,
        CancellationToken cancellationToken = default)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
            Amount = amountInCents,
        };

        var service = _stripeFactory.CreateRefundService();
        var refund = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation("Created Refund {RefundId} for PaymentIntent {PaymentIntentId}",
            refund.Id, paymentIntentId);

        return MapToRefundResult(refund);
    }

    public async Task HandleWebhookAsync(
        string json,
        string signature,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, signature, _options.WebhookSecret);

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

    private static PaymentIntentResult MapToPaymentIntentResult(PaymentIntent pi) => new()
    {
        Id = pi.Id,
        AmountInCents = pi.Amount,
        Currency = pi.Currency,
        Status = pi.Status,
        ClientSecret = pi.ClientSecret,
        CustomerId = pi.CustomerId,
        Metadata = pi.Metadata?.ToDictionary(k => k.Key, v => v.Value) ?? new(),
        CreatedAt = pi.Created
    };

    private static CheckoutSessionResult MapToCheckoutSessionResult(Session s) => new()
    {
        Id = s.Id,
        Url = s.Url,
        Status = s.Status,
        PaymentIntentId = s.PaymentIntentId,
        CustomerId = s.CustomerId,
        AmountTotal = s.AmountTotal ?? 0,
        Currency = s.Currency,
        Metadata = s.Metadata?.ToDictionary(k => k.Key, v => v.Value) ?? new()
    };

    private static CustomerResult MapToCustomerResult(Customer c) => new()
    {
        Id = c.Id,
        Email = c.Email,
        Name = c.Name,
        CreatedAt = c.Created
    };

    private static RefundResult MapToRefundResult(Refund r) => new()
    {
        Id = r.Id,
        PaymentIntentId = r.PaymentIntentId,
        AmountInCents = r.Amount,
        Status = r.Status,
        Reason = r.Reason,
        CreatedAt = r.Created
    };
}
