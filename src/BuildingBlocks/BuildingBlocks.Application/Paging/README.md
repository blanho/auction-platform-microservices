# Common Pagination Guide

This guide shows how to implement consistent pagination, filtering, and sorting across all services using BuildingBlocks.

## Overview

The pagination system provides:
- **Unified QueryParameters** - Single source of truth for page, pageSize, sorting
- **Type-Safe Filtering** - FilterBuilder for conditional filter composition
- **Sortable Fields Map** - Whitelisted sort fields (prevents SQL injection)
- **Paged Responses** - PaginatedResult with metadata
- **Validation** - QueryParametersValidator for input validation

## Quick Start

### 1. Define Your Query Parameters (with typed filter)

```csharp
public class UserFilter
{
    public string? Search { get; init; }
    public bool? IsActive { get; init; }
    public string? Role { get; init; }
    public DateTimeOffset? CreatedFrom { get; init; }
    public DateTimeOffset? CreatedTo { get; init; }
}

public class GetUsersQuery : QueryParameters<UserFilter> { }
```

### 2. Define Sort Map (whitelisted fields only)

```csharp
public static class UserSortMap
{
    public static readonly Dictionary<string, Expression<Func<User, object>>> Map = new()
    {
        ["name"] = u => u.Name,
        ["email"] = u => u.Email,
        ["createdat"] = u => u.CreatedAt,
        ["status"] = u => u.Status
    };
}
```

### 3. Repository Implementation

```csharp
public async Task<PaginatedResult<UserDto>> GetUsersAsync(
    GetUsersQuery query,
    CancellationToken ct = default)
{
    var filter = query.Filter;
    
    var filterBuilder = FilterBuilder<User>.Create()
        .WhenNotEmpty(filter.Search, u => 
            u.Name.Contains(filter.Search!) || u.Email.Contains(filter.Search!))
        .WhenHasValue(filter.IsActive, u => u.IsActive == filter.IsActive!.Value)
        .WhenNotEmpty(filter.Role, u => u.Roles.Any(r => r.Name == filter.Role))
        .WhenHasValue(filter.CreatedFrom, u => u.CreatedAt >= filter.CreatedFrom!.Value)
        .WhenHasValue(filter.CreatedTo, u => u.CreatedAt <= filter.CreatedTo!.Value);

    return await _db.Users
        .AsNoTracking()
        .ApplyFiltering(filterBuilder)
        .ApplySorting(query, UserSortMap.Map, u => u.CreatedAt, tieBreaker: u => u.Id)
        .ToPaginatedResultAsync(query, u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email
        }, ct);
}
```

### 4. Controller/Endpoint

```csharp
[HttpGet]
public async Task<IResult> GetUsers(
    [FromQuery] GetUsersQuery query,
    CancellationToken ct)
{
    var result = await _userService.GetUsersAsync(query, ct);
    return Results.Ok(result);
}
```

## API Request Examples

```http
GET /api/users?page=1&pageSize=20&sortBy=name&sortDescending=false

GET /api/users?page=2&pageSize=10&filter.search=john&filter.isActive=true

GET /api/users?sorts[0].field=name&sorts[0].desc=false&sorts[1].field=createdAt&sorts[1].desc=true
```

HTTP parameter names and multi-sort binding depend on the endpoint's request DTO.
An application query supporting `Sorts` does not automatically expose it over HTTP.
See the [service audit](../../../../docs/paging-consistency.md) for existing API
differences and omitted-input defaults across all ten services.

## Bounds, Sorting, and Validation

`QueryParameters.Create`, `ApplyPaging`, the EF helpers, and `PaginatedResult.Empty`
share the same normalization: page values below 1 become 1, nonpositive page sizes
become 10, and sizes above 100 become 100. Services may retain explicit defaults
such as 20 or 50 when an input is omitted. Pages whose offsets would overflow
`int` are normalized to the largest page whose offset fits. Metadata and HTTP
pagination headers must use these effective values.

Numeric ranges are normalized rather than rejected; invalid numeric syntax still
fails HTTP binding. `QueryParametersValidator` runs after normalization, validates
sort-name length and malformed sort-list entries, and can compose typed filter
validation. Include it explicitly in the owning request validator: inheriting
`QueryParameters` does not register a validator. Shared validation messages and
field labels support English and Japanese.

