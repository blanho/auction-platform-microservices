# Common Pagination Guide

This guide shows how to implement consistent pagination, filtering, and sorting across all services using BuildingBlocks.

## Overview

The pagination system provides:
- **Unified QueryParameters** - Single source of truth for page, pageSize, sorting
- **Type-Safe Filtering** - FilterBuilder for conditional filter composition
- **Sortable Fields Map** - Whitelisted sort fields (prevents SQL injection)
- **Paged Responses** - StandardPaginatedResult with metadata
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
        .ApplySorting(query, UserSortMap.Map, u => u.CreatedAt)
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
    .ApplySorting(query, UserSortMap.Map)
    .ToPaginatedResultAsync(
        query,
        u => new UserDto { Id = u.Id, Name = u.Name }, 
        ct);
```
