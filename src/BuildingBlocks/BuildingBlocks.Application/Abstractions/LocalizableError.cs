namespace BuildingBlocks.Application.Abstractions;

public record LocalizableError : Error
{
    public object[] Parameters { get; }

    private LocalizableError(string code, string message, object[] parameters)
        : base(code, message)
    {
        Parameters = parameters;
    }

    public static LocalizableError Localizable(string code, string fallbackMessage, params object[] parameters)
        => new(code, fallbackMessage, parameters);
}
