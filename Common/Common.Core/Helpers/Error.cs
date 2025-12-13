namespace Common.Core.Helpers;

public record Error
{
    public string Code { get; }
    public string Message { get; }

    protected Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static Error Create(string code, string message) => new(code, message);

    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");
}
