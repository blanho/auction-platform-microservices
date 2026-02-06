# Clean Code Refactoring Plan — Auction Service

> A comprehensive, step-by-step plan based on **Clean Code by Robert C. Martin**.
> Each step is independent and safe to implement incrementally.

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Step 1 — Remove All Comments](#step-1--remove-all-comments)
3. [Step 2 — Resolve Namespace Ambiguities](#step-2--resolve-namespace-ambiguities)
4. [Step 3 — Fix Leaky Abstractions in Commands](#step-3--fix-leaky-abstractions-in-commands)
5. [Step 4 — Split Fat Repository Interface (ISP)](#step-4--split-fat-repository-interface-isp)
6. [Step 5 — Extract Business Logic from Repository](#step-5--extract-business-logic-from-repository)
7. [Step 6 — Fix Inconsistent Timestamp Ownership](#step-6--fix-inconsistent-timestamp-ownership)
8. [Step 7 — Remove Empty Decorator](#step-7--remove-empty-decorator)
9. [Step 8 — Eliminate Duplicate Query Handlers](#step-8--eliminate-duplicate-query-handlers)
10. [Step 9 — Fix Dependency Rule Violation (BuyNowCommandHandler)](#step-9--fix-dependency-rule-violation-buynowcommandhandler)
11. [Step 10 — Standardize Endpoint Error Handling](#step-10--standardize-endpoint-error-handling)
12. [Step 11 — Consolidate Duplicate DTOs](#step-11--consolidate-duplicate-dtos)
13. [Step 12 — Break Up God Repository Class](#step-12--break-up-god-repository-class)
14. [Step 13 — Split Monolithic Mapping Profile](#step-13--split-monolithic-mapping-profile)
15. [Step 14 — Remove Direct Repository Injection from Endpoints](#step-14--remove-direct-repository-injection-from-endpoints)
16. [Step 15 — Remove Misleading Async Methods](#step-15--remove-misleading-async-methods)
17. [Step 16 — Add Missing Validators](#step-16--add-missing-validators)
18. [Step 17 — Clean Up Global Usings](#step-17--clean-up-global-usings)
19. [Step 18 — Improve Domain Entity Factory Methods](#step-18--improve-domain-entity-factory-methods)
20. [Verification Checklist](#verification-checklist)

---

## 1. Executive Summary

### Layers Reviewed

| Layer          | Files | Key Classes |
|----------------|-------|-------------|
| Api            | ~15   | Program.cs, ServiceExtensions.cs, 6 Endpoint modules, gRPC services |
| Application    | ~102  | 20+ CQRS feature folders, 7 domain event handlers, 8 interfaces |
| Domain         | ~15   | 8 entities, 7 domain events |
| Infrastructure | ~25   | 8 repositories (2 cached), 4 Quartz jobs, 9 MassTransit consumers |

### Top Code Smells Identified

| #  | Smell | Severity | Clean Code Principle Violated |
|----|-------|----------|-------------------------------|
| 1  | `IAuctionReadRepository` has 25+ methods | High | Interface Segregation (ISP) |
| 2  | `AuctionRepository` is ~600 lines | High | Single Responsibility (SRP) |
| 3  | `PreloadedAuction` in commands leaks domain entity | High | Dependency Rule |
| 4  | `BuyNowCommandHandler` imports Infrastructure | High | Dependency Rule |
| 5  | Business logic (`SellerStats`) computed in repository | Medium | SRP / Separation of Concerns |
| 6  | Duplicate query handlers (GetAuctions/GetMyAuctions) | Medium | DRY |
| 7  | Empty `CachedAuctionViewRepository` decorator | Medium | Dead code |
| 8  | Duplicate DTOs (`BuyNowResultDto`, `CreateAuction*Dto`) | Medium | DRY |
| 9  | Fully qualified types throughout codebase | Medium | Namespace pollution |
| 10 | `Auction.Create()` sets `CreatedAt` but repository overwrites it | Medium | Single source of truth |
| 11 | Magic strings for error codes in endpoint routing | Low | Named constants |
| 12 | Inconsistent error handling patterns across endpoints | Low | Consistency |
| 13 | Missing validators for BuyNow, Delete, Activate, Deactivate | Low | Fail fast |
| 14 | Monolithic `MappingProfiles` class | Low | SRP |

---

## Step 1 — Remove All Comments

**Principle**: Self-documenting code (project rule: no comments)

### Files to Change

| File | Line | Issue |
|------|------|-------|
| `Infrastructure/Messaging/Consumers/BidPlacedConsumer.cs` | Line 8 | `/// Consumes BidPlacedEvent to update auction's current high bid.` |

### Action

Remove the XML comment from `BidPlacedConsumer`. The class name already communicates its purpose.

**Before:**
```csharp
/// Consumes BidPlacedEvent to update auction's current high bid.
public class BidPlacedConsumer : IConsumer<BidPlacedEvent>
```

**After:**
```csharp
public class BidPlacedConsumer : IConsumer<BidPlacedEvent>
```

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`

---

## Step 2 — Resolve Namespace Ambiguities

**Principle**: Clean namespaces; avoid forcing fully qualified names

### Problem

Two `IUnitOfWork` interfaces exist:
- `BuildingBlocks.Application.Abstractions.IUnitOfWork`
- `BuildingBlocks.Infrastructure.Repository.IUnitOfWork`

Two `ICacheService` interfaces existed (duplicate was deleted, but fully qualified references remain).

This forces ugly fully qualified types across 10+ files.

### Files Affected

| File | Current | Desired |
|------|---------|---------|
| `Infrastructure/Persistence/UnitOfWork.cs` | `BuildingBlocks.Application.Abstractions.IUnitOfWork` | `IUnitOfWork` |
| `Infrastructure/Jobs/AuctionActivationJob.cs` | `BuildingBlocks.Application.Abstractions.IUnitOfWork` | `IUnitOfWork` |
| `Infrastructure/Jobs/CheckAuctionFinishedJob.cs` | `BuildingBlocks.Application.Abstractions.IUnitOfWork` | `IUnitOfWork` |
| `Infrastructure/Jobs/AuctionDeactivationJob.cs` | `BuildingBlocks.Application.Abstractions.IUnitOfWork` | `IUnitOfWork` |
| `Infrastructure/Persistence/Repositories/CachedAuctionRepository.cs` | `BuildingBlocks.Application.Abstractions.ICacheService` | `ICacheService` |
| `Api/Program.cs` | `BuildingBlocks.Application.Abstractions.ICacheService` | `ICacheService` |
| `Api/Extensions/DependencyInjection/ServiceExtensions.cs` | `BuildingBlocks.Application.Abstractions.ICacheService` | `ICacheService` |

### Action

1. **Rename** `BuildingBlocks.Infrastructure.Repository.IUnitOfWork` to `IBaseUnitOfWork` (or delete it if it's only used internally by `BaseUnitOfWork`).
2. Add a `global using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;` to `Infrastructure/GlobalUsings.cs`.
3. Add a `global using ICacheService = BuildingBlocks.Application.Abstractions.ICacheService;` to `Infrastructure/GlobalUsings.cs`.
4. Remove all fully qualified `BuildingBlocks.Application.Abstractions.IUnitOfWork` references.
5. Remove all fully qualified `BuildingBlocks.Application.Abstractions.ICacheService` references.

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`

---

## Step 3 — Fix Leaky Abstractions in Commands

**Principle**: Dependency Rule — Application layer must not depend on domain entity transport

### Problem

Four commands accept `Auction? PreloadedAuction` to pass a pre-fetched domain entity from the endpoint layer:

```csharp
public record UpdateAuctionCommand(Guid Id, ..., Auction? PreloadedAuction = null) : ICommand<bool>;
public record DeleteAuctionCommand(Guid Id, Auction? PreloadedAuction = null) : ICommand<bool>;
public record ActivateAuctionCommand(Guid AuctionId, Auction? PreloadedAuction = null) : ICommand<AuctionDto>;
public record DeactivateAuctionCommand(Guid AuctionId, string? Reason, Auction? PreloadedAuction = null) : ICommand<AuctionDto>;
```

This pattern:
- Leaks entity lifecycle management to the API layer
- Makes commands impure (they carry mutable state)
- Couples endpoint authorization checks to command execution

### Action

**Option A — Remove PreloadedAuction (Recommended)**

1. Remove `PreloadedAuction` from all 4 commands
2. Move authorization logic into a `MediatR` pipeline behavior (`IAuthorizationBehavior`)
3. Let each handler fetch the entity itself

**Option B — Use an Authorization Pipeline Behavior**

Create `AuctionOwnerAuthorizationBehavior<TRequest, TResponse>` that:
- Intercepts commands with an `[RequireAuctionOwnership]` attribute
- Fetches and validates the auction
- Sets it on a scoped `IAuctionContext` service
- Handlers read from `IAuctionContext` instead of command parameter

### Implementation (Option A)

#### 3a. Remove `PreloadedAuction` from Commands

**UpdateAuctionCommand.cs:**
```csharp
public record UpdateAuctionCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? Condition,
    int? YearManufactured,
    Dictionary<string, string>? Attributes
) : ICommand<bool>;
```

**DeleteAuctionCommand.cs:**
```csharp
public record DeleteAuctionCommand(Guid Id) : ICommand<bool>;
```

**ActivateAuctionCommand.cs:**
```csharp
public record ActivateAuctionCommand(Guid AuctionId) : ICommand<AuctionDto>;
```

**DeactivateAuctionCommand.cs:**
```csharp
public record DeactivateAuctionCommand(Guid AuctionId, string? Reason = null) : ICommand<AuctionDto>;
```

#### 3b. Update All 4 Handlers

Remove `request.PreloadedAuction ??` fallback pattern. Always fetch from repository:

```csharp
var auction = await _repository.GetByIdForUpdateAsync(request.Id, cancellationToken);
```

#### 3c. Update Endpoints

Remove `IAuctionWriteRepository` injection from endpoint methods. Send commands directly:

**Before (AuctionCrudEndpoints.UpdateAuction):**
```csharp
private static async Task<IResult> UpdateAuction(
    Guid id,
    UpdateAuctionDto dto,
    HttpContext httpContext,
    IMediator mediator,
    IAuctionWriteRepository auctionRepository,  // ← Remove this
    CancellationToken ct)
{
    var (auction, error) = await auctionRepository.GetAuthorizedAuctionAsync(
        httpContext, id, Permissions.Auctions.Edit, ct);
    if (error != null) return error;
    
    var command = new UpdateAuctionCommand(id, ..., auction);  // ← Remove auction
    ...
}
```

**After:**
```csharp
private static async Task<IResult> UpdateAuction(
    Guid id,
    UpdateAuctionDto dto,
    HttpContext httpContext,
    IMediator mediator,
    CancellationToken ct)
{
    var command = new UpdateAuctionCommand(id, dto.Title, dto.Description,
        dto.Condition, dto.YearManufactured, dto.Attributes);
    var result = await mediator.Send(command, ct);
    return result.IsSuccess
        ? Results.NoContent()
        : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
}
```

> **Note**: Authorization must then be handled either by the handler itself (checking seller ownership) or via a pipeline behavior.

#### 3d. Create Authorization Behavior (if using pipeline approach)

```
Application/Behaviors/AuctionOwnerAuthorizationBehavior.cs
```

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`
- Run existing unit tests: `dotnet test tests/Auction.Application.Tests/`

---

## Step 4 — Split Fat Repository Interface (ISP)

**Principle**: Interface Segregation — No client should depend on methods it doesn't use

### Problem

`IAuctionReadRepository` has **25+ methods**. Most handlers use only 1-2 methods.

### Action

Split into focused interfaces:

```
Application/Interfaces/
├── IAuctionReadRepository.cs        → Core CRUD reads (5 methods)
├── IAuctionWriteRepository.cs       → Already exists (8 methods) ✓
├── IAuctionQueryRepository.cs       → Filtered/paged queries
├── IAuctionAnalyticsRepository.cs   → Stats, revenue, trending
├── IAuctionSchedulingRepository.cs  → Job-related queries
└── IAuctionExportRepository.cs      → Export-related queries
```

#### Proposed Interface Split

**IAuctionReadRepository.cs** (Core — used by most handlers):
```csharp
public interface IAuctionReadRepository
{
    Task<Auction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<List<Auction>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
```

**IAuctionQueryRepository.cs** (Used by query handlers):
```csharp
public interface IAuctionQueryRepository
{
    Task<PaginatedResult<Auction>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PaginatedResult<Auction>> GetPagedAsync(AuctionFilterDto filter, CancellationToken ct = default);
    Task<List<Auction>> GetBySellerUsernameAsync(string username, CancellationToken ct = default);
    Task<List<Auction>> GetWonByUsernameAsync(string username, CancellationToken ct = default);
    Task<List<Auction>> GetTrendingItemsAsync(int limit, CancellationToken ct = default);
    Task<List<Auction>> GetActiveAuctionsBySellerIdAsync(Guid sellerId, CancellationToken ct = default);
    Task<List<Auction>> GetAllBySellerIdAsync(Guid sellerId, CancellationToken ct = default);
    Task<List<Auction>> GetAuctionsWithWinnerIdAsync(Guid winnerId, CancellationToken ct = default);
    Task<int> GetWatchlistCountByUsernameAsync(string username, CancellationToken ct = default);
}
```

**IAuctionAnalyticsRepository.cs** (Used by dashboard/analytics):
```csharp
public interface IAuctionAnalyticsRepository
{
    Task<int> CountLiveAuctionsAsync(CancellationToken ct = default);
    Task<int> CountEndingSoonAsync(CancellationToken ct = default);
    Task<int> GetCountByStatusAsync(Status status, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default);
    Task<int> GetCountEndingBetweenAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default);
    Task<List<Auction>> GetTopByRevenueAsync(int limit, CancellationToken ct = default);
    Task<List<CategoryStatDto>> GetCategoryStatsAsync(CancellationToken ct = default);
    Task<SellerStatsDto> GetSellerStatsAsync(string username, DateTimeOffset periodStart, DateTimeOffset? previousPeriodStart = null, CancellationToken ct = default);
}
```

**IAuctionSchedulingRepository.cs** (Used by Quartz jobs):
```csharp
public interface IAuctionSchedulingRepository
{
    Task<List<Auction>> GetFinishedAuctionsAsync(CancellationToken ct = default);
    Task<List<Auction>> GetAuctionsToAutoDeactivateAsync(CancellationToken ct = default);
    Task<List<Auction>> GetScheduledAuctionsToActivateAsync(CancellationToken ct = default);
    Task<List<Auction>> GetAuctionsEndingBetweenAsync(DateTime startTime, DateTime endTime, CancellationToken ct = default);
    Task<List<Auction>> GetAuctionsForExportAsync(Status? status = null, string? seller = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, CancellationToken ct = default);
}
```

### Migration

1. Create the new interfaces
2. Have `AuctionRepository` implement all of them
3. Update DI registration for each interface
4. Update each handler to depend only on the interface it needs
5. Update `CachedAuctionRepository` accordingly (or split caching too)

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`
- `dotnet test tests/Auction.Application.Tests/`

---

## Step 5 — Extract Business Logic from Repository

**Principle**: SRP — Repositories do data access, not business logic

### Problem

`AuctionRepository.GetSellerStatsAsync()` (~80 lines) computes:
- Revenue aggregation
- Period-over-period comparison
- Recent sales mapping
- Status counting

This is **business/application logic**, not data access.

### Action

1. Create `Application/Features/Auctions/GetSellerStats/SellerStatsCalculator.cs`
2. Move the computation logic there
3. Repository provides raw data only

#### New Structure

```
Application/Features/Auctions/GetSellerStats/
├── GetSellerStatsQuery.cs
├── GetSellerStatsQueryHandler.cs
└── SellerStatsCalculator.cs
```

**SellerStatsCalculator.cs:**
```csharp
public class SellerStatsCalculator
{
    public SellerStatsDto Calculate(
        List<Auction> currentPeriodAuctions,
        List<Auction> previousPeriodAuctions,
        Dictionary<Status, int> statusCounts)
    {
        // Move all aggregation logic here
    }
}
```

**Repository becomes:**
```csharp
Task<List<Auction>> GetBySellerInPeriodAsync(string username, DateTimeOffset start, DateTimeOffset? end, CancellationToken ct);
Task<Dictionary<Status, int>> GetStatusCountsBySellerAsync(string username, CancellationToken ct);
```

### Also Extract

`SaleProjection` private class inside `AuctionRepository` → Move to `Application/DTOs/Auctions/SaleProjection.cs` or inline the projection in the new calculator.

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`
- `dotnet test tests/Auction.Application.Tests/`

---

## Step 6 — Fix Inconsistent Timestamp Ownership

**Principle**: Single source of truth

### Problem

`Auction.Create()` sets `CreatedAt = DateTimeOffset.UtcNow` (domain entity).
`AuctionRepository.CreateAsync()` overwrites with `auction.CreatedAt = _dateTime.UtcNow` (repository).

Two sources of truth for the same value, and the domain entity bypasses the `IDateTimeProvider` abstraction.

### Action

**Option A — Domain owns timestamps (Recommended for DDD)**

1. Inject `IDateTimeProvider` into factory method or accept `DateTimeOffset` parameter
2. Remove timestamp-setting from repository

**Option B — Repository owns timestamps**

1. Remove `CreatedAt = DateTimeOffset.UtcNow` from `Auction.Create()`
2. Remove `CreatedAt = DateTimeOffset.UtcNow` from `Auction.CreateScheduled()`
3. Keep repository as the single source of truth

### Recommended: Option B

```csharp
public static Auction Create(...)
{
    return new Auction
    {
        Id = Guid.NewGuid(),
        SellerId = sellerId,
        // Remove: CreatedAt = DateTimeOffset.UtcNow,
        ...
    };
}
```

Repository already handles it:
```csharp
public async Task<Auction> CreateAsync(Auction auction, CancellationToken ct)
{
    auction.CreatedAt = _dateTime.UtcNow;
    auction.CreatedBy = _auditContext.UserId;
    ...
}
```

### Files to Change

| File | Action |
|------|--------|
| `Domain/Entities/Auction.cs` → `Create()` | Remove `CreatedAt = DateTimeOffset.UtcNow` |
| `Domain/Entities/Auction.cs` → `CreateScheduled()` | Remove `CreatedAt = DateTimeOffset.UtcNow` |
| `Domain/Entities/Auction.cs` → `Cancel()` | Remove `UpdatedAt = DateTimeOffset.UtcNow` |
| `Domain/Entities/Auction.cs` → `UpdateSellerUsername()` | Remove `UpdatedAt = DateTimeOffset.UtcNow` |
| `Domain/Entities/Auction.cs` → `UpdateWinnerUsername()` | Remove `UpdatedAt = DateTimeOffset.UtcNow` |
| `Domain/Events/AuctionUpdatedDomainEvent.cs` | Remove `UpdatedAt = DateTimeOffset.UtcNow` default value |

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`
- `dotnet test tests/Auction.Domain.Tests/`

---

## Step 7 — Remove Empty Decorator

**Principle**: Every class must earn its existence

### Problem

`CachedAuctionViewRepository` implements `IAuctionViewRepository` but has **zero caching logic**. Every method simply delegates to `_inner`:

```csharp
public class CachedAuctionViewRepository : IAuctionViewRepository
{
    private readonly IAuctionViewRepository _inner;
    
    public async Task<int> GetViewCountForAuctionAsync(Guid auctionId, CancellationToken ct)
        => await _inner.GetViewCountForAuctionAsync(auctionId, ct);

    // ... all methods are pure pass-through
}
```

### Action

1. Delete `Infrastructure/Persistence/Repositories/CachedAuctionViewRepository.cs`
2. Update DI in `ServiceExtensions.cs`:

**Before:**
```csharp
services.AddScoped<AuctionViewRepository>();
services.AddScoped<IAuctionViewRepository>(sp =>
{
    var inner = sp.GetRequiredService<AuctionViewRepository>();
    return new CachedAuctionViewRepository(inner);
});
```

**After:**
```csharp
services.AddScoped<IAuctionViewRepository, AuctionViewRepository>();
```

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`

---

## Step 8 — Eliminate Duplicate Query Handlers

**Principle**: DRY — Don't Repeat Yourself

### Problem

`GetAuctionsQueryHandler` and `GetMyAuctionsQueryHandler` are nearly identical (~90% code duplication). Both:
1. Build an `AuctionFilterDto`
2. Call `_repository.GetPagedAsync(queryParams, ct)`
3. Map to `PaginatedResult<AuctionDto>`

The only difference is that `GetMyAuctions` sets `Seller = request.Username`.

### Action

**Option A — Share a private method**

Create a base class or shared service:

```csharp
public class PaginatedAuctionQueryService
{
    private readonly IAuctionReadRepository _repository;
    private readonly IMapper _mapper;

    public async Task<Result<PaginatedResult<AuctionDto>>> ExecutePagedQuery(
        AuctionFilterDto queryParams,
        CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(queryParams, cancellationToken);
        var dtos = result.Items.Select(a => _mapper.Map<AuctionDto>(a)).ToList();
        return Result.Success(new PaginatedResult<AuctionDto>(dtos, result.TotalCount, queryParams.Page, queryParams.PageSize));
    }
}
```

**Option B — Merge into one handler with optional username filter**

Simplify `GetMyAuctionsQueryHandler` to just call `GetAuctionsQuery` with the seller filter set.

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`
- `dotnet test tests/Auction.Application.Tests/`

---

## Step 9 — Fix Dependency Rule Violation (BuyNowCommandHandler)

**Principle**: Dependency Rule — Application layer must not reference Infrastructure

### Problem

`BuyNowCommandHandler` imports:
```csharp
using BuildingBlocks.Infrastructure.Locking;
using Microsoft.EntityFrameworkCore;
```

Both are Infrastructure concerns leaking into the Application layer.

### Action

1. Create `Application/Abstractions/IDistributedLock.cs` (if not already in `BuildingBlocks.Application.Abstractions`)
2. Replace `using BuildingBlocks.Infrastructure.Locking` → `using BuildingBlocks.Application.Abstractions`
3. Replace `catch (DbUpdateConcurrencyException)` → Catch a domain-level exception or have the repository translate it

#### For DbUpdateConcurrencyException

**Option A**: Repository wraps it:
```csharp
public async Task UpdateWithConcurrencyCheckAsync(Auction auction, CancellationToken ct)
{
    try { _context.Update(auction); await _context.SaveChangesAsync(ct); }
    catch (DbUpdateConcurrencyException) { throw new ConcurrencyConflictException(); }
}
```

**Option B**: Use Result pattern from repository:
```csharp
public async Task<Result> TryUpdateAsync(Auction auction, CancellationToken ct);
```

### Files to Change

| File | Action |
|------|--------|
| `Application/Features/Auctions/BuyNow/BuyNowCommandHandler.cs` | Remove Infrastructure imports |
| `BuildingBlocks.Application/Abstractions/IDistributedLock.cs` | Create if missing |
| `Application/Exceptions/ConcurrencyConflictException.cs` | Create |

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`
- `dotnet test tests/Auction.Application.Tests/`

---

## Step 10 — Standardize Endpoint Error Handling

**Principle**: Consistency; Don't use magic strings

### Problem

Three different error-handling patterns exist across endpoints:

**Pattern 1** (AuctionCrudEndpoints):
```csharp
return result.IsSuccess
    ? Results.NoContent()
    : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
```

**Pattern 2** (CategoryEndpoints, BrandEndpoints):
```csharp
if (!result.IsSuccess)
{
    return result.Error?.Code == "Category.NotFound"   // ← Magic string
        ? Results.NotFound(ProblemDetailsHelper.FromError(result.Error))
        : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
}
```

**Pattern 3** (BookmarkEndpoints):
```csharp
if (result.IsFailure)
{
    if (result.Error.Code == AuctionErrors.Auction.NotFound.Code)
        return Results.NotFound(result.Error.Message);
    return Results.Problem(ProblemDetailsHelper.FromError(result.Error));
}
```

### Action

1. Create a shared extension method:

```csharp
public static class ResultExtensions
{
    public static IResult ToApiResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess(result.Value!);

        return result.Error!.Code.EndsWith(".NotFound")
            ? Results.NotFound(ProblemDetailsHelper.FromError(result.Error))
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error));
    }
}
```

2. Replace all three patterns with:
```csharp
var result = await mediator.Send(command, ct);
return result.ToApiResult(_ => Results.NoContent());
```

3. Replace hardcoded `"Category.NotFound"` / `"Brand.NotFound"` with `AuctionErrors.Category.NotFound.Code`.

### Files to Change

| File | Changes |
|------|---------|
| `Api/Extensions/ResultExtensions.cs` | New file |
| `Api/Endpoints/Auctions/AuctionCrudEndpoints.cs` | Standardize |
| `Api/Endpoints/Auctions/AuctionQueryEndpoints.cs` | Standardize |
| `Api/Endpoints/Categories/CategoryEndpoints.cs` | Remove magic strings |
| `Api/Endpoints/Brands/BrandEndpoints.cs` | Remove magic strings |
| `Api/Endpoints/Bookmarks/BookmarkEndpoints.cs` | Standardize |
| `Api/Endpoints/Reviews/ReviewEndpoints.cs` | Standardize |

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`

---

## Step 11 — Consolidate Duplicate DTOs

**Principle**: DRY; Single source of truth

### Problem

| DTO | Location 1 | Location 2 |
|-----|-----------|-----------|
| `BuyNowResultDto` | `DTOs/Auctions/BuyNowDto.cs` | Referenced in `BuyNowCommand` result |
| `CreateAuctionDto` | `DTOs/CreateAuctionDto.cs` | Nearly duplicated by `DTOs/Auctions/CreateAuctionWithFileIdsDto.cs` |
| `AuctionFileDto` | `DTOs/AuctionDto.cs` (nested) | `DTOs/Auctions/CreateAuctionWithFileIdsDto.cs` has `CreateAuctionFileInputDto` |

### Action

1. **Delete** `DTOs/CreateAuctionDto.cs` if unused (only `CreateAuctionWithFileIdsDto` is used by endpoint)
2. **Consolidate** file DTOs: Use `CreateAuctionFileDto` (from Command record) as the single file input DTO
3. **Move** `BuyNowResultDto` to a single location, remove duplicates
4. Reorganize DTOs:

```
Application/DTOs/
├── Auctions/
│   ├── AuctionDto.cs
│   ├── AuctionFileDto.cs
│   ├── AuctionFilterDto.cs
│   ├── BuyNowResultDto.cs
│   ├── CreateAuctionRequestDto.cs     ← Renamed from CreateAuctionWithFileIdsDto
│   ├── GetAuctionsRequest.cs
│   ├── GetMyAuctionsRequest.cs
│   ├── SellerStatsDto.cs
│   └── UpdateAuctionDto.cs
├── Bookmarks/
├── Brands/
├── Categories/
└── Reviews/
```

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`

---

## Step 12 — Break Up God Repository Class

**Principle**: SRP — A class should have one reason to change

### Problem

`AuctionRepository` is **~600 lines** implementing both `IAuctionReadRepository` (25+ methods) and `IAuctionWriteRepository` (8 methods). It also contains a private `SaleProjection` class.

### Action

Split into focused repository classes (aligned with Step 4's interfaces):

```
Infrastructure/Persistence/Repositories/
├── AuctionReadRepository.cs           → Core reads (GetById, Exists, GetByIds)
├── AuctionWriteRepository.cs          → Writes (Create, Update, Delete, Range ops)
├── AuctionQueryRepository.cs          → Paged/filtered queries
├── AuctionAnalyticsRepository.cs      → Stats, revenue, trending
├── AuctionSchedulingRepository.cs     → Job queries (finished, expiring, scheduled)
```

All share `AuctionDbContext` via constructor injection. Each class is ~80-120 lines.

### Shared Concerns

Extract common query building:

```csharp
internal static class AuctionQueryExtensions
{
    internal static IQueryable<Auction> ActiveAuctions(this AuctionDbContext context)
        => context.Auctions.Where(x => !x.IsDeleted);

    internal static IQueryable<Auction> WithItemAndRelations(this IQueryable<Auction> query)
        => query.Include(x => x.Item).ThenInclude(i => i!.Category)
                .Include(x => x.Item).ThenInclude(i => i!.Brand);
}
```

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`
- `dotnet test tests/Auction.Application.Tests/`

---

## Step 13 — Split Monolithic Mapping Profile

**Principle**: SRP — Each profile maps one aggregate

### Problem

`MappingProfiles.cs` maps **all entities** (Auction, Item, Category, Brand, Review, MediaFile) in one class. Any change to any mapping requires touching this file.

### Action

Split into:

```
Application/Mappings/
├── AuctionMappingProfile.cs
├── CategoryMappingProfile.cs
├── BrandMappingProfile.cs
├── ReviewMappingProfile.cs
└── MediaFileMappingProfile.cs
```

AutoMapper will auto-discover all `Profile` classes in the assembly.

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`

---

## Step 14 — Remove Direct Repository Injection from Endpoints

**Principle**: Endpoints should be thin; delegate all logic to handlers

### Problem

`AuctionCrudEndpoints` injects `IAuctionWriteRepository` directly into endpoint methods for authorization:

```csharp
private static async Task<IResult> UpdateAuction(
    Guid id,
    UpdateAuctionDto dto,
    HttpContext httpContext,
    IMediator mediator,
    IAuctionWriteRepository auctionRepository,  // ← Repository in endpoint
    CancellationToken ct)
```

This violates the principle that endpoints should only know about `IMediator`.

### Action

This is completed as part of **Step 3**. After removing `PreloadedAuction`, endpoints no longer need repository injection. Authorization is handled by:
- Handler checking ownership, **or**
- A pipeline behavior

### Verify

- Ensure no endpoint file imports any repository interface

---

## Step 15 — Remove Misleading Async Methods

**Principle**: Functions should do what their name promises

### Problem

Several repository methods are `async` but never `await` anything:

```csharp
public async Task UpdateAsync(Auction auction, CancellationToken ct)
{
    auction.UpdatedAt = _dateTime.UtcNow;
    _context.Auctions.Update(auction);
    await Task.CompletedTask;  // ← Misleading
}

public async Task DeleteAsync(Auction auction, CancellationToken ct)
{
    auction.IsDeleted = true;
    await Task.CompletedTask;  // ← Misleading
}
```

### Action

Remove `async` keyword and `await Task.CompletedTask`:

```csharp
public Task UpdateAsync(Auction auction, CancellationToken ct = default)
{
    auction.UpdatedAt = _dateTime.UtcNow;
    _context.Auctions.Update(auction);
    return Task.CompletedTask;
}

public Task DeleteAsync(Auction auction, CancellationToken ct = default)
{
    auction.IsDeleted = true;
    auction.DeletedAt = _dateTime.UtcNow;
    auction.DeletedBy = _auditContext.UserId;
    return Task.CompletedTask;
}
```

### Files to Change

| File | Methods |
|------|---------|
| `AuctionRepository.cs` | `UpdateAsync(Auction)`, `DeleteAsync(Auction)`, `AddRangeAsync`, `UpdateRangeAsync` |

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`

---

## Step 16 — Add Missing Validators

**Principle**: Fail fast; validate at the boundary

### Problem

These commands have **no validator**:

| Command | Validation Needed |
|---------|-------------------|
| `BuyNowCommand` | `AuctionId` not empty, `BuyerId` not empty |
| `DeleteAuctionCommand` | `Id` not empty |
| `ActivateAuctionCommand` | `AuctionId` not empty |
| `DeactivateAuctionCommand` | `AuctionId` not empty |
| `GetAuctionByIdQuery` | `Id` not empty |
| `GetAuctionsByIdsQuery` | `Ids` not empty, max count |

### Action

Create validators for each:

```
Application/Features/Auctions/BuyNow/BuyNowCommandValidator.cs
Application/Features/Auctions/DeleteAuction/DeleteAuctionCommandValidator.cs
Application/Features/Auctions/ActivateAuction/ActivateAuctionCommandValidator.cs
Application/Features/Auctions/DeactivateAuction/DeactivateAuctionCommandValidator.cs
Application/Features/Auctions/GetAuctionById/GetAuctionByIdQueryValidator.cs
Application/Features/Auctions/GetAuctionsByIds/GetAuctionsByIdsQueryValidator.cs
```

**Example — BuyNowCommandValidator.cs:**
```csharp
public class BuyNowCommandValidator : AbstractValidator<BuyNowCommand>
{
    public BuyNowCommandValidator()
    {
        RuleFor(x => x.AuctionId).NotEmpty();
        RuleFor(x => x.BuyerId).NotEmpty();
        RuleFor(x => x.BuyerUsername).NotEmpty();
    }
}
```

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`
- `dotnet test tests/Auction.Application.Tests/`

---

## Step 17 — Clean Up Global Usings

**Principle**: Import only what you need; avoid polluting scope

### Current Issues

- `Application/GlobalUsings.cs` imports individual DTO types and specific interfaces
- `Infrastructure/GlobalUsings.cs` uses `BuildingBlocks.Infrastructure.Caching` and `BuildingBlocks.Infrastructure.Repository` which cause ambiguities

### Action

**Application/GlobalUsings.cs** — Keep namespace-level imports, remove type-level:

```csharp
global using BuildingBlocks.Application.Abstractions;
global using BuildingBlocks.Application.CQRS;
global using Auctions.Application.Interfaces;
global using Auctions.Application.DTOs;
```

Remove individual type imports like:
```csharp
// Remove these:
global using BuildingBlocks.Application.Abstractions.IDateTimeProvider;
global using BuildingBlocks.Application.Abstractions.ISanitizationService;
```

**Infrastructure/GlobalUsings.cs** — Add explicit aliases for ambiguous types:

```csharp
global using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;
global using ICacheService = BuildingBlocks.Application.Abstractions.ICacheService;
```

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`

---

## Step 18 — Improve Domain Entity Factory Methods

**Principle**: Encapsulation; meaningful names

### Problem

1. `Auction.Create()` has **8 parameters** — too many for a constructor-like method
2. `Auction.CreateForSeeding()` is test infrastructure leaking into domain
3. `Auction.CreateSnapshot()` is a copy mechanism that belongs in a mapper or extension

### Action

#### 18a. Use Builder Pattern for Creation

```csharp
public class AuctionBuilder
{
    private Guid _sellerId;
    private string _sellerUsername = string.Empty;
    private Item _item = null!;
    private decimal _reservePrice;
    private DateTimeOffset _auctionEnd;
    private string _currency = "USD";
    private decimal? _buyNowPrice;
    private bool _isFeatured;

    public AuctionBuilder WithSeller(Guid id, string username) { ... return this; }
    public AuctionBuilder WithItem(Item item) { ... return this; }
    public AuctionBuilder WithPricing(decimal reserve, decimal? buyNow = null) { ... return this; }
    public AuctionBuilder WithEnd(DateTimeOffset end) { ... return this; }
    public AuctionBuilder WithCurrency(string currency) { ... return this; }
    public AuctionBuilder AsFeatured() { ... return this; }
    public Auction Build() { ... }
}
```

#### 18b. Move `CreateForSeeding` to Test Project

Move to `tests/Auction.Domain.Tests/Builders/AuctionTestBuilder.cs` or use reflection.

#### 18c. Move `CreateSnapshot` to Extension Method

```csharp
public static class AuctionExtensions
{
    public static Auction ToSnapshot(this Auction source) { ... }
}
```

### Verify

- `dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj`
- `dotnet test tests/Auction.Domain.Tests/`

---

## Verification Checklist

After completing all steps, run:

```bash
# Build
dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj

# Unit tests
dotnet test tests/Auction.Application.Tests/
dotnet test tests/Auction.Domain.Tests/

# Check for remaining warnings
dotnet build src/Services/Auction/Auction.Api/Auction.Api.csproj --no-incremental 2>&1 | grep -c "warning"
```

### Clean Code Metrics (Target)

| Metric | Before | Target |
|--------|--------|--------|
| Max file length | ~600 lines (AuctionRepository) | < 200 lines |
| Max interface methods | 25+ (IAuctionReadRepository) | < 8 per interface |
| Fully qualified types | 10+ occurrences | 0 |
| Duplicate handlers | 2 (GetAuctions/GetMyAuctions) | 0 |
| Dead code files | 1 (CachedAuctionViewRepository) | 0 |
| Commands with domain entity params | 4 | 0 |
| Missing validators | 6 | 0 |
| Infrastructure imports in Application | 2 (BuyNowCommandHandler) | 0 |
| Comments | 1 | 0 |

---

## Implementation Order (Recommended)

| Priority | Step | Risk | Effort |
|----------|------|------|--------|
| 1 | Step 1 — Remove comments | None | 5 min |
| 2 | Step 7 — Remove empty decorator | None | 10 min |
| 3 | Step 15 — Fix misleading async | None | 15 min |
| 4 | Step 6 — Fix timestamp ownership | Low | 20 min |
| 5 | Step 2 — Resolve namespace ambiguities | Low | 30 min |
| 6 | Step 17 — Clean up global usings | Low | 20 min |
| 7 | Step 1 — Add missing validators | Low | 30 min |
| 8 | Step 11 — Consolidate DTOs | Low | 30 min |
| 9 | Step 13 — Split mapping profile | Low | 20 min |
| 10 | Step 10 — Standardize error handling | Medium | 45 min |
| 11 | Step 8 — Eliminate duplicate handlers | Medium | 30 min |
| 12 | Step 9 — Fix dependency rule violation | Medium | 45 min |
| 13 | Step 3 — Fix leaky abstractions | Medium | 1-2 hr |
| 14 | Step 14 — Remove repo from endpoints | Medium | (Part of Step 3) |
| 15 | Step 4 — Split fat interface | High | 1-2 hr |
| 16 | Step 12 — Break up god repository | High | 2-3 hr |
| 17 | Step 5 — Extract business logic | High | 1-2 hr |
| 18 | Step 18 — Improve factory methods | Medium | 1 hr |

**Total estimated effort: 10-14 hours**
