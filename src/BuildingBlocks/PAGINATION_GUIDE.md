# Common Pagination System Implementation

## Overview

A production-ready pagination, filtering, and sorting system built into `BuildingBlocks` for consistent API behavior across all services.

## Architecture

### Layer Separation

**Application Layer** (`BuildingBlocks.Application`)
- Core abstractions (no EF Core dependency)
- `QueryParameters` - Pagination model
- `FilterBuilder<T>` - Type-safe filter composition
- `QueryExtensions` - Paging, sorting, filtering operations
- `PaginationDefaults` - Configuration constants

**Infrastructure Layer** (`BuildingBlocks.Infrastructure`)
- EF Core specific extensions
- `EfCoreQueryExtensions` - Async materialization methods
- Works with any DbContext

### Key Types

```
QueryParameters
├── Page (default: 1, min: 1)
├── PageSize (default: 10, max: 100 - auto-clamped)
├── SortBy (string, matches keys in SortMap)
├── SortDescending (bool)
├── Sorts (IReadOnlyList<SortDescriptor>, advanced)
└── Skip / Take properties

QueryParameters<TFilter>
├── Inherits QueryParameters
└── Filter (TFilter object)

PaginatedResult<T>
├── Items (IReadOnlyList<T>)
├── TotalCount (int)
├── Page (int)
├── PageSize (int)
├── TotalPages (computed)
├── HasPreviousPage (bool)
└── HasNextPage (bool)

FilterBuilder<T>
├── When(condition, predicate)
├── WhenNotNull(value, predicate)
├── WhenHasValue(struct?, predicate)
├── WhenNotEmpty(string, predicate)
├── WhenNotEmpty(collection, predicate)
├── WhenInRange(from, to, predicate)
└── Apply(query)
```

## Implementation Pattern

### 1. Define Filter

```csharp
public class ProductFilter
{
    public string? Search { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? InStock { get; init; }
    public DateTime? CreatedAfter { get; init; }
}

public class GetProductsQuery : QueryParameters<ProductFilter> { }
```

### 2. Define Sort Map

```csharp
public static class ProductSortMap
{
    public static readonly Dictionary<string, Expression<Func<Product, object>>> Map = new()
    {
        ["name"] = p => p.Name,
        ["price"] = p => p.Price,
        ["createdat"] = p => p.CreatedAt,
        ["stock"] = p => p.AvailableStock
    };
}
```

### 3. Repository Method

```csharp
public async Task<PaginatedResult<ProductDto>> GetProductsAsync(
    GetProductsQuery query,
    CancellationToken ct)
{
    var filter = query.Filter;

    var filterBuilder = FilterBuilder<Product>.Create()
        .WhenNotEmpty(
            filter.Search,
            p => p.Name.Contains(filter.Search) || p.Description.Contains(filter.Search))
        .WhenHasValue(
            filter.MinPrice,
            p => p.Price >= filter.MinPrice)
        .WhenHasValue(
            filter.MaxPrice,
            p => p.Price <= filter.MaxPrice)
        .WhenHasValue(
            filter.InStock,
            p => p.AvailableStock > 0)
        .WhenHasValue(
            filter.CreatedAfter,
            p => p.CreatedAt >= filter.CreatedAfter);

    return await _db.Products
        .AsNoTracking()
        .ApplyFiltering(filterBuilder)
        .ApplySorting(query, ProductSortMap.Map, p => p.CreatedAt)
        .ToPaginatedResultAsync(
            query,
            p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                InStock = p.AvailableStock > 0
            },
            ct);
}
```

### 4. Endpoint

```csharp
[HttpGet]
public async Task<IResult> GetProducts(
    [FromQuery] GetProductsQuery query,
    CancellationToken ct)
{
    var result = await _productService.GetProductsAsync(query, ct);
    return Results.Ok(result);
}
```

## Usage Examples

### Basic Pagination
```
GET /api/products?page=1&pageSize=20
```

### With Sorting
```
GET /api/products?page=1&pageSize=20&sortBy=price&sortDescending=true
```

### With Filtering
```
GET /api/products?page=1&pageSize=20
  &filter.search=laptop
  &filter.minPrice=500
  &filter.maxPrice=1500
  &filter.inStock=true
```

### Multiple Sorts
```
GET /api/products?page=1
  &sorts[0].field=price&sorts[0].desc=true
  &sorts[1].field=name&sorts[1].desc=false
```

