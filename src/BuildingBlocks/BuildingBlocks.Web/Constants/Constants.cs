namespace BuildingBlocks.Web.Constants;

public static class HeaderConstants
{
    public const string CorrelationId = "X-Correlation-Id";
}

public static class ProblemDetailsExtensionKeys
{
    public const string Errors = "errors";
    public const string General = "general";
    public const string CorrelationId = "correlationId";
    public const string Exception = "exception";
}

public static class MediaTypeConstants
{
    public const string Json = "application/json";
    public const string ProblemJson = "application/problem+json";
}

public static class EnvironmentNameConstants
{
    public const string Local = "Local";
}