Sort fields are trimmed and matched case-insensitively against the map. Unknown
fields are ignored; a list with no supported fields falls back to the default
sort. Repeated fields use their first direction. `Sorts` takes precedence over
`SortBy`. Always supply a default sort and a unique tie-breaker when paging; the
helper does not infer entity identifiers. Cache keys must include every filter
and ordering descriptor.

## Response Format

```json
{
  "items": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## Component Locations

| Component | Location | Purpose |
|-----------|----------|---------|
| `QueryParameters` | `src/BuildingBlocks/BuildingBlocks.Application/Paging/QueryParameters.cs` | Base class with page, pageSize, sorting |
| `QueryParameters<TFilter>` | `src/BuildingBlocks/BuildingBlocks.Application/Paging/QueryParameters.cs` | Generic version with typed filter |
| `FilterBuilder<T>` | `src/BuildingBlocks/BuildingBlocks.Application/Filtering/FilterBuilder.cs` | Fluent filter building |
| `QueryExtensions` | `src/BuildingBlocks/BuildingBlocks.Application/Paging/QueryExtensions.cs` | IQueryable extensions (paging, sorting, filtering) |
| `EfCoreQueryExtensions` | `src/BuildingBlocks/BuildingBlocks.Infrastructure/Extensions/EfCoreQueryExtensions.cs` | EF Core specific extensions |
| `PaginatedResult<T>` | `src/BuildingBlocks/BuildingBlocks.Application/Abstractions/PaginatedResult.cs` | Standard response |
| `QueryParametersValidator` | `src/BuildingBlocks/BuildingBlocks.Application/Paging/QueryParametersValidator.cs` | Input validation |
| `PaginationDefaults` | `src/BuildingBlocks/BuildingBlocks.Application/Constants/PaginationDefaults.cs` | Defaults: page=1, pageSize=10, maxPageSize=100 |

## Best Practices

### DO
- Use `AsNoTracking()` for read-only queries
- Use `.Select()` projection to avoid returning entities
- Define sort maps to whitelist allowed sort fields
- Validate query parameters early
- Use `FilterBuilder` for complex conditional filtering
- Index fields used in filters and sorting
- Apply paging at database level
- Add a unique tie-breaker, such as ID, after the selected sort fields
- Pass cancellation through count and page reads, executing them sequentially on one DbContext

### DON'T
- Accept raw field names for sorting (SQL injection risk)
- Allow `PageSize > 100` (automatically clamped by `QueryParameters`)
- Filter after `.ToListAsync()` (memory issues)
- Return full entities to API responses
- Hardcode sorting logic per endpoint
- Mix filtering logic between controller and service

## Advanced Usage

### Multiple Sort Fields

```csharp
var query = new GetUsersQuery
{
    Sorts = new[]
    {
        new SortDescriptor { Field = "status", Desc = false },
        new SortDescriptor { Field = "createdAt", Desc = true }
    }
};
```

### FilterBuilder Fluent API

```csharp
var filters = FilterBuilder<User>.Create()
    .When(onlyActive, u => u.IsActive)
    .WhenNotEmpty(search, u => u.Name.Contains(search))
    .WhenHasValue(roleId, u => u.RoleId == roleId)
    .WhenInRange(startDate, endDate, u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);

query = query.ApplyFiltering(filters);
```

### Projection with Pagination

```csharp
return await _db.Users
    .AsNoTracking()
    .ApplySorting(query, UserSortMap.Map, u => u.CreatedAt, tieBreaker: u => u.Id)
    .ToPaginatedResultAsync(
        query,
        u => new UserDto { Id = u.Id, Name = u.Name }, 
        ct);
```

Count the filtered rows before paging, then select the page. Application projects
with EF query interfaces can use `CountAsync`, `ApplyPaging`, and `Select` directly;
do not add an Infrastructure dependency to Application just to use the EF helper.
Use `ApplyOffsetPaging` only to adapt an existing offset/take contract. It clamps
negative offsets to zero and applies the common size bounds.

Unique ordering makes tied rows deterministic for unchanged data. It does not
freeze records across requests or make separate count/page reads a transaction
snapshot. Cursor/snapshot pagination is appropriate when stable traversal under
concurrent writes or deep browsing is required. Elasticsearch rejects requests
beyond its configured result window. Index filtering/ordering combinations based
on actual query plans. Worker batches, category trees, lookups, autocomplete, and
export streams have separate contracts and are not API pages.
