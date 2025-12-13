using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityService.Services;

namespace UtilityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IStripePaymentService _stripeService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IStripePaymentService stripeService, ILogger<PaymentsController> logger)
    {
        _stripeService = stripeService;
        _logger = logger;
    }

    [HttpPost("create-payment-intent")]
    [Authorize]
    public async Task<ActionResult<CreatePaymentIntentResponse>> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        var customerId = await GetOrCreateStripeCustomerId(request.CustomerEmail, request.CustomerName);
        
        var metadata = new Dictionary<string, string>
        {
            { "auctionId", request.AuctionId },
            { "buyerId", request.BuyerId },
            { "orderId", request.OrderId ?? string.Empty }
        };

        var paymentIntent = await _stripeService.CreatePaymentIntentAsync(
            request.AmountInCents,
            request.Currency,
            customerId,
            metadata);

        return Ok(new CreatePaymentIntentResponse(paymentIntent.ClientSecret, paymentIntent.Id));
    }

    [HttpPost("create-checkout-session")]
    [Authorize]
    public async Task<ActionResult<CheckoutSessionResponse>> CreateCheckoutSession([FromBody] CheckoutSessionRequest request)
    {
        var session = await _stripeService.CreateCheckoutSessionAsync(new CreateCheckoutSessionRequest
        {
            CustomerEmail = request.CustomerEmail,
            AmountInCents = request.AmountInCents,
            Currency = request.Currency,
            ProductName = request.ProductName,
            ProductDescription = request.ProductDescription,
            ProductImageUrl = request.ProductImageUrl,
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "auctionId", request.AuctionId },
                { "buyerId", request.BuyerId }
            }
        });

        return Ok(new CheckoutSessionResponse(session.Id, session.Url));
    }

    [HttpGet("payment-intent/{paymentIntentId}")]
    [Authorize]
    public async Task<ActionResult<PaymentIntentStatusResponse>> GetPaymentIntentStatus(string paymentIntentId)
    {
        var paymentIntent = await _stripeService.GetPaymentIntentAsync(paymentIntentId);
        
        return Ok(new PaymentIntentStatusResponse(paymentIntent.Id, paymentIntent.Status, paymentIntent.Amount, paymentIntent.Currency));
    }

    [HttpPost("refund")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<RefundResponse>> CreateRefund([FromBody] RefundRequest request)
    {
        var refund = await _stripeService.CreateRefundAsync(request.PaymentIntentId, request.AmountInCents);
        
        return Ok(new RefundResponse(refund.Id, refund.Status, refund.Amount));
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        if (string.IsNullOrEmpty(stripeSignature))
        {
            return BadRequest("Missing Stripe-Signature header");
        }

        try
        {
            await _stripeService.HandleWebhookAsync(json, stripeSignature);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Stripe webhook");
            return BadRequest();
        }
    }

    private async Task<string> GetOrCreateStripeCustomerId(string email, string name)
    {
        var existingCustomer = await _stripeService.GetCustomerByEmailAsync(email);
        if (existingCustomer != null)
        {
            return existingCustomer.Id;
        }

        var newCustomer = await _stripeService.CreateCustomerAsync(email, name);
        return newCustomer.Id;
    }
}

public record CreatePaymentIntentRequest(
    long AmountInCents,
    string Currency,
    string CustomerEmail,
    string CustomerName,
    string AuctionId,
    string BuyerId,
    string? OrderId
);

public record CreatePaymentIntentResponse(string ClientSecret, string PaymentIntentId)
{
    public string ClientSecret { get; init; } = ClientSecret;
    public string PaymentIntentId { get; init; } = PaymentIntentId;
}

public record CheckoutSessionRequest(
    string CustomerEmail,
    long AmountInCents,
    string Currency,
    string ProductName,
    string ProductDescription,
    string? ProductImageUrl,
    string SuccessUrl,
    string CancelUrl,
    string AuctionId,
    string BuyerId
);

public record CheckoutSessionResponse(string SessionId, string SessionUrl)
{
    public string SessionId { get; init; } = SessionId;
    public string SessionUrl { get; init; } = SessionUrl;
}

public record PaymentIntentStatusResponse(string Id, string Status, long Amount, string Currency)
{
    public string Id { get; init; } = Id;
    public string Status { get; init; } = Status;
    public long Amount { get; init; } = Amount;
    public string Currency { get; init; } = Currency;
}

public record RefundRequest(string PaymentIntentId, long? AmountInCents);

public record RefundResponse(string RefundId, string Status, long Amount)
{
    public string RefundId { get; init; } = RefundId;
    public string Status { get; init; } = Status;
    public long Amount { get; init; } = Amount;
}
