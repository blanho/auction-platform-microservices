# Clean Code review

## Scope and approach

This review continues on `fix/auction-reliability-and-test-coverage` from
`a14006cc`, preserving the earlier reliability fixes and tests. The initial
working tree was clean, and no applicable `AGENTS.md` was found.

The inventory covers all ten services, orchestration, the gateway, shared
libraries, all ten frontend modules, and build/test tooling. Source enumeration,
project-reference guards, pattern searches, and existing tests support manual
review of the flows listed below. This is a repository-wide structural review
with targeted manual flow review, not an exhaustive line-by-line security audit.
Generated migrations and public contracts were preserved. No deployment or
real payment was performed.

## Baseline and classification

Before production changes, the Release solution built with zero warnings/errors;
304 backend tests across 30 projects, 21 frontend tests, and two Chromium fixture
journeys passed. Backend reports showed zero skipped tests with disposable
PostgreSQL and RabbitMQ.

**Reproduced bug:** auction-specific auto-bids used `['autobid', 'auction', id]`,
outside the `['autoBids']` prefix invalidated by mutations. Four real QueryClient
hook tests failed before the fix: create, update, toggle, and cancel left the
displayed data stale. The new shared `autoBidForAuction` factory puts this query
under the existing mutation prefix. All four tests then passed. This is the only
intentional production behavior change in this pass.

**Behavior-preserving refactors:**

- HTTP response handling now separates error routing, transient retries, and
  authentication refresh. Retry count, backoff/jitter, allowed HTTP methods,
  one refresh for concurrent 401 responses, and original-error propagation remain
  intact. Seven added retry tests passed against the original implementation
  before extraction; the three existing refresh cases continue to pass.
- Bid-result mapping uses an early return and a single unsupported-status
  fallback. Eight regression cases cover accepted, below-reserve, rejected,
  pending, unknown, numeric, and explicit-message outcomes, including audit and
  deduplication cleanup behavior. Cancellation coverage remains in the renamed
  `PlaceBidCommandHandlerTests.cs`.
- Saga recovery names messages, cases, identifiers, and asynchronous operations
  explicitly. Workflow and recovery-step constants are shared inside the existing
  orchestration boundary. Their persisted and transmitted string values are
  unchanged. Advisory-lock keys, receive-outbox transactions, timeout generations,
  and compensation ordering are preserved.
- Storage upload rollback now names the uncertain-commit decision and blob
  cleanup separately. Four added characterization cases passed before extraction:
  retain the blob after a matching committed record or unavailable lookup; delete
  it after a missing record or a different committed blob. The original database
  exception survives cleanup failures. Tests reuse the existing shared proxy.
- Notification preference reads and updates share one DTO mapping. Persistence
  and hub delivery ordering are unchanged.
- Identity's unused private `RevokeDescendantTokensAsync` was removed. Repository
  search found only its definition and its own recursive call. Active token-family
  and user-wide revocation paths remain intact.
- Four Analytics consumers moved from `AuctionAnalyticsConsumers.cs` into files
  named after each consumer. A comparison against the original Git revision
  verified that every class body was preserved exactly.
- `OrderDetailPage` delegates progress rendering to `OrderProgressStepper`, using
  domain-specific names and removing an unnecessary icon wrapper. Labels, dates,
  status mapping, styles, and cancelled-order visibility are preserved. The browser
  journey was extended through order detail and passed before the extraction.
- System-test Compose preparation has a named entry point and port constants.
  Three CLI tests protect project isolation, loopback/offset ports, and bounded
  readiness probes. Its resolved Compose transformation is unchanged.

## Backend review inventory

Paths below are relative to `src/Services/<service>/` unless specified otherwise.

