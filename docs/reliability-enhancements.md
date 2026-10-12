# Auction reliability changes

## Verified defects and behavior

- SignalR invalidated keys that differed from `auctionKeys.detail(id)` and `bidKeys.forAuction(id)`. Shared factories now refresh the correct data on bid, end and extension events. Joining and rejoining fetch authoritative state, with cleanup on navigation and pending joins.
- Wallet duplicate detection ran before locking. Eight overlapping identical requests reproduced multiple debits; an index alone instead produced duplicate-key errors. A transaction-scoped reference lock and partial unique index now return one logical payment for identical retries and reject conflicting reference reuse.
- Concurrent requests waiting for token refresh lacked a retry marker and could refresh again when renewed credentials were rejected. All retried requests now carry that marker.
- Purchase snackbars crashed because `InlineAlert` did not forward its DOM ref and transition props. It now forwards both. A throttled login discarded form fields because `GuestRoute` unmounted the form; it now keeps the form mounted. Orders now fall back to backend `itemTitle` and participant usernames.
- CI omitted integration-test environment variables and the coverage collector. Disposable database/broker services, readiness checks, a collector, distinct project outputs and a result verifier now reject missing, empty, failed and skipped runs.
- Full-stack validation reproduced missing Docker build inputs, a non-executable database initialization script, nginx upstream resolution and IPv6 health-probe failures. The Dockerfiles, script, gateway alias and IPv4 probe are corrected.
- Development container endpoints and token issuer settings overrode container addresses. Compose now supplies matching HTTP/gRPC bindings and issuer settings. Identity's database interface and CORS services are registered; startup exceptions return a failing exit code.
- Identity and Analytics GET endpoints inferred a body from nested filter/sort objects, preventing endpoint startup. Flat requests now map to existing application queries. Auction and order pagination apply defaults when omitted.
- Auction detail/list JSON caching of EF aggregate graphs failed on navigation cycles. Reads now use the existing EF repository directly. Safe caching needs DTO projections and round-trip tests; measure database load before rollout.
- Bidding persisted accepted bids while Auction's projection stayed empty: the consumer listened to `BidPlacedEvent` (`Placed`) but handled only accepted statuses. It now also consumes `HighestBidUpdatedEvent`, preserves accepted legacy events, ignores duplicate/older amounts and updates a tracked aggregate.
- Auction-room hub broadcasting methods had no callers. A Notification consumer now bridges committed auction outbox bid/end/extension events to room invalidations. Refetches therefore follow the authoritative projection commit; duplicate invalidations are safe.

## Wallet payment semantics

Payments require a non-empty reference and a positive amount with at most two decimal places. A PostgreSQL transaction acquires an advisory lock for the reference before duplicate lookup. Matching username, amount, description, order reference type and completed status return the original result. Conflicting reuse returns `Payment.ReferenceConflict` without exposing another user's transaction. Existing wallet locking and optimistic concurrency protect different references competing for the same balance.

Wallet mutation, transaction insertion, domain-event outbox records and the audit outbox save share the transaction. Failure or cancellation rolls back and clears tracked mutations. The execution strategy retries the whole transaction, including duplicate lookup after an uncertain commit. `ProcessWalletPaymentCommand` currently has no endpoint or other caller; future callers must derive the username from trusted identity and validate order eligibility.

## Safe migration

`WalletPaymentIdempotency` adds a unique partial index on non-null payment references (`Type = 4`). Other ledger operations can share an order reference. The migration checks for historical duplicate payment references and aborts without deleting or rewriting financial history.

Before applying it to an existing database, stop payment writers, back up the database and inspect:

```sql
SELECT "ReferenceId", count(*)
FROM "WalletTransactions"
WHERE "Type" = 4 AND "ReferenceId" IS NOT NULL
GROUP BY "ReferenceId"
HAVING count(*) > 1;
```

Reconcile returned rows against orders, actual debits and balances using an operator-reviewed repair. Do not silently delete records to satisfy the index. Apply the migration before enabling the handler, then resume writers. Index creation can block writers, so use a maintenance window. Rolling back the migration removes the uniqueness guarantee; retain it when rolling application code back whenever possible.

## Repeatable validation

Prerequisites: .NET 9 SDK, Node/npm, Python 3, Docker with Compose and Playwright Chromium. From the repository root:

```bash
docker compose -p auction-reliability-tests -f deploy/docker/docker-compose.tests.yml up -d --wait
export BACKEND_TEST_POSTGRES='Host=localhost;Port=15432;Database=postgres;Username=auction_test;Password=auction_test'
export BACKEND_TEST_RABBIT_PORT=15673
dotnet restore auction.sln -m:1
dotnet build auction.sln --no-restore -c Release -m:1
bash scripts/run-backend-tests.sh
python3 -m unittest discover -s scripts/tests -v
docker compose -p auction-reliability-tests -f deploy/docker/docker-compose.tests.yml down -v

cd web
npm ci
npm test
npm run validate
npm run build
npx playwright install --with-deps chromium
npm run test:e2e
cd ..
bash scripts/run-system-tests.sh
```

