using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using PaymentService.Application.Interfaces;

namespace PaymentService.API.Endpoints.Payments;

public class PaymentWebhookEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/payments")
            .WithTags("Payment Webhooks");

        group.MapPost("/webhook", HandleWebhook)
            .WithName("StripeWebhook")
            .WithSummary("Handle Stripe webhook events")
            .AllowAnonymous();
    }

    private static async Task<Results<Ok, BadRequest>> HandleWebhook(
        HttpContext httpContext,
        IStripePaymentService stripePaymentService,
        ILogger<PaymentWebhookEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync(cancellationToken);
        var stripeSignature = httpContext.Request.Headers["Stripe-Signature"].ToString();

        try
        {
            await stripePaymentService.HandleWebhookAsync(json, stripeSignature, cancellationToken);
            return TypedResults.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Stripe webhook");
            return TypedResults.BadRequest();
        }
    }
}
