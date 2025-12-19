namespace NotificationService.Domain.ValueObjects;

public sealed record Recipient
{
    public string UserId { get; }
    public string Username { get; }
    public string? Email { get; }
    public string? PhoneNumber { get; }
    public string? DeviceToken { get; }

    private Recipient(string userId, string username, string? email, string? phoneNumber, string? deviceToken)
    {
        UserId = userId;
        Username = username;
        Email = email;
        PhoneNumber = phoneNumber;
        DeviceToken = deviceToken;
    }

    public static Recipient Create(
        string userId,
        string username,
        string? email = null,
        string? phoneNumber = null,
        string? deviceToken = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));

        return new Recipient(userId, username, email, phoneNumber, deviceToken);
    }

    public bool CanReceiveEmail => !string.IsNullOrWhiteSpace(Email);
    public bool CanReceiveSms => !string.IsNullOrWhiteSpace(PhoneNumber);
    public bool CanReceivePush => !string.IsNullOrWhiteSpace(DeviceToken);
}