| Area | Inspected flows and infrastructure | Outcome |
|---|---|---|
| Auction | `BuyNowCommandHandler`, aggregate reservation and sale completion, tracked auction repository, bid projection and room-update handoff, application/persistence tests | Existing service boundary and purchase/finalization behavior retained; no implementation change needed |
| Bidding | `BidPlacementService`, `PlaceBidCommandHandler`, repository queries, Redis/auction locks, accepted bid metadata and cancellation tests | Simplified outcome mapping and added regression coverage; lock/save order retained |
| Payment | Wallet handler/executor, checkout ownership/status guards, create/cancel buy-now consumers, unit of work, MassTransit outbox registration, wallet persistence tests | Existing durable idempotency and transaction guarantees retained; no implementation change needed |
| Identity | Token generation/rotation/revocation, user-service responsibilities, auth endpoints and frontend refresh integration | Removed verified dead private method; no token policy or public API changes |
| Catalog | Category update/hierarchy guards, brand/category gRPC reads and cancellation propagation, hierarchy/ownership tests | Existing cohesive handlers retained |
| Notification | Notification creation/preferences, localization, hub delivery, auction-room bridge, consumer registration and tests | Consolidated preference DTO mapping; delivery error policy retained |
| Analytics | Auction lifecycle consumers, duplicate guards, persistence/event payloads, query request shapes | Consumer files split for discovery; class bodies unchanged |
| Search | Search/query/filter/sort builders, index/get/autocomplete error and cancellation behavior, transport-request tests | Existing local query helpers and failure semantics retained |
| Storage | Single/batch uploads, uncertain commit cleanup, report idempotency, ownership/presigned confirmation/deletion and persistence tests | Single-upload rollback clarified; batch-upload behavior retained |
| Job | Single/batch result consumers, attempt/ownership guards, `JobItem` transitions and progress persistence/restart tests | Existing implementation retained; job failure-count semantics need a separate behavior decision before alteration |
| Orchestration | Both state machines, recovery consumer/endpoints, timeout dispatcher, registration/outboxes, broker restart and persistence tests | Named constants and clearer recovery/timeout code; no schema or correlation changes |
| Gateway | Authentication, hub access-token routing, CORS, rate-limit partitions, proxy startup | Existing policies retained; live API/auth journeys provide runtime checks |
| BuildingBlocks | `BaseUnitOfWork`, audit publisher, Redis cache/deduplication/locks, error/result primitives and architecture guards | Existing event/save ordering and cross-service independence retained |

## Frontend review inventory

Paths are relative to `web/src/`.

| Module | Reviewed areas | Outcome |
|---|---|---|
| auctions | Query-key factories, live subscriptions/rejoin/cleanup, bid and buy-now actions, purchase polling | Existing reliability fixes retained; lifecycle tests rerun |
| bidding | Bid and auto-bid hooks, constants, API and request schemas | Reproduced and fixed auto-bid invalidation mismatch |
| payments | Order/wallet hooks, order detail rendering, status utilities, purchase fixture journey | Extracted cohesive progress display; browser now reaches order detail |
| auth | Auth hooks/provider, token/CSRF helpers, login retention and refresh behavior | Existing auth flow retained; HTTP retry/refresh responsibilities separated |
| users | Role/review/profile mutation keys and large auction-management page | Existing behavior retained; further page decomposition is optional |
| notifications | Query factories, notification/record hooks and hub subscriptions | Existing behavior retained; missed-notification reconciliation needs separate testing |
| search | Query/suggestion/recent-search hooks and API boundary | Existing query separation retained |
| jobs | Query factories, active-job polling and cancel/retry invalidation | Existing polling behavior retained |
| analytics | Dashboard/report/settings factories and mutations | Existing factory prefixes and domain separation retained |
| home | Metrics hook and its auction/catalog query dependencies | Existing memoized aggregation retained |
| shared/app/services | HTTP and SignalR services, auth provider, snackbar, formatters, shared styling, routing/test setup | HTTP responsibilities clarified without changing transport policy |

## Tooling and documentation review

Reviewed the mandatory backend runner, TRX verifier, disposable dependency Compose,
system-stack runner and Compose transformation, frontend scripts/configuration,
CI/Sonar workflows, central build/package configuration, and README testing claims.
CI's database/broker provisioning, readiness checks, per-project result names, and
skip rejection remain in place. New Python isolation tests run through the existing
CI unittest discovery. README now describes auto-bid/retry/order-detail coverage,
the orchestration container, and the actual Sonar trigger branches.

## Validation

| Check | Result |
|---|---|
| Release solution build | Passed; zero warnings/errors |
| Backend suite | 316 passed across 30 projects; zero failed/skipped |
| Frontend behavioral suite | 32 passed across 5 files |
| Frontend formatting, ESLint, TypeScript and i18n checks | Passed |
| Frontend production build | Passed; existing large-chunk warning remains |
| Chromium fixture purchase/order-detail journeys | 2 passed |
| Python result-verifier and isolation tests | 5 passed |
| Bash runner syntax checks | Passed |
| Live disposable-stack journeys with current-code images | 3 passed; all 18 containers healthy; fresh isolated databases |
| Complete rebuild through standard Dockerfiles | Blocked by stalled Bidding NuGet restore; cancelled after over eight minutes |

The new coverage adds 12 backend cases, 11 frontend cases, and three Python cases;
the existing successful browser journey also reaches order detail. The initial
storage characterization test had a proxy-name collision, and the first complete
build reported a nullable test assertion warning. Both were corrected before the
final passing runs. Four deliberate pre-fix auto-bid failures are separate from
the final suite results.

