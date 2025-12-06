namespace Common.Utilities.Extensions;

public static class GuidExtensions
{
    public static bool IsEmpty(this Guid guid)
    {
        return guid == Guid.Empty;
    }
    public static bool IsNullOrEmpty(this Guid? guid)
    {
        return !guid.HasValue || guid.Value == Guid.Empty;
    }

    public static Guid? NullIfEmpty(this Guid guid)
    {
        return guid == Guid.Empty ? null : guid;
    }

    public static Guid OrDefault(this Guid? guid, Guid defaultValue = default)
    {
        return guid ?? defaultValue;
    }
}
