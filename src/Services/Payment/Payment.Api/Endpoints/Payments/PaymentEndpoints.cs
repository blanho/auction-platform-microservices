using Carter;
using BuildingBlocks.Web.Helpers;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Constants;
using Payment.Domain.Enums;
using Payment.Infrastructure.Constants;
using Stripe;

namespace Payment.Api.Endpoints.Payments;

public class PaymentEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/payments")
            .WithTags("Payments")
            .RequireAuthorization();

        group.MapPost("/orders/{orderId:guid}/checkout-session", CreateOrderCheckoutSession)
            .WithName("CreateOrderCheckoutSession")
            .WithSummary("Create a Stripe checkout session from a buyer-owned order");
    }

    private static async Task<IResult> CreateOrderCheckoutSession(
        Guid orderId,
        HttpContext httpContext,
        IOrderRepository orderRepository,
        IPaymentGateway paymentGateway,
        IConfiguration configuration,
        ILogger<PaymentEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return Results.NotFound(ProblemDetailsHelper.NotFound("Order", orderId));
        }

        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        if (order.BuyerId != userId)
        {
            return Results.Forbid();
        }

        if (order.PaymentStatus != PaymentStatus.Pending ||
            order.Status is not (OrderStatus.Pending or OrderStatus.PaymentPending))
        {
            return Results.BadRequest(ProblemDetailsHelper.ValidationError(
                $"Order cannot be paid while it is in {order.Status} status"));
        }

        if (string.IsNullOrWhiteSpace(order.ShippingAddress))
        {
            return Results.BadRequest(ProblemDetailsHelper.ValidationError(
                "A shipping address is required before payment"));
        }

        var email = UserHelper.GetEmail(httpContext.User);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest(ProblemDetailsHelper.ValidationError(
                "An email address is required before payment"));
        }

        if (!TryGetFrontendBaseUrl(configuration["FrontendUrl"], out var frontendBaseUrl))
        {
            logger.LogError("FrontendUrl is missing or is not a valid HTTP(S) URL");
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Payment configuration error");
        }

        long amountInCents;
        try
        {
            amountInCents = checked(decimal.ToInt64(
                decimal.Round(order.TotalAmount * 100m, 0, MidpointRounding.AwayFromZero)));
        }
        catch (OverflowException)
        {
            logger.LogError("Order {OrderId} amount is outside the supported Stripe range", order.Id);
            return Results.BadRequest(ProblemDetailsHelper.ValidationError(
                "The order amount cannot be processed"));
        }

        if (amountInCents <= 0)
        {
            return Results.BadRequest(ProblemDetailsHelper.ValidationError(
                "The order amount must be greater than zero"));
        }

        try
        {
            var session = await paymentGateway.CreateCheckoutSessionAsync(
                new CreateCheckoutSessionRequest
                {
                    CustomerEmail = email,
                    AmountInCents = amountInCents,
                    Currency = WalletDefaults.DefaultCurrency,
                    ProductName = order.ItemTitle,
                    ProductDescription = $"Auction order {order.Id}",
                    SuccessUrl = $"{frontendBaseUrl}/payment/success?order_id={order.Id}&session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{frontendBaseUrl}/payment/cancel?order_id={order.Id}&auction_id={order.AuctionId}",
                    Metadata = new Dictionary<string, string>
                    {
                        [StripeMetadataKeys.OrderId] = order.Id.ToString(),
                        [StripeMetadataKeys.UserId] = userId.ToString(),
                        [StripeMetadataKeys.Username] = order.BuyerUsername
                    },
                },
                cancellationToken);

            return Results.Ok(new CheckoutSessionResponseDto
            {
                SessionId = session.Id,
                Url = session.Url,
            });
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Stripe rejected checkout creation for order {OrderId}", order.Id);
            return Results.BadRequest(new { error = ex.StripeError?.Message ?? ex.Message });
        }
    }

    private static bool TryGetFrontendBaseUrl(string? value, out string baseUrl)
    {
        baseUrl = string.Empty;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return false;
        }

        baseUrl = uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        return true;
    }
}
