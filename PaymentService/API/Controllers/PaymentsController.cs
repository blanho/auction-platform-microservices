using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Interfaces;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IStripePaymentService _stripePaymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IStripePaymentService stripePaymentService,
        ILogger<PaymentsController> logger)
    {
        _stripePaymentService = stripePaymentService;
        _logger = logger;
    }

    [HttpPost("payment-intent")]
    public async Task<ActionResult<CreatePaymentIntentResponse>> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentRequest request,
        CancellationToken cancellationToken)
    {
        var paymentIntent = await _stripePaymentService.CreatePaymentIntentAsync(
            request.AmountInCents,
            request.Currency,
            request.CustomerId,
            request.Metadata,
            cancellationToken);

        return Ok(new CreatePaymentIntentResponse
        {
            PaymentIntentId = paymentIntent.Id,
            ClientSecret = paymentIntent.ClientSecret,
            Status = paymentIntent.Status,
        });
    }

    [HttpGet("payment-intent/{paymentIntentId}")]
    public async Task<ActionResult<PaymentIntentResponse>> GetPaymentIntent(
        string paymentIntentId,
        CancellationToken cancellationToken)
    {
        var paymentIntent = await _stripePaymentService.GetPaymentIntentAsync(paymentIntentId, cancellationToken);

        return Ok(new PaymentIntentResponse
        {
            PaymentIntentId = paymentIntent.Id,
            Status = paymentIntent.Status,
            Amount = paymentIntent.Amount,
            Currency = paymentIntent.Currency,
        });
    }

    [HttpPost("checkout-session")]
    public async Task<ActionResult<CheckoutSessionResponse>> CreateCheckoutSession(
        [FromBody] CheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var session = await _stripePaymentService.CreateCheckoutSessionAsync(
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

        return Ok(new CheckoutSessionResponse
        {
            SessionId = session.Id,
            Url = session.Url,
        });
    }

    [HttpPost("customer")]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var existingCustomer = await _stripePaymentService.GetCustomerByEmailAsync(request.Email, cancellationToken);

        if (existingCustomer != null)
        {
            return Ok(new CustomerResponse
            {
                CustomerId = existingCustomer.Id,
                Email = existingCustomer.Email,
                Name = existingCustomer.Name,
            });
        }

        var customer = await _stripePaymentService.CreateCustomerAsync(request.Email, request.Name, cancellationToken);

        return Ok(new CustomerResponse
        {
            CustomerId = customer.Id,
            Email = customer.Email,
            Name = customer.Name,
        });
    }

    [HttpPost("refund")]
    public async Task<ActionResult<RefundResponse>> CreateRefund(
        [FromBody] CreateRefundRequest request,
        CancellationToken cancellationToken)
    {
        var refund = await _stripePaymentService.CreateRefundAsync(
            request.PaymentIntentId,
            request.AmountInCents,
            cancellationToken);

        return Ok(new RefundResponse
        {
            RefundId = refund.Id,
            Status = refund.Status,
            Amount = refund.Amount,
        });
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(cancellationToken);
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

        try
        {
            await _stripePaymentService.HandleWebhookAsync(json, stripeSignature, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return BadRequest();
        }
    }
}

public record CreatePaymentIntentRequest
{
    public long AmountInCents { get; init; }
    public string Currency { get; init; } = "usd";
    public string CustomerId { get; init; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; init; }
}

public record CreatePaymentIntentResponse
{
    public string PaymentIntentId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public record PaymentIntentResponse
{
    public string PaymentIntentId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public long? Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}

public record CheckoutSessionRequest
{
    public string CustomerId { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public long AmountInCents { get; init; }
    public string Currency { get; init; } = "usd";
    public string ProductName { get; init; } = string.Empty;
    public string ProductDescription { get; init; } = string.Empty;
    public string? ProductImageUrl { get; init; }
    public string SuccessUrl { get; init; } = string.Empty;
    public string CancelUrl { get; init; } = string.Empty;
    public Dictionary<string, string> Metadata { get; init; } = new();
}

public record CheckoutSessionResponse
{
    public string SessionId { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}

public record CreateCustomerRequest
{
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

public record CustomerResponse
{
    public string CustomerId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

public record CreateRefundRequest
{
    public string PaymentIntentId { get; init; } = string.Empty;
    public long? AmountInCents { get; init; }
}

public record RefundResponse
{
    public string RefundId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public long? Amount { get; init; }
}
