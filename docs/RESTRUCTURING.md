# 🏗️ Folder Structure Restructuring Plan

> **Date:** March 14, 2026  
> **Status:** Approved  
> **Impact:** High — touches all projects, CI/CD, Dockerfiles, and solution file

---

## 📋 Issues Found

### 1. ❌ Contracts Separated from Their Owning Service
**Current:** All contracts live in a global `src/Contracts/` folder (10 projects).  
**Problem:** Violates microservice autonomy. When you work on AuctionService, you need to jump to a completely different folder tree to edit its contracts. Makes it unclear which team/service owns which contracts.

### 2. ❌ Inconsistent Service Architecture
**Current:**
- **Clean Architecture (4 layers):** Auction, Bidding, Payment, Notification, Job, Storage ✅
- **Monolithic single-project:** Identity, Search, Analytics ❌

**Problem:** Identity.Api has 15+ folders (Controllers, DTOs, Data, DomainEvents, EventHandlers, Filters, Grpc, Helpers, Interfaces, Jobs, Mappings, Migrations, Models, Services) — everything crammed into one project with no separation of concerns.

### 3. ❌ Tests Dumped Flat at Root Level
**Current:** `tests/Auction.Domain.Tests/`, `tests/Bidding.Application.Tests/`, etc. — all mixed together.  
**Problem:** No clear ownership. When adding a new service, you have to remember to add tests in a completely separate directory. Hard to find related tests.

### 4. ❌ Dockerfile Has Wrong File Names
**Current:** `src/Services/Auction/Auction.Api/Dockerfile` references:
- `Auctions.Api.csproj` → actual file is `Auction.Api.csproj` (plural mismatch)
- `Auctions.Domain.csproj` → actual file is `Auction.Domain.csproj`
- `src/Services/Auction/Auction.Contracts/Auctions.Contracts.csproj` → this path doesn't exist
- `ENTRYPOINT ["dotnet", "Auctions.Api.dll"]` → wrong DLL name

### 5. ❌ No Centralized Build Configuration
**Current:** No `Directory.Build.props` or `Directory.Packages.props` at root.  
**Problem:** Every `.csproj` duplicates `<TargetFramework>net9.0</TargetFramework>`, `<Nullable>enable</Nullable>`, etc. Package versions like MassTransit `8.4.0` and EF Core `9.0.13` are repeated in every project — version drift risk.

### 6. ❌ Inconsistent RootNamespace in csproj
**Current:** Some projects use `Auctions.Infrastructure` (plural) while folder is `Auction.Infrastructure` (singular). Mismatch between folder names and namespace convention.

### 7. ❌ `.shared/ui-ux-pro-max/` — Unclear Purpose
**Current:** Hidden folder with unclear naming convention. Not referenced by any project.

---

## 🎯 Target Structure

```
auction-platform-microservices/
├── Directory.Build.props               # NEW — shared build properties
├── Directory.Packages.props            # NEW — centralized NuGet versions
├── global.json                         # NEW — pin .NET SDK 9.0.x
├── auction.sln                         # UPDATED — new project paths
├── .github/workflows/                  # (keep)
├── docs/                               # NEW — architecture docs, ADRs
│   ├── RESTRUCTURING.md
│   └── adr/
│
├── deploy/
│   ├── docker/
│   │   ├── docker-compose.yml          # UPDATED — fix build contexts
│   │   └── scripts/
│   ├── config/
│   └── kubernetes/                     # (keep as-is)
│
├── src/
│   ├── BuildingBlocks/                 # (keep as-is)
│   │   ├── BuildingBlocks.Domain/
│   │   ├── BuildingBlocks.Application/
│   │   ├── BuildingBlocks.Infrastructure/
│   │   └── BuildingBlocks.Web/
│   │
│   ├── Contracts/                      # SLIMMED — only cross-cutting
│   │   └── Common.Contracts/
│   │
│   ├── Gateway/
│   │   └── Gateway.Api/
│   │
│   ├── Orchestration/
│   │   ├── Orchestration.Sagas/
│   │   └── Orchestration.Contracts/    # MOVED from src/Contracts/OrchestrationService.Contracts
│   │
│   └── Services/
│       ├── Auction/
│       │   ├── Auction.Contracts/      # MOVED from src/Contracts/AuctionService.Contracts
│       │   ├── Auction.Domain/
│       │   ├── Auction.Application/
│       │   ├── Auction.Infrastructure/
│       │   ├── Auction.Api/
│       │   └── tests/
│       │       ├── Auction.Domain.Tests/
│       │       └── Auction.Application.Tests/
│       │
│       ├── Bidding/
│       │   ├── Bidding.Contracts/      # MOVED from src/Contracts/BidService.Contracts
│       │   ├── Bidding.Domain/
│       │   ├── Bidding.Application/
│       │   ├── Bidding.Infrastructure/
│       │   ├── Bidding.Api/
│       │   └── tests/
│       │       ├── Bidding.Domain.Tests/
│       │       └── Bidding.Application.Tests/
│       │
│       ├── Payment/
│       │   ├── Payment.Contracts/
│       │   ├── Payment.Domain/
│       │   ├── Payment.Application/
│       │   ├── Payment.Infrastructure/
│       │   ├── Payment.Api/
│       │   └── tests/
│       │
│       ├── Notification/
│       │   ├── Notification.Contracts/
│       │   ├── ... (Domain/Application/Infrastructure/Api)
│       │   └── tests/
│       │
│       ├── Identity/
│       │   ├── Identity.Contracts/
│       │   ├── Identity.Api/           # (keep as-is for now, refactor later)
│       │   └── tests/
│       │
│       ├── Search/
│       │   ├── Search.Contracts/
│       │   ├── Search.Api/
│       │   └── tests/
│       │
│       ├── Analytics/
│       │   ├── Analytics.Api/
│       │   └── tests/
│       │
│       ├── Job/
│       │   ├── Job.Contracts/
│       │   ├── ... (Domain/Application/Infrastructure/Api)
│       │   └── tests/
│       │       └── Job.Domain.Tests/
│       │
│       └── Storage/
│           ├── Storage.Contracts/
│           ├── ... (Domain/Application/Infrastructure/Api)
│           └── tests/
│
├── tests/                               # REPURPOSED — integration/E2E only
│   └── (future: Integration.Tests/)
│
└── web/                                 # (keep as-is)
```

