namespace Payment.Application.DTOs;

public record CheckoutSessionResult
{
    public string Id { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? PaymentIntentId { get; init; }
    public string? CustomerId { get; init; }
    public long AmountTotal { get; init; }
    public string Currency { get; init; } = string.Empty;
    public Dictionary<string, string> Metadata { get; init; } = new();
}
