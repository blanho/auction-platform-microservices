namespace Payment.Application.DTOs;

public record CheckoutSessionResponseDto
{
    public string SessionId { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
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