## Response Format

```json
{
  "items": [
    {
      "id": "123",
      "name": "Laptop",
      "price": 1299.99,
      "inStock": true
    }
  ],
  "totalCount": 1250,
  "page": 1,
  "pageSize": 20,
  "totalPages": 63,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## Safety Features

✅ **Page Size Limiting**
- Maximum 100 items per page
- Prevents memory exhaustion
- Auto-clamped, not rejected

✅ **Sort Field Whitelist**
- Only defined sort keys allowed
- Prevents SQL injection
- Invalid fields silently ignored

✅ **Type-Safe Filtering**
- Compile-time validation
- FilterBuilder enforces null checks
- No magic strings

✅ **Input Validation**
- `QueryParametersValidator` for automatic validation
- Integrated with FluentValidation
- Page >= 1, PageSize >= 1 && <= 100

## Performance Considerations

### Indexing Strategy

For tables with pagination:

```sql
CREATE INDEX idx_product_createdat ON Products(CreatedAt DESC);
CREATE INDEX idx_product_name ON Products(Name);
CREATE INDEX idx_product_price ON Products(Price);
CREATE INDEX idx_product_availablestock ON Products(AvailableStock);
```

### AsNoTracking()
Always use for read-only queries to avoid change tracking overhead.

### Select Projection
```csharp
// ❌ Bad: Returns entities
var items = await query.ToListAsync();

// ✅ Good: Projects to DTOs early
var items = await query
    .Select(p => new ProductDto { ... })
    .ToListAsync();
```

## Migration from Old Pattern

### Before
```csharp
public async Task<PaginatedResult<ProductDto>> GetProductsAsync(
    int page, int pageSize, string? search, decimal? minPrice,
    decimal? maxPrice, string? sortBy, CancellationToken ct)
{
    // Manual validation
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;
    
    // Inline filtering
    var query = _db.Products.AsNoTracking();
    if (!string.IsNullOrEmpty(search))
        query = query.Where(p => p.Name.Contains(search));
    if (minPrice.HasValue)
        query = query.Where(p => p.Price >= minPrice);
    if (maxPrice.HasValue)
        query = query.Where(p => p.Price <= maxPrice);
    
    // Inline sorting (unsafe!)
    query = sortBy?.ToLower() switch
    {
        "price" => query.OrderBy(p => p.Price),
        "name" => query.OrderBy(p => p.Name),
        _ => query.OrderBy(p => p.CreatedAt)
    };
    
    var totalCount = await query.CountAsync(ct);
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new ProductDto { ... })
        .ToListAsync(ct);
    
    return new PaginatedResult<ProductDto>(items, totalCount, page, pageSize);
}
```

### After
```csharp
public async Task<PaginatedResult<ProductDto>> GetProductsAsync(
    GetProductsQuery query,
    CancellationToken ct)
{
    var filter = query.Filter;

    return await _db.Products
        .AsNoTracking()
        .ApplyFiltering(FilterBuilder<Product>.Create()
            .WhenNotEmpty(filter.Search, p => p.Name.Contains(filter.Search))
            .WhenHasValue(filter.MinPrice, p => p.Price >= filter.MinPrice)
            .WhenHasValue(filter.MaxPrice, p => p.Price <= filter.MaxPrice))
        .ApplySorting(query, ProductSortMap.Map, p => p.CreatedAt)
        .ToPaginatedResultAsync(query, p => new ProductDto { ... }, ct);
}
```

## Benefits

| Aspect | Benefit |
|--------|---------|
| **Consistency** | All services use same pattern |
| **Safety** | Type-safe, no SQL injection, validated |
| **Performance** | Paging at DB level, projection early |
| **Maintainability** | Centralized logic, easy to extend |
| **Reusability** | Copy pattern to any entity |
| **Testing** | Easy to unit test with FilterBuilder |

## Files Created/Modified

- ✅ `Paging/QueryParameters.cs` - New
- ✅ `Filtering/FilterBuilder.cs` - New
- ✅ `Paging/QueryParametersValidator.cs` - New
- ✅ `Paging/QueryExtensions.cs` - Enhanced
- ✅ `Infrastructure/Extensions/EfCoreQueryExtensions.cs` - New
- ✅ `Paging/README.md` - Documentation
