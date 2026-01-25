using System.Security.Claims;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Auditing;

public class HttpAuditContext : IAuditContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    public HttpAuditContext(
        IHttpContextAccessor httpContextAccessor,
        ICorrelationIdProvider correlationIdProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _correlationIdProvider = correlationIdProvider;
    }

    public Guid UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("username")?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("preferred_username")?.Value;

    public string? CorrelationId => _correlationIdProvider.Get();

    public string? IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
                return forwardedFor.Split(',').First().Trim();

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