The backend runner discovers every `*Tests.csproj`, writes TRX/OpenCover files under `coverage/<project>/` and requires every test to execute and pass. Individual `dotnet test` commands can skip integrations without environment variables; the mandatory runner fails early instead. Existing bid/finalization, saga duplicate-delivery, compensation and recovery tests run against disposable PostgreSQL/RabbitMQ.

Frontend tests exercise real hooks with controlled HTTP/SignalR boundaries: bid rejection, buy-now processing and terminal outcomes, two viewers, reconnects, navigation, pending joins, cleanup and shared authentication refresh. Fixture browser tests use the actual application with ephemeral API responses and block external network requests.

The live runner derives an isolated Compose project, binds loopback ports 3000/6001 and other original ports plus 20000, builds sequentially, waits for readiness and seeds its own users. It exercises actual HTTP, WebSocket, gRPC, database, broker and saga boundaries. Authentication throttling remains enabled. It retains logs in `system-test-artifacts/`, browser traces in `web/system-test-results/` and removes only its own containers and volumes on exit. `SYSTEM_TEST_SKIP_BUILD=1` reuses already-built images. Fake Stripe credentials are supplied; buy-now stops at an unpaid order and never invokes checkout.

## Architecture regression coverage

The additional test pass added 30 backend cases and five frontend cases without changing service implementations:

- [Service boundary guards](../src/Tests/Architecture.Tests/ServiceBoundaryTests.cs): 15 checks across all ten services and orchestration, shared building blocks, contracts and production/test dependencies. Temporarily inserting forbidden references made both the cross-service and contract guards fail; the exact project files were restored afterward.
- [Auction completion routing](../src/Services/Auction/tests/Auction.Application.Tests/AuctionCompletionRoutingTests.cs) and [legacy payment consumer](../src/Services/Payment/tests/Payment.Infrastructure.Tests/PaymentOrderConsumerTests.cs): only an eligible winner sale starts the completion saga; managed events cannot also create a legacy order. Buy-now, unsold and missing-winner cases are covered.
- [Wallet persistence](../src/Services/Payment/tests/Payment.Infrastructure.Tests/WalletPaymentIdempotencyTests.cs): different references cannot overdraw a wallet; failure or cancellation after the first save rolls back the debit and transaction, clears tracked mutations and allows the reference to retry. A separate connection verifies uncommitted changes stay invisible.
- [Saga recovery](../src/Orchestration/tests/Orchestration.Sagas.Tests/SagaRecoveryTests.cs): simultaneous purchases remain isolated and operator retries resume only the pending release, cancellation or confirmation step.
- [Recovery persistence](../src/Orchestration/tests/Orchestration.Infrastructure.Tests/SagaRecoveryPersistenceTests.cs): eight concurrent deliveries create one durable case, late messages cannot reopen a completed purchase, and acknowledgements affect only their workflow and step. Direct-consumer tests use a PostgreSQL transaction matching the production receive-outbox boundary.
- [SignalR lifecycle](../web/src/modules/auctions/hooks/useAuctionSignalR.test.tsx): initial disconnection, enable/disable, unmount during join, recovery after a rejected join, and extension events that preserve unchanged bid history.

The architecture guide was corrected to describe the existing durable orchestration host and saga-managed order creation. The commands above reproduce the complete suite.

## Local validation results (2026-10-12)

| Check | Result |
|---|---|
| Backend Release solution build | Passed; zero warnings/errors |
| Backend tests | 304 passed across 30 projects; zero failed/skipped |
| Frontend behavioral tests | 21 passed across 4 files |
| Frontend formatting, ESLint, TypeScript, translations and build | Passed |
| Chromium fixture purchase journeys | 2 passed |
| CI result-verifier tests | 2 passed |
| Live disposable-stack journeys (earlier run) | 3 passed; all 18 containers healthy |

The live tests verified public/authenticated APIs and refresh cookies; two viewers converging on bids, low-bid and duplicate-key rejection, and recovery after one viewer missed a bid during a forced WebSocket disconnect; and the real buy-now saga producing a visible unpaid order while another viewer observed auction closure. Every WebSocket frame went to/from the real server; only the transport break was controlled. The final fresh-volume run reused previously built images.

Those live journeys were not rerun for this test-only expansion. Current validation executed all 30 backend projects, including real database/broker duplicate-delivery and host-restart tests, followed by both affected projects after the final routing cases were added. The result verifier checked all final reports. Frontend behavioral tests, validation, build and both fixture browser journeys ran again. Initial local test-host socket restrictions and a missing temporary Chromium installation were resolved before the final passing runs; deliberate dependency-mutation failures are separate from those results.

An intermediate backend rerun failed after Docker killed its disposable dependencies for lack of memory (`OOMKilled`). The complete suite passed after restarting them. Sequential image builds reduce memory demand.

## Remaining limits

Passing these tests does not establish production readiness. GitHub Actions was not executed remotely. Real/sandbox Stripe webhooks, external OAuth/email, load at auction close and production recovery remain untested. Notification delivery still depends on the broker, notification service and projections; the existing hub service catches send errors, so some failures require reconnect recovery. The live suite is a local command; CI mandates database/broker tests and frontend behavioral/fixture browser tests.

The frontend build reports large chunks. The dependency audit reports 26 findings (one low, five moderate, 20 high); dependency remediation remains separate work.
