using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Payment.Application.Interfaces;
using Stripe;

namespace Payment.API.Endpoints.Payments;

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

    private static async Task<IResult> HandleWebhook(
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
        catch (StripeException ex) when (ex.Message.Contains("signature") || ex.Message.Contains("Signature"))
        {
            logger.LogWarning(ex, "Invalid Stripe webhook signature - will not retry");
            return TypedResults.BadRequest();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error processing webhook - will retry");
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error processing Stripe webhook - will retry");
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