Backend TRX/OpenCover reports are under `/private/tmp/auction-refactor-final-results`.
The standard Docker build logs are under `/private/tmp/auction-refactor-system`;
the subsequent runtime run retains startup/service/cleanup logs under
`/private/tmp/auction-refactor-system-runtime`. These local temporary artifacts
are not committed. The final nullable assertion change was followed by a warning-free
solution build, a complete rerun of its bidding test project, and verification of
all 30 final project reports.

The live journeys verified authenticated gateway/profile APIs and refresh cookies,
two actual browsers observing accepted bids and low-bid rejection with authoritative
recovery after a forced transport disconnect, and the real buy-now saga producing
an unpaid order visible in the browser. The runner removed its disposable containers,
network, and volumes afterward. Final application/test checks had zero failures
and zero skips; the standard Dockerfile rebuild remains a separate blocked check.

From the repository root, with .NET 9, Node/npm, Python 3, Docker/Compose and Chromium:

```bash
docker compose -p auction-clean-code-review -f deploy/docker/docker-compose.tests.yml up -d --wait
export BACKEND_TEST_POSTGRES='Host=localhost;Port=15432;Database=postgres;Username=auction_test;Password=auction_test'
export BACKEND_TEST_RABBIT_PORT=15673
dotnet restore auction.sln -m:1
dotnet build auction.sln --no-restore -c Release -m:1
TEST_RESULTS_DIR=/private/tmp/auction-refactor-final-results bash scripts/run-backend-tests.sh
python3 -m unittest discover -s scripts/tests -v
bash -n scripts/run-backend-tests.sh scripts/run-system-tests.sh
docker compose -p auction-clean-code-review -f deploy/docker/docker-compose.tests.yml down -v

cd web
npm ci
npm run validate
npm test
npm run build
PLAYWRIGHT_BROWSERS_PATH=/private/tmp/auction-playwright npx playwright install chromium
PLAYWRIGHT_BROWSERS_PATH=/private/tmp/auction-playwright npm run test:e2e
cd ..
PLAYWRIGHT_BROWSERS_PATH=/private/tmp/auction-playwright bash scripts/run-system-tests.sh
```

The system command rebuilds images and uses fresh, isolated databases. Reserve
loopback ports 3000/6001 and stop the smaller disposable test dependencies first
to avoid unnecessary memory pressure. It exercises actual HTTP, SignalR, gRPC,
PostgreSQL, RabbitMQ and saga boundaries, stopping purchase flow at an unpaid order.

### Local Docker restore workaround

The standard build rebuilt Analytics, Auction, Catalog, Identity, Notification,
Payment and Storage, then stalled during Bidding's NuGet restore. NuGet's index
responded from both the host and a Docker container; the cause of the individual
restore stall was not established. This is a blocked build-path check, not a
passing Dockerfile rebuild or a reproduced application defect.

To continue runtime validation, Bidding and Orchestration were published from the
already-restored local projects and copied into existing disposable runtime
images. The current frontend build was copied into its existing nginx image.
Unmodified service images were reused. SHA-256 checks confirmed that packaged
`Bidding.Application.dll` and `Orchestration.Sagas.dll` matched the local publish
outputs. No repository Dockerfile or dependency policy was changed for this workaround.

These commands reproduce the workaround when the other changed-service images
have already been rebuilt and the `auction-system-validation-*` runtime images exist:

```bash
overlay_root="$(mktemp -d)"
dotnet publish src/Services/Bidding/Bidding.Api/Bidding.Api.csproj --no-restore -c Release -m:1 \
  --self-contained false -p:UseAppHost=false -o "$overlay_root/bidding-api/publish"
dotnet publish src/Orchestration/Orchestration.Api/Orchestration.Api.csproj --no-restore -c Release -m:1 \
  --self-contained false -p:UseAppHost=false -o "$overlay_root/orchestration-api/publish"
for service in bidding-api orchestration-api; do
  printf 'FROM auction-system-validation-%s:latest\nCOPY publish/ /app/\n' "$service" \
    > "$overlay_root/$service/Dockerfile"
  docker build --pull=false -t "auction-system-validation-$service:latest" "$overlay_root/$service"
done
VITE_API_URL=/api VITE_SIGNALR_URL=http://localhost:6001/hubs/notifications npm --prefix web run build
mkdir -p "$overlay_root/web-frontend"
cp -R web/dist "$overlay_root/web-frontend/dist"
printf 'FROM auction-system-validation-web-frontend:latest\nCOPY --chown=nginx:nginx dist/ /usr/share/nginx/html/\n' \
  > "$overlay_root/web-frontend/Dockerfile"
docker build --pull=false -t auction-system-validation-web-frontend:latest "$overlay_root/web-frontend"
PLAYWRIGHT_BROWSERS_PATH=/private/tmp/auction-playwright SYSTEM_TEST_SKIP_BUILD=1 \
  SYSTEM_TEST_RESULTS_DIR=/private/tmp/auction-refactor-system-runtime bash scripts/run-system-tests.sh
```

