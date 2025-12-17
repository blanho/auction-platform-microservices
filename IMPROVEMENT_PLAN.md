# 🚀 Auction Platform - Comprehensive Improvement Plan

> **Purpose:** Senior Interview Preparation  
> **Created:** December 17, 2025  
> **Estimated Total Effort:** 40-60 hours

---

## 📋 Table of Contents

1. [Phase 1: Critical Security Fixes](#phase-1-critical-security-fixes)
2. [Phase 2: Data Integrity & Performance](#phase-2-data-integrity--performance)
3. [Phase 3: BidService Refactoring](#phase-3-bidservice-refactoring)
4. [Phase 4: PaymentService Refactoring](#phase-4-paymentservice-refactoring)
5. [Phase 5: Common Libraries Cleanup](#phase-5-common-libraries-cleanup)
6. [Phase 6: NotificationService Improvements](#phase-6-notificationservice-improvements)
7. [Phase 7: UtilityService Improvements](#phase-7-utilityservice-improvements)
8. [Phase 8: Frontend Consistency](#phase-8-frontend-consistency)
9. [Phase 9: Testing Strategy](#phase-9-testing-strategy)
10. [Phase 10: Documentation & Polish](#phase-10-documentation--polish)

---

## Phase 1: Critical Security Fixes

**Priority:** 🔴 CRITICAL  
**Effort:** 2-3 hours  
**Impact:** Security vulnerabilities that could fail an interview

### 1.1 IdentityService - Remove Hardcoded Secrets

**File:** `IdentityService/Config.cs`

**Current Issue:**
```csharp
ClientSecrets = new [] { new Secret("NotASecret".Sha256()) }
ClientSecrets = { new Secret("secret".Sha256()) }
```

**Fix:**
```csharp
ClientSecrets = { new Secret(configuration["Clients:NextApp:Secret"].Sha256()) }
```

**Tasks:**
- [ ] Create `appsettings.secrets.json` (gitignored) for local development
- [ ] Move all client secrets to configuration
- [ ] Update `appsettings.Development.json` with placeholder references
- [ ] Document required environment variables in README

---

### 1.2 IdentityService - Reduce Token Lifetime

**File:** `IdentityService/Config.cs`

**Current Issue:**
```csharp
AccessTokenLifetime = 3600 * 24 * 30, // 30 days - TOO LONG
```

**Fix:**
```csharp
AccessTokenLifetime = 3600, // 1 hour
RefreshTokenLifetime = 3600 * 24 * 7, // 7 days for refresh token
```

**Tasks:**
- [ ] Reduce access token to 1 hour
- [ ] Implement refresh token flow if not exists
- [ ] Update frontend to handle token refresh

---

### 1.3 IdentityService - Strengthen Password Requirements

**File:** `IdentityService/HostingExtensions.cs`

**Current Issue:**
```csharp
options.Password.RequiredLength = 6;
options.Password.RequireNonAlphanumeric = false;
```

**Fix:**
```csharp
options.Password.RequiredLength = 12;
options.Password.RequireNonAlphanumeric = true;
options.Password.RequireUppercase = true;
options.Password.RequireLowercase = true;
options.Password.RequireDigit = true;
```

**Tasks:**
- [ ] Update password requirements
- [ ] Update seed data passwords to meet requirements
- [ ] Update any password validation messages

---

### 1.4 GatewayService - Add Rate Limiting

**File:** `GatewayService/Program.cs`

**Current Issue:** No rate limiting protection

**Fix:** Add rate limiting middleware
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
    
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// In pipeline
app.UseRateLimiter();
```

**Tasks:**
- [ ] Add `Microsoft.AspNetCore.RateLimiting` package
- [ ] Configure global rate limiter
- [ ] Add specific rate limits for sensitive endpoints (login, bid)
- [ ] Add rate limit headers to responses

---

### 1.5 GatewayService - Add Security Headers

**File:** `GatewayService/Program.cs`

**Tasks:**
- [ ] Add security headers middleware
- [ ] X-Content-Type-Options: nosniff
- [ ] X-Frame-Options: DENY
- [ ] X-XSS-Protection: 1; mode=block
- [ ] Referrer-Policy: strict-origin-when-cross-origin

---

### 1.6 Enable HTTPS Metadata Validation in Production

**Files:** 
- `GatewayService/Program.cs`
- `IdentityService/HostingExtensions.cs`

**Fix:**
```csharp
options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
```

**Tasks:**
- [ ] Update Gateway JWT validation
- [ ] Update Identity JWT validation
- [ ] Update all services that validate tokens

---

### 1.7 UtilityService - Add Authorization to FilesController

**File:** `UtilityService/Controllers/FilesController.cs`

**Current Issue:** No `[Authorize]` attribute - files publicly accessible

**Tasks:**
- [ ] Add `[Authorize]` to controller class
- [ ] Add `[AllowAnonymous]` only for public file downloads if needed
- [ ] Add ownership verification for file operations

---

## Phase 2: Data Integrity & Performance

**Priority:** 🔴 HIGH  
**Effort:** 3-4 hours  
**Impact:** Data loss and performance issues

### 2.1 Fix Missing SaveChanges in Consumers

**Files:**
- `AuctionService/Infrastructure/Messaging/Consumers/AuctionUpdatedConsumer.cs`
- `AuctionService/Infrastructure/Messaging/Consumers/BidPlacedConsumer.cs`

**Current Issue:**
```csharp
await _repository.UpdateAsync(auction);
// Missing: await _context.SaveChangesAsync() or UnitOfWork.SaveChanges()
```

**Tasks:**
- [ ] Inject `IUnitOfWork` into consumers
- [ ] Add `await _unitOfWork.SaveChangesAsync()` after updates
- [ ] Add similar fix to all consumers that modify data

---

### 2.2 Fix In-Memory Pagination - AuctionService

**File:** `AuctionService/Application/Queries/GetAuctions/GetAuctionsQueryHandler.cs`

**Current Issue:**
```csharp
var allAuctions = await _repository.GetAllAsync(cancellationToken);
var query = allAuctions.AsQueryable(); // In-memory filtering!
```

**Fix:** Move filtering to repository level

**Tasks:**
- [ ] Create `GetPagedAuctionsAsync` method in repository
- [ ] Accept filter parameters (status, search, category, etc.)
- [ ] Use `IQueryable<T>` with server-side filtering
- [ ] Apply pagination at database level with `Skip/Take`
- [ ] Return `PagedResult<Auction>` with total count

---

### 2.3 Fix In-Memory Pagination - NotificationService

**File:** `NotificationService/Application/Services/NotificationService.cs`

**Same pattern as above:**
- [ ] Create paginated repository method
- [ ] Move filtering to database query

---

### 2.4 Fix In-Memory Stats - UtilityService

**File:** `UtilityService/Services/ReportService.cs`

**Current Issue:**
```csharp
var allReports = await _reportRepository.GetAllAsync(cancellationToken);
return new ReportStatsDto
{
    TotalReports = allReports.Count,
    PendingReports = allReports.Count(r => r.Status == ReportStatus.Pending),
```

**Fix:**
```csharp
return new ReportStatsDto
{
    TotalReports = await _reportRepository.CountAsync(cancellationToken),
    PendingReports = await _reportRepository.CountByStatusAsync(ReportStatus.Pending, cancellationToken),
```

**Tasks:**
- [ ] Add `CountAsync` method to repository
- [ ] Add `CountByStatusAsync` method
- [ ] Update service to use count methods

---

### 2.5 Fix Decimal to Int Cast

**File:** `AuctionService/Application/Commands/BuyNow/BuyNowCommandHandler.cs`

**Current Issue:**
```csharp
BuyNowPrice = (int)auction.BuyNowPrice!.Value, // Loses precision!
```

**Fix:**
```csharp
BuyNowPrice = auction.BuyNowPrice!.Value,
```

**Tasks:**
- [ ] Update event to use `decimal` for price
- [ ] Update all places where this event is consumed
- [ ] Verify no other int casts for money fields

---

## Phase 3: BidService Refactoring

**Priority:** 🟠 HIGH  
**Effort:** 8-10 hours  
**Impact:** Pattern consistency across services

### 3.1 Domain Layer - Fix Entity Types

**File:** `BidService/Domain/Entities/Bid.cs`

**Current Issues:**
```csharp
public int Amount { get; set; }        // Should be decimal
public DateTime BidTime { get; set; }  // Should be DateTimeOffset
public string Bidder { get; set; }     // Missing BidderId (Guid)
```

**Tasks:**
- [ ] Change `Amount` from `int` to `decimal`
- [ ] Change `BidTime` from `DateTime` to `DateTimeOffset`
- [ ] Add `BidderId` (Guid) property
- [ ] Rename `Bidder` to `BidderUsername` for clarity
- [ ] Create EF migration for schema change

---

### 3.2 Domain Layer - Fix AutoBid Entity

**File:** `BidService/Domain/Entities/AutoBid.cs`

**Same changes:**
- [ ] `MaxAmount` from `int` to `decimal`
- [ ] `LastBidAmount` from `int` to `decimal`
- [ ] Timestamps to `DateTimeOffset`
- [ ] Add `UserId` alongside `Username`

---

### 3.3 Domain Layer - Consolidate BidIncrement

**Files:**
- `BidService/Domain/ValueObjects/BidIncrement.cs`
- `BidService/Application/Services/AutoBidService.cs`

**Current Issue:** Duplicate `GetBidIncrement` with DIFFERENT values!

**Tasks:**
- [ ] Remove duplicate method from `AutoBidService`
- [ ] Use `BidIncrement.GetIncrement()` everywhere
- [ ] Consider making increments configurable

---

### 3.4 Infrastructure - Update EF Configuration

**File:** `BidService/Infrastructure/Data/Configurations/BidConfiguration.cs`

**Tasks:**
- [ ] Add `HasPrecision(18, 2)` for Amount
- [ ] Update index configurations
- [ ] Add BidderId index

---

### 3.5 Application Layer - Implement CQRS

**Current Structure:**
```
BidService/Application/
├── Services/
│   ├── BidService.cs
│   └── AutoBidService.cs
```

**Target Structure:**
```
BidService/Application/
├── Commands/
│   ├── PlaceBid/
│   │   ├── PlaceBidCommand.cs
│   │   ├── PlaceBidCommandHandler.cs
│   │   └── PlaceBidCommandValidator.cs
│   ├── CreateAutoBid/
│   │   └── ...
│   └── CancelAutoBid/
│       └── ...
├── Queries/
│   ├── GetBidsForAuction/
│   │   ├── GetBidsForAuctionQuery.cs
│   │   └── GetBidsForAuctionQueryHandler.cs
│   ├── GetBidderBids/
│   │   └── ...
│   └── GetAutoBids/
│       └── ...
├── DTOs/
│   ├── BidDto.cs
│   └── AutoBidDto.cs
└── Mappings/
    └── BidMappingProfile.cs
```

**Tasks:**
- [ ] Create Command classes with `ICommand<Result<T>>`
- [ ] Create Query classes with `IQuery<T>`
- [ ] Create Handlers using MediatR
- [ ] Create Validators using FluentValidation
- [ ] Move DTOs to dedicated folder
- [ ] Create AutoMapper profile
- [ ] Update controller to use MediatR
- [ ] Remove old service classes

---

### 3.6 Application Layer - Implement Result Pattern

**Current Issue:**
```csharp
return new BidDto
{
    Status = BidStatus.Rejected.ToString(),
    ErrorMessage = validationResult.ErrorMessage  // Anti-pattern
};
```

**Fix:**
```csharp
return Result.Failure<BidDto>(BidErrors.ValidationFailed(validationResult.ErrorMessage));
```

**Tasks:**
- [ ] Define `BidErrors` static class with error definitions
- [ ] Update all handlers to return `Result<T>`
- [ ] Update controller to handle Result pattern

---

### 3.7 API Layer - Add Versioning & CancellationToken

**File:** `BidService/API/Controllers/BidsController.cs`

**Tasks:**
- [ ] Add `[ApiVersion("1.0")]` attribute
- [ ] Update route to `api/v{version:apiVersion}/[controller]`
- [ ] Add `CancellationToken` to all action methods

---

### 3.8 Infrastructure - Remove Redundant IsDeleted Checks

**File:** `BidService/Infrastructure/Data/Repositories/BidRepository.cs`

**Current Issue:** Manual `IsDeleted` checks when global query filter exists

**Tasks:**
- [ ] Remove manual `.Where(b => !b.IsDeleted)` from queries
- [ ] Rely on EF Core global query filter

---

## Phase 4: PaymentService Refactoring

**Priority:** 🟠 HIGH  
**Effort:** 6-8 hours  
**Impact:** Clean Architecture compliance

### 4.1 Add Service Layer

**Current Issue:** All business logic in controllers

**Target Structure:**
```
PaymentService/Application/
├── Interfaces/
│   ├── IOrderService.cs
│   └── IWalletService.cs
├── Services/
│   ├── OrderService.cs
│   └── WalletService.cs
├── DTOs/
│   └── (existing)
└── Mappings/
    └── (existing)
```

**Tasks:**
- [ ] Create `IOrderService` interface
- [ ] Create `IWalletService` interface
- [ ] Implement `OrderService` with business logic
- [ ] Implement `WalletService` with business logic
- [ ] Move controller logic to services
- [ ] Register services in DI

---

### 4.2 Add Transaction Support for Wallet Operations

**Current Issue:**
```csharp
wallet.Balance += dto.Amount;
await _walletRepository.UpdateAsync(wallet);
await _transactionRepository.AddAsync(transaction);
// Two separate saves - not atomic!
```

**Fix:** Use UnitOfWork pattern
```csharp
await using var transaction = await _unitOfWork.BeginTransactionAsync();
try
{
    wallet.Balance += dto.Amount;
    await _walletRepository.UpdateAsync(wallet);
    await _transactionRepository.AddAsync(walletTransaction);
    await _unitOfWork.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**Tasks:**
- [ ] Add `BeginTransactionAsync` to UnitOfWork
- [ ] Wrap wallet operations in transactions
- [ ] Add optimistic concurrency to Wallet entity

---

### 4.3 Add Authorization Checks

**Current Issue:**
```csharp
[HttpGet("{username}")]
public async Task<ActionResult<WalletDto>> GetWallet(string username)
{
    // No check if current user == username!
```

**Tasks:**
- [ ] Add user identity check in wallet operations
- [ ] Add ownership verification for orders
- [ ] Create authorization handler for resource-based auth

---

### 4.4 Fix Entity Base Inheritance

**Current Issue:** Entities don't extend `BaseEntity`

**Tasks:**
- [ ] Update `Order` to extend `BaseEntity`
- [ ] Update `Wallet` to extend `BaseEntity`
- [ ] Update `WalletTransaction` to extend `BaseEntity`
- [ ] Add audit fields (CreatedAt, UpdatedAt, CreatedBy)
- [ ] Create migration

---

### 4.5 Add User ID to Entities

**Current Issue:** Only usernames stored, no GUIDs

**Tasks:**
- [ ] Add `BuyerId` to Order
- [ ] Add `SellerId` to Order
- [ ] Add `UserId` to Wallet
- [ ] Update mappings and DTOs

---

## Phase 5: Common Libraries Cleanup

**Priority:** 🟡 MEDIUM  
**Effort:** 2-3 hours  
**Impact:** Cleaner codebase, reduced confusion

### 5.1 Delete Dead Code

**Folders to Delete:**
- [ ] `Common/Common.Security/` - Empty interfaces, never used
- [ ] `Common/Common.ServiceClients/` - Empty folder

---

### 5.2 Consolidate Distributed Locking

**Issue:** Two implementations exist

| Library | Interface | Features |
|---------|-----------|----------|
| Common.Caching | `IDistributedLock` | Simple |
| Common.Locking | `IDistributedLock` | Robust (retry, handle) |

**Tasks:**
- [ ] Keep `Common.Locking` version (better implementation)
- [ ] Remove `IDistributedLock` from `Common.Caching`
- [ ] Update all references to use `Common.Locking`

---

### 5.3 Consolidate Logger Wrapper

**Issue:** Duplicate interfaces

| Library | Interface |
|---------|-----------|
| Common.Repository | `IAppLogger<T>` |
| Common.Logging | `ILoggerAdapter<T>` |

**Tasks:**
- [ ] Remove `IAppLogger` from `Common.Repository`
- [ ] Keep logging abstractions in `Common.Logging`
- [ ] Update all services to use `Common.Logging`

---

### 5.4 Consolidate Exception Middleware

**Issue:** Two implementations

| Library | Class |
|---------|-------|
| Common.Core | `ExceptionMiddleware` |
| Common.OpenApi | `ExceptionHandlingMiddleware` |

**Tasks:**
- [ ] Merge functionality into one middleware
- [ ] Keep in `Common.Core` (more appropriate)
- [ ] Remove from `Common.OpenApi`

---

### 5.5 Consolidate DateTime Extensions

**Issue:** Duplicate extension methods

| Library | File |
|---------|------|
| Common.Core | `DateTimeExtensions.cs` |
| Common.Utilities | `DateTimeExtensions.cs` |

**Tasks:**
- [ ] Merge all DateTime extensions into `Common.Core`
- [ ] Remove from `Common.Utilities`
- [ ] Update all references

---

### 5.6 Fix Event Type Inconsistencies

**File:** `Common/Common.Messaging/Events/`

**Issue:** Some events use `int` for money, some use `decimal`

**Tasks:**
- [ ] Update `BidPlacedEvent.Amount` from `int` to `decimal`
- [ ] Update `BuyNowExecutedEvent.Price` from `int` to `decimal`
- [ ] Ensure all money fields use `decimal`
- [ ] Ensure all timestamps use `DateTimeOffset`

---

## Phase 6: NotificationService Improvements

**Priority:** 🟡 MEDIUM  
**Effort:** 3-4 hours  
**Impact:** Security and reliability

### 6.1 Add Authorization Checks

**File:** `NotificationService/API/Controllers/NotificationController.cs`

**Current Issue:**
```csharp
[HttpPut("{id}/read")]
public async Task<ActionResult> MarkAsRead(Guid id)
{
    await _notificationService.MarkAsReadAsync(id); // No ownership check!
```

**Tasks:**
- [ ] Add ownership verification before mark as read
- [ ] Add ownership verification before delete
- [ ] Extract user ID from JWT claims
- [ ] Verify notification belongs to user

---

### 6.2 Add Idempotency to Consumers

**Current Issue:** No duplicate message protection (unlike PaymentService)

**Tasks:**
- [ ] Add cache-based idempotency check to all consumers
- [ ] Use message ID as idempotency key
- [ ] Set appropriate cache expiration (24 hours)

**Example:**
```csharp
var idempotencyKey = $"notification:{context.MessageId}";
if (await _cache.GetAsync<string>(idempotencyKey) != null)
{
    _logger.LogInformation("Message already processed");
    return;
}

// Process message...

await _cache.SetAsync(idempotencyKey, "processed", TimeSpan.FromHours(24));
```

---

### 6.3 Fix DateTime Type

**File:** `NotificationService/Domain/Entities/Notification.cs`

**Tasks:**
- [ ] Change `ReadAt` from `DateTime?` to `DateTimeOffset?`
- [ ] Update any other DateTime fields
- [ ] Create migration

---

### 6.4 Add CancellationToken Propagation

**Tasks:**
- [ ] Add `CancellationToken` to controller methods
- [ ] Pass through to service methods
- [ ] Pass through to repository methods

---

## Phase 7: UtilityService Improvements

**Priority:** 🟡 MEDIUM  
**Effort:** 4-5 hours  
**Impact:** Code quality and completeness

### 7.1 Complete Stripe Webhook Handlers

**File:** `UtilityService/Services/StripePaymentService.cs`

**Current Issue:**
```csharp
private Task HandlePaymentIntentSucceeded(Event stripeEvent)
{
    _logger.LogInformation("PaymentIntent succeeded");
    return Task.CompletedTask;  // Does nothing!
}
```

**Tasks:**
- [ ] Implement `HandlePaymentIntentSucceeded` - update order status
- [ ] Implement `HandlePaymentIntentFailed` - mark order as failed
- [ ] Publish events for order status changes
- [ ] Add proper error handling

---

### 7.2 Fix AdminDashboardController

**File:** `UtilityService/Controllers/AdminDashboardController.cs`

**Current Issue:** Returns hardcoded zeros

**Tasks:**
- [ ] Create `IDashboardService` interface
- [ ] Implement actual stats queries via gRPC to other services
- [ ] Or create a read model updated by events

---

### 7.3 Complete AuditLogArchiveJob

**File:** `UtilityService/Jobs/AuditLogArchiveJob.cs`

**Current Issue:** Only logs, doesn't archive

**Tasks:**
- [ ] Implement actual archival logic
- [ ] Export old logs to file/blob storage
- [ ] Delete archived logs from database
- [ ] Or remove the job if not needed

---

### 7.4 Fix ReportAutoEscalationJob Bug

**File:** `UtilityService/Jobs/ReportAutoEscalationJob.cs`

**Current Issue:**
```csharp
report.Priority = newPriority;  // Changed here
report.AdminNotes = AppendNote(report.AdminNotes, 
    $"[Auto-escalated from {report.Priority}...]");  // Shows same value!
```

**Fix:**
```csharp
var oldPriority = report.Priority;
report.Priority = newPriority;
report.AdminNotes = AppendNote(report.AdminNotes, 
    $"[Auto-escalated from {oldPriority} to {newPriority}...]");
```

---

### 7.5 Standardize API Versioning

**Current Issue:** Inconsistent versioning across controllers

| Controller | Route |
|------------|-------|
| AdminDashboardController | `/api/v1/admin/[controller]` |
| PlatformSettingsController | `/api/v1/admin/[controller]` |
| FilesController | `/api/[controller]` (no version) |
| Others | `/api/[controller]` (no version) |

**Tasks:**
- [ ] Add API versioning to all controllers
- [ ] Standardize route format

---

### 7.6 Move Inline DTOs

**Current Issue:** DTOs defined inside controllers

**Tasks:**
- [ ] Move DTOs from `FilesController` to `DTOs/` folder
- [ ] Move DTOs from `PaymentController` to `DTOs/` folder
- [ ] Move DTOs from `ReportsController` to `DTOs/` folder

---

## Phase 8: Frontend Consistency

**Priority:** 🟡 MEDIUM  
**Effort:** 6-8 hours  
**Impact:** Code consistency with guidelines

### 8.1 Migrate Icons from Lucide to FontAwesome

**Current Issue:** 50+ files use Lucide icons, guidelines say FontAwesome

**Files to Update (partial list):**
- [ ] `Web/components/ui/accordion.tsx`
- [ ] `Web/components/ui/alert.tsx`
- [ ] `Web/components/ui/breadcrumb.tsx`
- [ ] `Web/components/ui/calendar.tsx`
- [ ] `Web/components/ui/checkbox.tsx`
- [ ] `Web/components/ui/command.tsx`
- [ ] `Web/components/ui/dialog.tsx`
- [ ] `Web/components/ui/dropdown-menu.tsx`
- [ ] `Web/components/ui/input.tsx`
- [ ] `Web/components/ui/pagination.tsx`
- [ ] `Web/components/ui/select.tsx`
- [ ] `Web/components/ui/sheet.tsx`
- [ ] `Web/components/ui/sidebar.tsx`
- [ ] `Web/components/ui/table.tsx`
- [ ] `Web/components/ui/toast.tsx`
- [ ] `Web/app/(dashboard)/` - All dashboard pages
- [ ] `Web/app/(admin)/` - All admin pages

**Tasks:**
- [ ] Create icon mapping guide (Lucide → FontAwesome)
- [ ] Update each file systematically
- [ ] Remove `lucide-react` from package.json
- [ ] Verify no broken icons

---

### 8.2 Review Unused Components

**Potentially Unused:**
- [ ] `Web/components/home/popular-categories.tsx`
- [ ] `Web/components/home/testimonials.tsx`
- [ ] `Web/components/home/newsletter.tsx`
- [ ] `Web/components/home/how-it-works.tsx`

**Tasks:**
- [ ] Verify if components are used
- [ ] Either integrate into homepage or delete
- [ ] Update exports in index.ts

---

### 8.3 Extract Inline CountdownTimer

**File:** `Web/app/(root)/auctions/[id]/page.tsx`

**Task:**
- [ ] Extract inline countdown component
- [ ] Create `Web/components/common/countdown-timer.tsx`
- [ ] Reuse across auction cards and detail page

---

## Phase 9: Testing Strategy

**Priority:** 🟠 HIGH  
**Effort:** 15-20 hours  
**Impact:** Critical for senior role demonstration

### 9.1 Unit Tests - Domain Layer

**Target Coverage:** Domain entities and value objects

**Projects to Create:**
- [ ] `AuctionService.Domain.Tests`
- [ ] `BidService.Domain.Tests`

**Test Examples:**
```csharp
public class AuctionTests
{
    [Fact]
    public void IsLive_WhenNotEndedAndNotReserveMet_ReturnsTrue()
    
    [Fact]
    public void CurrentHighBid_ReturnsHighestBidAmount()
    
    [Fact]
    public void CanBid_WhenAuctionEnded_ReturnsFalse()
}

public class BidIncrementTests
{
    [Theory]
    [InlineData(50, 5)]
    [InlineData(150, 10)]
    [InlineData(600, 25)]
    public void GetIncrement_ReturnsCorrectIncrement(int currentBid, int expectedIncrement)
}
```

---

### 9.2 Unit Tests - Application Layer

**Target Coverage:** Command/Query handlers

**Projects to Create:**
- [ ] `AuctionService.Application.Tests`

**Test Examples:**
```csharp
public class CreateAuctionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    
    [Fact]
    public async Task Handle_InvalidCategory_ReturnsFailure()
    
    [Fact]
    public async Task Handle_PublishesAuctionCreatedEvent()
}
```

---

### 9.3 Integration Tests

**Target Coverage:** API endpoints and database

**Projects to Create:**
- [ ] `AuctionService.API.IntegrationTests`

**Test Examples:**
```csharp
public class AuctionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetAuctions_ReturnsPagedResult()
    
    [Fact]
    public async Task CreateAuction_WithValidData_Returns201()
    
    [Fact]
    public async Task CreateAuction_Unauthorized_Returns401()
}
```

---

### 9.4 Frontend Tests

**Target Coverage:** Critical components and hooks

**Files to Create:**
- [ ] `Web/__tests__/hooks/useCountdown.test.ts`
- [ ] `Web/__tests__/components/AuctionCard.test.tsx`
- [ ] `Web/__tests__/services/auction.service.test.ts`

**Setup:**
- [ ] Install Jest and React Testing Library
- [ ] Configure jest.config.js
- [ ] Add test scripts to package.json

---

## Phase 10: Documentation & Polish

**Priority:** 🟢 LOW  
**Effort:** 2-3 hours  
**Impact:** Professional presentation

### 10.1 Update README.md

**Tasks:**
- [ ] Update architecture diagram
- [ ] Add technology stack badges
- [ ] Document all environment variables
- [ ] Add screenshots
- [ ] Add API documentation link

---

### 10.2 Add API Documentation

**Tasks:**
- [ ] Verify Swagger/OpenAPI is complete
- [ ] Add XML documentation to controllers
- [ ] Add request/response examples
- [ ] Document authentication requirements

---

### 10.3 Clean Up ARCHITECTURE_REVIEW.md

**Tasks:**
- [ ] Update with completed improvements
- [ ] Remove outdated issues
- [ ] Add architecture decision records (ADRs)

---

### 10.4 Add .editorconfig

**Tasks:**
- [ ] Create `.editorconfig` for consistent formatting
- [ ] Configure for C# and TypeScript
- [ ] Run formatter on all files

---

### 10.5 Add GitHub Actions (Optional)

**Tasks:**
- [ ] Create CI workflow for build
- [ ] Add test runner
- [ ] Add Docker build verification

---

## 📊 Progress Tracker

| Phase | Status | Completion |
|-------|--------|------------|
| Phase 1: Security | ⬜ Not Started | 0% |
| Phase 2: Data Integrity | ⬜ Not Started | 0% |
| Phase 3: BidService | ⬜ Not Started | 0% |
| Phase 4: PaymentService | ⬜ Not Started | 0% |
| Phase 5: Common Libraries | ⬜ Not Started | 0% |
| Phase 6: NotificationService | ⬜ Not Started | 0% |
| Phase 7: UtilityService | ⬜ Not Started | 0% |
| Phase 8: Frontend | ⬜ Not Started | 0% |
| Phase 9: Testing | ⬜ Not Started | 0% |
| Phase 10: Documentation | ⬜ Not Started | 0% |

---

## 🎯 Interview Preparation Minimum

If time is limited, complete at minimum:

1. ✅ Phase 1 (Security) - **Required**
2. ✅ Phase 2 (Data Integrity) - **Required**
3. ✅ Phase 5.1 (Delete dead code) - **Quick win**
4. ✅ Phase 9.1-9.2 (Some unit tests) - **Shows testing knowledge**

This gives you:
- Secure code to demonstrate
- No obvious bugs
- Clean codebase
- Proof of testing practices

---

## 📝 Notes

- Update this document as you complete tasks
- Check off items as you go
- Prioritize based on interview timeline
- Focus on quality over quantity
