namespace NotificationService.Domain.ValueObjects;

public sealed record IdempotencyKey
{
    public string Value { get; }

    private IdempotencyKey(string value)
    {
        Value = value;
    }

    public static IdempotencyKey Create(string recipientId, string notificationType, string? referenceId)
    {
        var key = $"{recipientId}:{notificationType}:{referenceId ?? "none"}";
        return new IdempotencyKey(key);
    }

    public static IdempotencyKey FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Idempotency key cannot be empty", nameof(value));

        return new IdempotencyKey(value);
    }

    public override string ToString() => Value;
}
