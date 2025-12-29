using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;

namespace PaymentService.API.Endpoints.Payments;

public class PaymentEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/payments")
            .WithTags("Payments")
            .RequireAuthorization();

        group.MapPost("/payment-intent", CreatePaymentIntent)
            .WithName("CreatePaymentIntent")
            .WithSummary("Create a Stripe payment intent");

        group.MapGet("/payment-intent/{paymentIntentId}", GetPaymentIntent)
            .WithName("GetPaymentIntent")
            .WithSummary("Get a payment intent by ID");

        group.MapPost("/checkout-session", CreateCheckoutSession)
            .WithName("CreateCheckoutSession")
            .WithSummary("Create a Stripe checkout session");

        group.MapPost("/customer", CreateCustomer)
            .WithName("CreateStripeCustomer")
            .WithSummary("Create or get existing Stripe customer");

        group.MapPost("/refund", CreateRefund)
            .WithName("CreateRefund")
            .WithSummary("Create a refund for a payment");
    }

    private static async Task<Ok<CreatePaymentIntentResponseDto>> CreatePaymentIntent(
        CreatePaymentIntentRequestDto request,
        IStripePaymentService stripePaymentService,
        CancellationToken cancellationToken)
    {
        var paymentIntent = await stripePaymentService.CreatePaymentIntentAsync(
            request.AmountInCents,
            request.Currency,
            request.CustomerId,
            request.Metadata,
            cancellationToken);

        return TypedResults.Ok(new CreatePaymentIntentResponseDto
        {
            PaymentIntentId = paymentIntent.Id,
            ClientSecret = paymentIntent.ClientSecret,
            Status = paymentIntent.Status,
        });
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
