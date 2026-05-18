Plan: Remove Over-Engineering Across Auction Platform
The codebase has ~20 identifiable over-engineering patterns ranging from dead code to structural duplication. This plan targets them in order of value vs. risk — quick wins first, structural changes last. No functional behaviour changes.

Group A — Dead Code Deletion (Zero Risk, Zero Effort)
Delete Specification pattern — ISpecification.cs and Specification.cs have zero usages outside themselves. Services use inline LINQ. ~120 LoC removed.

Delete Event Versioning infrastructure — EventUpcaster.cs and IVersionedDomainEvent.cs have zero usages in any service. Pre-emptive CQRS/ES scaffolding that was never wired. ~70 LoC removed.

Delete IDomainService marker interface — IDomainService.cs has zero implementations anywhere. 3 lines, pure noise.

Delete IFeatureFlagService + FeatureFlagService — IFeatureFlagService.cs has zero call sites in any service. Use IFeatureManager directly if ever needed. ~90 LoC removed.

Group B — Merge Redundant Behaviours (Low Risk, Small Effort)
Merge PerformanceBehavior into LoggingBehavior — PerformanceBehavior.cs allocates a second Stopwatch per request and logs a warning when >500ms. LoggingBehavior.cs already logs elapsed time on every request. Add a > 500ms warning branch to LoggingBehavior, delete PerformanceBehavior, remove its DI registration in all services.

Remove duplicate IRepository in Infrastructure — BuildingBlocks.Infrastructure/Repository/IRepository.cs redefines the same CRUD interface already in BuildingBlocks.Application/Abstractions/IRepository.cs. Delete the Infrastructure copy; update the one reference.

Group C — Collapse Per-Service UnitOfWork Sub-Interfaces (Low Risk, Small Effort)
Delete 4 empty IUnitOfWork sub-interfaces in Notification, Storage, Job, and Bidding — each is just public interface IUnitOfWork : BuildingBlocks.IUnitOfWork { } adding nothing. Replace usages with the BuildingBlocks interface directly. The Analytics IUnitOfWork (has a Reports property) and Auction one stay. ~50 LoC removed.
Group D — Remove Duplicate Error Handling in Handlers (Low Risk, Medium Effort)
Strip try/catch from ~20 handlers that catch DomainException and return Result.Failure(ex.Message). The global ExceptionHandlingMiddleware already does this. Handlers should let exceptions propagate. Also fix the string-based ex.GetType().Name == "StripeException" type check in BuyNowCommandHandler — replace with catch (StripeException ex).
Group E — Fix Dead Value Objects (Medium Risk, Medium Effort)
Wire AuctionDuration or delete it — AuctionDuration.cs enforces min 1hr / max 30-day constraints but Auction.Create(...) takes a raw DateTimeOffset AuctionEnd — the value object is never called, meaning duration limits are silently unenforced. Either call AuctionDuration.Create(start, end) in Auction.Create or delete the class and add the rule to the FluentValidation validator.

Wire BidAmount or delete it — BidAmount.cs has operator overloads and validation but Bid stores a raw decimal Amount. The validation is duplicated in PlaceBidCommandValidator. Either use BidAmount as the property type (add EF value converter) or delete and keep the decimal + validator rule.

Group F — Reduce Auction Repository Interface Sprawl (Medium Risk, Medium Effort)
Collapse 7 IAuction*Repository interfaces to 2 — IAuctionQueryRepository, IAuctionWriteRepository, IAuctionSchedulerRepository, IAuctionUserRepository, IAuctionExportRepository all have a single implementation in AuctionRepository. Consolidate to IAuctionReadRepository + IAuctionWriteRepository. Update AuctionCacheRepository to implement those 2. Update all DI registrations and constructor injection sites.
Group G — Analytics In-Memory Aggregation (Low Risk, Medium Effort)
Replace in-memory LINQ aggregation with SQL in BidAnalyticsService.cs — it calls .ToListAsync() and then computes today/week/month counts in C# LINQ. Replace with GroupBy + COUNT SQL projections. This is a latent O(n) memory bomb as fact tables grow.
Group H — Structural / Architectural (High Effort, Assess First)
Consolidate dual bid placement paths — PlaceBidCommandHandler (API path) and BidPlacementService (consumer path) both implement full lock → validate → create → save → publish. Evaluate whether BidPlacementService consumers can dispatch PlaceBidCommand via MediatR and share the single implementation. ~300 LoC of duplication eliminated, but requires careful integration testing.

Downgrade Brand + Category from AggregateRoot to BaseEntity — Brand.cs and Category.cs are lookup tables that emit no domain events and are never saga participants. Removing AggregateRoot removes the optimistic concurrency token and domain event overhead from reference data.

