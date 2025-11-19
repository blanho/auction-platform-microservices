namespace Payment.Application.DTOs;

public record PaymentIntentResult
{
    public string Id { get; init; } = string.Empty;
    public long AmountInCents { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? ClientSecret { get; init; }
    public string? CustomerId { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
    public DateTimeOffset CreatedAt { get; init; }
}

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

public record CustomerResult
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record RefundResult
{
    public string Id { get; init; } = string.Empty;
    public string PaymentIntentId { get; init; } = string.Empty;
    public long AmountInCents { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
