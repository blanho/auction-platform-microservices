namespace BuildingBlocks.Application.Abstractions;

public record LocalizableError : Error
{
    public object[] Parameters { get; }
    public int? DiagnosticParameterIndex { get; private init; }

    private LocalizableError(string code, string message, object[] parameters)
        : base(code, message)
    {
        Parameters = parameters;
    }

    public static LocalizableError Localizable(string code, string fallbackMessage, params object[] parameters)
        => new(code, fallbackMessage, parameters);

    public static LocalizableError WithDiagnostic(string code, string fallbackMessage, params object[] parameters)
    {
        if (parameters.Length == 0 || parameters[^1] is not string)
            throw new ArgumentException("The final parameter must be a diagnostic message.", nameof(parameters));

        return new LocalizableError(code, fallbackMessage, parameters)
        {
            DiagnosticParameterIndex = parameters.Length - 1
        };
    }
}