---

## 📝 Migration Steps

### Phase 1: Add Centralized Build Files (safe, no breaks)
- [x] Create `Directory.Build.props`
- [x] Create `Directory.Packages.props`
- [x] Create `global.json`

### Phase 2: Co-locate Contracts (requires csproj updates)
- [ ] Move `src/Contracts/AuctionService.Contracts/` → `src/Services/Auction/Auction.Contracts/`
- [ ] Move `src/Contracts/BidService.Contracts/` → `src/Services/Bidding/Bidding.Contracts/`
- [ ] Move `src/Contracts/PaymentService.Contracts/` → `src/Services/Payment/Payment.Contracts/`
- [ ] Move `src/Contracts/NotificationService.Contracts/` → `src/Services/Notification/Notification.Contracts/`
- [ ] Move `src/Contracts/IdentityService.Contracts/` → `src/Services/Identity/Identity.Contracts/`
- [ ] Move `src/Contracts/JobService.Contracts/` → `src/Services/Job/Job.Contracts/`
- [ ] Move `src/Contracts/StorageService.Contracts/` → `src/Services/Storage/Storage.Contracts/`
- [ ] Move `src/Contracts/SearchService.Contracts/` → `src/Services/Search/Search.Contracts/`
- [ ] Move `src/Contracts/OrchestrationService.Contracts/` → `src/Orchestration/Orchestration.Contracts/`
- [ ] Keep `src/Contracts/Common.Contracts/` in place
- [ ] Update ALL `ProjectReference` paths in every `.csproj`
- [ ] Update `auction.sln` with new paths

### Phase 3: Co-locate Tests (requires csproj updates)
- [ ] Move `tests/Auction.Domain.Tests/` → `src/Services/Auction/tests/Auction.Domain.Tests/`
- [ ] Move `tests/Auction.Application.Tests/` → `src/Services/Auction/tests/Auction.Application.Tests/`
- [ ] Move `tests/Bidding.Domain.Tests/` → `src/Services/Bidding/tests/Bidding.Domain.Tests/`
- [ ] Move `tests/Bidding.Application.Tests/` → `src/Services/Bidding/tests/Bidding.Application.Tests/`
- [ ] Move `tests/Job.Domain.Tests/` → `src/Services/Job/tests/Job.Domain.Tests/`
- [ ] Update test `ProjectReference` paths
- [ ] Update `auction.sln`

### Phase 4: Fix Dockerfiles
- [ ] Fix file name mismatches (`Auctions.Api.csproj` → `Auction.Api.csproj`)
- [ ] Fix COPY paths for contracts (new co-located paths)
- [ ] Fix ENTRYPOINT DLL names

### Phase 5: Fix RootNamespace Inconsistencies
- [ ] Audit all `.csproj` files for `<RootNamespace>` mismatches
- [ ] Decide on singular (`Auction`) vs plural (`Auctions`) convention

### Phase 6 (Future): Refactor Flat Services to Clean Architecture
- [ ] Identity.Api → Identity.Domain + Identity.Application + Identity.Infrastructure + Identity.Api
- [ ] Search.Api → Search.Domain + Search.Application + Search.Infrastructure + Search.Api
- [ ] Analytics.Api → same
- ⚠️ This is a large effort — do incrementally in separate PRs

---

## 🔍 References
- [Microsoft eShop Architecture](https://github.com/dotnet/eShop)
- [.NET Microservices Architecture Guide](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/)
- [Clean Architecture by Jason Taylor](https://github.com/jasontaylordev/CleanArchitecture)

