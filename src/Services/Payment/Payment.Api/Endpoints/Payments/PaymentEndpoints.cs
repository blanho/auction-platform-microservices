using Carter;
using BuildingBlocks.Web.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

namespace Payment.Api.Endpoints.Payments;

public class PaymentEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/payments")
            .WithTags("Payments")
            .RequireAuthorization();

        group.MapPost("/payment-intent", CreatePaymentIntent)
            .WithName("CreatePaymentIntent")
            .WithSummary("Create a Stripe payment intent (idempotent with Idempotency-Key header)")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Payments.Process));

        group.MapGet("/payment-intent/{paymentIntentId}", GetPaymentIntent)
            .WithName("GetPaymentIntent")
            .WithSummary("Get a payment intent by ID")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Payments.View));

        group.MapPost("/checkout-session", CreateCheckoutSession)
            .WithName("CreateCheckoutSession")
            .WithSummary("Create a Stripe checkout session (idempotent with Idempotency-Key header)")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Payments.Process));

        group.MapPost("/customer", CreateCustomer)
            .WithName("CreateStripeCustomer")
            .WithSummary("Create or get existing Stripe customer")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Payments.Process));

        group.MapPost("/refund", CreateRefund)
            .WithName("CreateRefund")
            .WithSummary("Create a refund for a payment (idempotent with Idempotency-Key header)")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Payments.Refund));
    }

    private static async Task<Results<Ok<CreatePaymentIntentResponseDto>, BadRequest<object>>> CreatePaymentIntent(
        CreatePaymentIntentRequestDto request,
        IStripePaymentService stripePaymentService,
        CancellationToken cancellationToken)
    {
        var paymentIntent = await stripePaymentService.CreatePaymentIntentAsync(
            request.AmountInCents, request.Currency, request.CustomerId, request.Metadata, cancellationToken);

        var response = new CreatePaymentIntentResponseDto
        {
            PaymentIntentId = paymentIntent.Id,
            ClientSecret = paymentIntent.ClientSecret,
            Status = paymentIntent.Status,
        };

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<PaymentIntentResponseDto>> GetPaymentIntent(
        string paymentIntentId,
        IStripePaymentService stripePaymentService,
        CancellationToken cancellationToken)
    {
        var paymentIntent = await stripePaymentService.GetPaymentIntentAsync(paymentIntentId, cancellationToken);

        return TypedResults.Ok(new PaymentIntentResponseDto
        {
            PaymentIntentId = paymentIntent.Id,
            Status = paymentIntent.Status,
            Amount = paymentIntent.Amount,
            Currency = paymentIntent.Currency,
        });
    }

    private static async Task<Ok<CheckoutSessionResponseDto>> CreateCheckoutSession(
        CheckoutSessionRequestDto request,
        IStripePaymentService stripePaymentService,
        CancellationToken cancellationToken)
    {
        var session = await stripePaymentService.CreateCheckoutSessionAsync(
            new CreateCheckoutSessionRequest
            {
                CustomerId = request.CustomerId,
                CustomerEmail = request.CustomerEmail,
                AmountInCents = request.AmountInCents,
                Currency = request.Currency,
                ProductName = request.ProductName,
                ProductDescription = request.ProductDescription,
                ProductImageUrl = request.ProductImageUrl,
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                Metadata = request.Metadata,
            },
            cancellationToken);

        return TypedResults.Ok(new CheckoutSessionResponseDto
        {
            SessionId = session.Id,
            Url = session.Url,
        });
    }

    private static async Task<Ok<CustomerResponseDto>> CreateCustomer(
        CreateCustomerRequestDto request,
        IStripePaymentService stripePaymentService,
        CancellationToken cancellationToken)
    {
        var existingCustomer = await stripePaymentService.GetCustomerByEmailAsync(request.Email, cancellationToken);

        if (existingCustomer != null)
        {
            return TypedResults.Ok(new CustomerResponseDto
            {
                CustomerId = existingCustomer.Id,
                Email = existingCustomer.Email,
                Name = existingCustomer.Name,
            });
        }

        var customer = await stripePaymentService.CreateCustomerAsync(request.Email, request.Name, cancellationToken);

        return TypedResults.Ok(new CustomerResponseDto
        {
            CustomerId = customer.Id,
            Email = customer.Email,
            Name = customer.Name,
        });
    }

    private static async Task<Ok<RefundResponseDto>> CreateRefund(
        CreateRefundRequestDto request,
        IStripePaymentService stripePaymentService,
        CancellationToken cancellationToken)
    {
        var refund = await stripePaymentService.CreateRefundAsync(
            request.PaymentIntentId,
            request.AmountInCents,
            cancellationToken);

        return TypedResults.Ok(new RefundResponseDto
        {
            RefundId = refund.Id,
            Status = refund.Status,
            Amount = refund.Amount,
        });
    }
}
