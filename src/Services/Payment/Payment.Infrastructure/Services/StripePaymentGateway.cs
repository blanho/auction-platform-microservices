using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Infrastructure.Configuration;
using Payment.Infrastructure.Constants;
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
            PaymentMethodTypes = new List<string> { StripePaymentMethodTypes.Card },
            LineItems = lineItems,
            Mode = StripePaymentModes.Payment,
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            Metadata = request.Metadata,
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = request.Metadata,
            },
        };

        var service = _stripeFactory.CreateSessionService();
        var idempotencyKey = request.Metadata.GetValueOrDefault(StripeMetadataKeys.OrderId)
            ?? Guid.NewGuid().ToString();
        var session = await service.CreateAsync(
            options,
            new RequestOptions { IdempotencyKey = $"cs-{idempotencyKey}" },
            cancellationToken);

        _logger.LogInformation("Created Checkout Session {SessionId}", session.Id);

        return MapToCheckoutSessionResult(session);
    }

    public async Task HandleWebhookAsync(
        string json,
        string signature,
        CancellationToken cancellationToken = default)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, signature, _options.WebhookSecret);

        _logger.LogInformation(
            "Processing Stripe webhook event {StripeEventId} of type {EventType}",
            stripeEvent.Id,
            stripeEvent.Type);

        switch (stripeEvent.Type)
        {
            case StripeEventTypes.PaymentIntentSucceeded:
                await HandlePaymentIntentSucceeded(stripeEvent, cancellationToken);
                break;
            case StripeEventTypes.PaymentIntentPaymentFailed:
                HandlePaymentIntentFailed(stripeEvent);
                break;
            case StripeEventTypes.CheckoutSessionCompleted:
                await HandleCheckoutSessionCompleted(stripeEvent, cancellationToken);
                break;
            default:
                _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                break;
        }
    }

    private async Task HandlePaymentIntentSucceeded(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent) return;

        _logger.LogInformation("PaymentIntent succeeded: {PaymentIntentId}", paymentIntent.Id);

        var order = await ResolveOrderFromMetadata(paymentIntent.Metadata, cancellationToken);
        if (order == null) return;

        if (!MatchesOrderAmount(order, paymentIntent.AmountReceived, paymentIntent.Currency))
        {
            _logger.LogError(
                "Rejected Stripe payment amount mismatch for order {OrderId}, PaymentIntent {PaymentIntentId}",
                order.Id,
                paymentIntent.Id);
            return;
        }

        if (!order.CompletePayment(paymentIntent.Id))
        {
            _logger.LogInformation(
                "Ignoring duplicate payment success for order {OrderId}, PaymentIntent {PaymentIntentId}",
                order.Id,
                paymentIntent.Id);
            return;
        }

        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} marked as paid", order.Id);
    }

    private void HandlePaymentIntentFailed(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent) return;

        _logger.LogWarning(
            "PaymentIntent {PaymentIntentId} failed; the order remains payable for a retry",
            paymentIntent.Id);
    }

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not Session session) return;

        _logger.LogInformation("Checkout session completed: {SessionId}", session.Id);

        if (!string.Equals(session.PaymentStatus, StripePaymentStatuses.Paid, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Ignoring incomplete checkout session {SessionId} with payment status {PaymentStatus}",
                session.Id,
                session.PaymentStatus);
            return;
        }

        var order = await ResolveOrderFromMetadata(session.Metadata, cancellationToken);
        if (order == null) return;

        if (!MatchesOrderAmount(order, session.AmountTotal, session.Currency))
        {
            _logger.LogError(
                "Rejected Stripe checkout amount mismatch for order {OrderId}, Session {SessionId}",
                order.Id,
                session.Id);
            return;
        }

        if (!order.CompletePayment(session.PaymentIntentId))
        {
            _logger.LogInformation(
                "Ignoring duplicate checkout completion for order {OrderId}, Session {SessionId}",
                order.Id,
                session.Id);
            return;
        }

        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} marked as paid via checkout session", order.Id);
    }

    private async Task<Order?> ResolveOrderFromMetadata(
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken)
    {
        if (!metadata.TryGetValue(StripeMetadataKeys.OrderId, out var orderIdStr) ||
            !Guid.TryParse(orderIdStr, out var orderId))
            return null;

        return await _orderRepository.GetByIdAsync(orderId);
    }

    private static bool MatchesOrderAmount(Order order, long? amountInCents, string? currency)
    {
        long expectedAmount;
        try
        {
            expectedAmount = checked(decimal.ToInt64(
                decimal.Round(order.TotalAmount * 100m, 0, MidpointRounding.AwayFromZero)));
        }
        catch (OverflowException)
        {
            return false;
        }

        return amountInCents == expectedAmount &&
               string.Equals(currency, WalletDefaults.DefaultCurrency, StringComparison.OrdinalIgnoreCase);
    }

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

}
