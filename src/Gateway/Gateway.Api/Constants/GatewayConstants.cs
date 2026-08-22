namespace Gateway.Api.Constants;

internal static class GatewayConstants
{
    public const string LocalEnvironment = "Local";
    public const string DefaultAudience = "auctionApp";
    public const string CorrelationIdHeader = "X-Correlation-Id";
    public const string NameClaim = "name";
    public const string RoleClaim = "role";
    public const string CorrelationIdExtension = "correlationId";
    public const string ProblemJsonMediaType = "application/problem+json";
}
