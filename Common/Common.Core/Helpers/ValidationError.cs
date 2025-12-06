namespace Common.Core.Helpers;

public sealed record ValidationError : Error
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    private ValidationError(string code, string message, IReadOnlyDictionary<string, string[]> errors)
        : base(code, message)
    {
        Errors = errors;
    }

    public static ValidationError Create(string code, string message, IDictionary<string, string[]> errors) 
        => new(code, message, new Dictionary<string, string[]>(errors));
    
    public static ValidationError WithErrors(IDictionary<string, string[]> errors)
        => new("Validation.Failed", "One or more validation errors occurred.", new Dictionary<string, string[]>(errors));
}
