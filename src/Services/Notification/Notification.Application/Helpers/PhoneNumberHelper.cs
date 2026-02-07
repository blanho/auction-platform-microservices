namespace Notification.Application.Helpers;

public static class PhoneNumberHelper
{
    public static string? FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone))
            return null;

        var cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

        if (cleaned.StartsWith('+'))
            return cleaned;

        if (cleaned.Length >= 10)
        {
            if (cleaned.Length == 10)
                return $"+1{cleaned}";

            if (cleaned.Length == 11 && cleaned.StartsWith('1'))
                return $"+{cleaned}";

            return $"+{cleaned}";
        }

        return null;
    }

    public static string MaskPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 6)
            return "***";

        return phone[..3] + new string('*', phone.Length - 6) + phone[^3..];
    }
}
