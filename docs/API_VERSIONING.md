# API Versioning Strategy

## Overview

This document outlines the API versioning strategy for the Auction Platform Microservices.

## Current Versioning Scheme

### URL Path Versioning

All APIs use URL path versioning with the format: `/api/v{version}/{resource}`

| Service | Base Path | Current Version |
|---------|-----------|-----------------|
| Auction Service | `/api/v1/auctions` | v1 |
| Bidding Service | `/api/v1/bids` | v1 |
| Payment Service | `/api/v1/orders`, `/api/v1/wallets`, `/api/v1/payments` | v1 |
| Notification Service | `/api/v1/notifications` | v1 |
| Search Service | `/api/v1/search` | v1 |
| Analytics Service | `/api/v1/dashboard`, `/api/v1/analytics` | v1 |
| Identity Service | `/api/auth`, `/api/profile`, `/api/users` | v1 (no version prefix) |

### Gateway Path Mapping

The API Gateway strips the `/api/v1` prefix for cleaner client URLs:

| Client Request | Backend Route |
|----------------|---------------|
| `GET /auctions` | `GET /api/v1/auctions` |
| `POST /bids` | `POST /api/v1/bids` |
| `GET /orders` | `GET /api/v1/orders` |

## Versioning Rules

### When to Create a New Version

Create a new API version (v2, v3, etc.) when making **breaking changes**:

1. **Removing fields** from response DTOs
2. **Renaming fields** in request/response DTOs
3. **Changing field types** (e.g., `string` to `number`)
4. **Changing URL structure** for existing endpoints
5. **Removing endpoints**
6. **Changing authentication/authorization requirements**
7. **Changing error response format**

### Non-Breaking Changes (No Version Bump Required)

1. **Adding new optional fields** to request DTOs
2. **Adding new fields** to response DTOs
3. **Adding new endpoints**
4. **Adding new query parameters** (optional)
5. **Relaxing validation** (making required fields optional)
6. **Bug fixes** that don't change the contract

## Version Lifecycle

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Active    │────▶│  Deprecated │────▶│   Retired   │
│    (v2)     │     │    (v1)     │     │   (v0)      │
└─────────────┘     └─────────────┘     └─────────────┘
     │                   │                    │
     │                   │                    │
  Current             6 months            12 months
  Version           grace period          after deprecation
```

### Deprecation Process

1. **Announce deprecation** via `Deprecation` header in responses
2. **Add sunset date** via `Sunset` header
3. **Document migration path** in changelog
4. **Log usage** of deprecated endpoints for monitoring
5. **Remove after grace period** (minimum 6 months)

## Implementation Guidelines

### Controller Versioning

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuctionsController : ControllerBase
{
    // v1 endpoints
}

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuctionsV2Controller : ControllerBase
{
    // v2 endpoints with breaking changes
}
```

### Deprecation Headers

```csharp
[Obsolete("Use v2 API instead")]
[ApiVersion("1.0", Deprecated = true)]
[HttpGet]
public IActionResult GetAuctions()
{
    Response.Headers.Add("Deprecation", "true");
    Response.Headers.Add("Sunset", "2027-01-01T00:00:00Z");
    Response.Headers.Add("Link", "</api/v2/auctions>; rel=\"successor-version\"");
    // ...
}
```

## Contract Testing

### Required Tests

1. **Schema validation** - Ensure response matches documented schema
2. **Backward compatibility** - New versions should not break existing clients
3. **Contract tests** - Use Pact or similar for consumer-driven contracts

### CI/CD Integration

- Run contract tests on every PR
- Block deployment if breaking changes detected without version bump
- Generate OpenAPI specs automatically

## Changelog Format

```markdown
## [v2.0.0] - 2026-03-01

### Breaking Changes
- Renamed `currentBid` to `currentHighBid` in AuctionDto
- Removed `legacyField` from response

### Migration Guide
1. Update client to use `currentHighBid` instead of `currentBid`
2. Remove references to `legacyField`

### Deprecations
- v1 API deprecated, sunset date: 2026-09-01
```

## Client Integration

### Frontend Configuration

```typescript
const API_VERSION = 'v1'
const API_BASE_URL = import.meta.env.VITE_API_URL || '/api'

// Versioned requests (if needed for specific endpoints)
const getVersionedUrl = (path: string, version = API_VERSION) => 
  `${API_BASE_URL}/${version}${path}`
```

### Error Handling for Deprecated APIs

```typescript
axios.interceptors.response.use((response) => {
  if (response.headers['deprecation']) {
    console.warn(`API deprecated. Sunset: ${response.headers['sunset']}`)
    // Log to analytics
  }
  return response
})
```

## Monitoring

### Metrics to Track

1. **Version usage** - Requests per API version
2. **Deprecated endpoint calls** - Track for migration planning
3. **Error rates by version** - Identify version-specific issues

### Alerts

- Alert when deprecated version usage exceeds threshold
- Alert on approaching sunset dates
- Alert on version mismatch errors

## References

- [API Versioning Best Practices](https://docs.microsoft.com/en-us/azure/architecture/best-practices/api-design#versioning-a-restful-web-api)
- [Semantic Versioning](https://semver.org/)
- [RFC 8594 - Sunset Header](https://tools.ietf.org/html/rfc8594)
