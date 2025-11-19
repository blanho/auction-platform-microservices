namespace Payment.Application.DTOs;

public record CreatePaymentIntentRequestDto
{
    public long AmountInCents { get; init; }
    public string Currency { get; init; } = "usd";
    public string CustomerId { get; init; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; init; }
}

public record CreatePaymentIntentResponseDto
{
    public string PaymentIntentId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public record PaymentIntentResponseDto
{
    public string PaymentIntentId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public long? Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}

public record CheckoutSessionRequestDto
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

public record CheckoutSessionResponseDto
{
    public string SessionId { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}

public record CreateCustomerRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

public record CustomerResponseDto
{
    public string CustomerId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

public record CreateRefundRequestDto
{
    public string PaymentIntentId { get; init; } = string.Empty;
    public long? AmountInCents { get; init; }
}

public record RefundResponseDto
{
    public string RefundId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public long? Amount { get; init; }
}

public class CreateCheckoutSessionRequest
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public long AmountInCents { get; set; }
    public string Currency { get; set; } = "usd";
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}