## Changed-file manifest

Changes are separated into local commits: `669cc264` contains the demonstrated
cache bug fix; `d2ee4f37`, `25795a35`, and `2932b0e0` contain the behavior-preserving
backend/frontend refactors and their tests. Tooling and this review are delivered
separately.

```text
README.md
docs/clean-code-review.md
scripts/prepare-system-compose.py
scripts/tests/test_prepare_system_compose.py
src/Orchestration/Orchestration.Api/Endpoints/SagaRecoveryEndpoints.cs
src/Orchestration/Orchestration.Infrastructure/Messaging/SagaRecoveryConsumer.cs
src/Orchestration/Orchestration.Infrastructure/Scheduling/SagaDeadlineDispatcher.cs
src/Orchestration/Orchestration.Sagas/AuctionCompletion/AuctionCompletionSagaStateMachine.cs
src/Orchestration/Orchestration.Sagas/BuyNow/BuyNowSagaStateMachine.cs
src/Orchestration/Orchestration.Sagas/SagaConstants.cs
src/Services/Analytics/Analytics.Infrastructure/Messaging/Consumers/AuctionAnalyticsConsumers.cs (split)
src/Services/Analytics/Analytics.Infrastructure/Messaging/Consumers/AuctionCreatedAnalyticsConsumer.cs
src/Services/Analytics/Analytics.Infrastructure/Messaging/Consumers/AuctionFinishedAnalyticsConsumer.cs
src/Services/Analytics/Analytics.Infrastructure/Messaging/Consumers/AuctionStartedAnalyticsConsumer.cs
src/Services/Analytics/Analytics.Infrastructure/Messaging/Consumers/BuyNowExecutedAnalyticsConsumer.cs
src/Services/Bidding/Bidding.Application/Features/Bids/PlaceBid/PlaceBidCommandHandler.cs
src/Services/Bidding/tests/Bidding.Application.Tests/PlaceBidCancellationTests.cs (renamed/expanded)
src/Services/Bidding/tests/Bidding.Application.Tests/PlaceBidCommandHandlerTests.cs
src/Services/Identity/Identity.Application/Services/TokenGenerationService.cs
src/Services/Notification/Notification.Application/Services/NotificationServiceImpl.cs
src/Services/Storage/Storage.Application/Features/Files/UploadFile/UploadFileCommandHandler.cs
src/Services/Storage/tests/Storage.Application.Tests/UploadFileFailureTests.cs
web/e2e/purchase.spec.ts
web/src/modules/bidding/constants/bid.constants.ts
web/src/modules/bidding/hooks/useAutoBids.ts
web/src/modules/bidding/hooks/useAutoBids.test.tsx
web/src/modules/payments/components/OrderProgressStepper.tsx
web/src/modules/payments/pages/OrderDetailPage.tsx
web/src/services/http.ts
web/src/services/http.test.ts
```

## Risks and optional follow-up

- Passing this suite does not establish production readiness. Stripe/OAuth/email
  integrations, peak-load behavior, external infrastructure and production
  incident recovery need separate validation. No real payment is invoked here.
- Some services still return fallback results or catch delivery errors. Changing
  those policies is a behavioral change and requires failure-injection tests;
  readability refactoring alone cannot establish their correctness.
- Large frontend pages remain candidates for further cohesive component
  extraction. The build's large-chunk warning and dependency audit findings from
  the earlier reliability review were not addressed in this refactoring pass.
- Auth-provider refresh and interceptor refresh have separate coordination, and
  notification subscriptions do not explicitly reconcile missed notifications.
  These are inspection risks, not reproduced defects in this pass.
- Job progress counting distinguishes retries and terminal state in ways that
  need explicit product semantics before any cleanup changes the counts.
- The mandatory CI workflow rejects skipped integration tests. Separate Sonar
  coverage collection does not provision those dependencies; its coverage should
  not be interpreted as proof that integration tests executed there.
- Remote CI and production deployment configuration were reviewed statically,
  not executed. Nothing was pushed or published during this pass.