Further Considerations
Group A + B + C together in one PR — all zero/low-risk deletions can be batched. Groups D–G warrant individual PRs per finding for easier review and rollback isolation.
Finding 9 (Auction repo interfaces) vs Finding 13 (dual bid paths) — do 9 before 13; consolidating the repo interfaces first makes the bid placement merge cleaner.
Analytics in-memory aggregation (Finding 12) — profile first with EXPLAIN ANALYZE on the Postgres queries before rewriting; confirm the fact tables are large enough to justify the change now vs. a future ticket.
# Over-Engineering Removal Plan

> Audit date: May 2026 — Principal Engineer Review  
> Scope: All services under `src/` + `BuildingBlocks`  
> Principle: Remove abstraction that adds no value, consolidate duplication, enforce what is defined

---

## Group A — Dead Code Deletion
> Risk: **None** · Effort: **XS** · Batch into a single PR

### A1 · Delete Specification Pattern
- **Files:** `src/BuildingBlocks/BuildingBlocks.Domain/Specifications/ISpecification.cs`, `Specification.cs`
- **Why:** Zero usages outside the files themselves. All services use inline LINQ predicates.
- **Remove:** ~120 LoC

### A2 · Delete Event Versioning Infrastructure
- **Files:** `src/BuildingBlocks/BuildingBlocks.Domain/Events/EventUpcaster.cs`, `IVersionedDomainEvent.cs`
- **Why:** No domain event in any service inherits `IVersionedDomainEvent`. No upcaster is ever registered or called. Pre-emptive scaffolding that was never wired.
- **Remove:** ~70 LoC

### A3 · Delete `IDomainService` Marker Interface
- **File:** `src/BuildingBlocks/BuildingBlocks.Domain/IDomainService.cs`
- **Why:** Zero implementations anywhere in the codebase. Pure noise.
- **Remove:** 3 LoC

### A4 · Delete `IFeatureFlagService` + `FeatureFlagService`
- **Files:** `src/BuildingBlocks/BuildingBlocks.Application/Abstractions/IFeatureFlagService.cs`, `FeatureFlagService.cs`
- **Why:** Zero call sites in any service. The implementation wraps `IConfiguration` in 3 trivial async wrappers. If feature flags are needed, use `Microsoft.FeatureManagement` directly.
- **Remove:** ~90 LoC

---

## Group B — Merge Redundant Pipeline Behaviours
> Risk: **Low** · Effort: **XS** · Single PR

### B1 · Merge `PerformanceBehavior` into `LoggingBehavior`
- **Files:** `src/BuildingBlocks/BuildingBlocks.Application/Behaviors/PerformanceBehavior.cs`, `LoggingBehavior.cs`
- **Why:** `PerformanceBehavior` allocates a second `Stopwatch` per request to log a warning when elapsed > 500ms. `LoggingBehavior` already logs elapsed time on every request — add a `> 500ms` warning branch there and delete `PerformanceBehavior`.
- **Steps:**
  1. Add `if (elapsed > 500) _logger.LogWarning(...)` branch to `LoggingBehavior`
  2. Delete `PerformanceBehavior.cs`
  3. Remove its DI registration from all `ServiceCollectionExtensions` files
- **Remove:** ~65 LoC + 1 DI registration per service

### B2 · Delete Duplicate `IRepository` in Infrastructure
- **Files:** `src/BuildingBlocks/BuildingBlocks.Infrastructure/Repository/IRepository.cs`
- **Why:** Redefines the same CRUD interface already in `BuildingBlocks.Application/Abstractions/IRepository.cs`. Delete the Infrastructure copy and update the one reference pointing to it.
- **Remove:** ~30 LoC

---

## Group C — Collapse Per-Service `IUnitOfWork` Sub-Interfaces
> Risk: **Low** · Effort: **S** · Single PR

### C1 · Delete 4 Empty `IUnitOfWork` Sub-Interfaces
- **Files:**
  - `Notification.Application/.../IUnitOfWork.cs`
  - `Storage.Application/.../IUnitOfWork.cs`
  - `Job.Application/.../IUnitOfWork.cs`
  - `Bidding.Application/.../IUnitOfWork.cs`
- **Why:** Each is literally `public interface IUnitOfWork : BuildingBlocks.IUnitOfWork { }` — zero added methods. Replace all constructor injection sites with the BuildingBlocks interface directly.
- **Keep:** `Analytics.IUnitOfWork` (has `Reports` property) and `Auction.IUnitOfWork` (unchanged)
- **Remove:** ~50 LoC + 4 redundant interface files

---

## Group D — Remove Duplicate Error Handling in Handlers
> Risk: **Low** · Effort: **M** · Single PR (~20 files)

### D1 · Strip `try/catch` from ~20 Handlers
- **Why:** The global `ExceptionHandlingMiddleware` already catches `DomainException`, `NotFoundException`, and generic `Exception` and maps them to correct HTTP responses. Handlers that catch `DomainException` and return `Result.Failure(ex.Message)` are duplicating this — and masking the exception from the middleware's structured logging.
- **Pattern to remove:**
  ```csharp
  // DELETE THIS PATTERN from handlers:
  catch (DomainException ex)
  {
      _logger.LogWarning(...);
      return Result.Failure(ex.Message);
  }