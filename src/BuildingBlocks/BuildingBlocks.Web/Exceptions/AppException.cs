namespace BuildingBlocks.Web.Exceptions;

public enum ErrorCode
{
    Unknown = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    Configuration = 6
}

public abstract class AppException : Exception
{
    public ErrorCode Code { get; }
    public string? Details { get; }

    protected AppException(string message, ErrorCode code, string? details = null, Exception? inner = null)
        : base(message, inner)
    {
        Code = code;
        Details = details;
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message, string? details = null)
        : base(message, ErrorCode.NotFound, details) { }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message, string? details = null)
        : base(message, ErrorCode.Conflict, details) { }
}

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message, string? details = null)
        : base(message, ErrorCode.Unauthorized, details) { }
}

public sealed class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message, string? details = null)
        : base(message, ErrorCode.Forbidden, details) { }
}

public sealed class ValidationAppException : AppException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationAppException(
        string message,
        IDictionary<string, string[]> errors,
        string? details = null)
        : base(message, ErrorCode.Validation, details)
    {
        Errors = new Dictionary<string, string[]>(errors);
    }
}

public sealed class ConfigurationException : AppException
{
    public ConfigurationException(string message, string? details = null)
        : base(message, ErrorCode.Configuration, details) { }
}
