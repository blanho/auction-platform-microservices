# Local backend code quality

Every backend project references the pinned `SonarAnalyzer.CSharp` package through
`Directory.Build.props` and `Directory.Packages.props`. Analyzer dependencies are
private build assets. Warnings fail builds across the solution, including test
projects. No Sonar account, token, Java installation, or scanner is required.

Run from the repository root:

```bash
dotnet restore auction.sln
dotnet build auction.sln --configuration Release --no-restore --no-incremental --warnaserror -m:1
```

The nonincremental build analyzes every project even when output assemblies are
current. The `sonar.yml` workflow runs this same command. There is no rule-wide
suppression or issue baseline hiding existing findings. The previous cloud-only
workflow and unused placeholder `sonar-project.properties` have been removed.

The standalone analyzer checks its enabled C# rules alongside compiler and other
Roslyn diagnostics. Server measures such as duplication, quality-gate coverage,
and advanced taint analysis require a SonarQube server. Frontend validation remains
`npm run validate` inside `web`.

## Remediation and verification

The October 2026 audit began with 215 distinct Sonar findings. Corrections include:

- Passing cancellation through affected repository, event, audit, and HTTP writes.
- Sending the constructed Search query to Elasticsearch; the prior descriptor
  callback discarded it, omitting requested text and filters.
- Removing hardcoded design-time database and Identity seed credentials.
- Removing the development gRPC certificate-validation bypass.
- Securing authentication cookies and bounding HTML-stripping regex execution.
- Preserving entity and value-object equality through explicit, overridable typed
  equality; removing duplicate exception logging while retaining cleanup/retries.
- Removing unused dependencies, ambiguous overload calls, redundant null-forgiving
  operators, and nested conditional expressions.

Regression coverage verifies Search request text/filters, cookie flags, encoded
authenticator URIs, and entity/value-object equality. The existing PostgreSQL,
RabbitMQ, localization, and workflow tests cover persistence and business behavior.

## Parameter objects and constructor dependencies

`S107` is explicitly enabled in `.editorconfig` with Sonar's default limit of
seven parameters. The parameter-count audit found and corrected 15 violations
across Auction, Bidding, Catalog, Notification, Payment, and Storage.

Use a service-owned record with `required` and `init` properties for creation data
that has many fields. For example:

```csharp
var order = Order.Create(new CreateOrderParams
{
    AuctionId = message.AuctionId,
    BuyerId = message.BuyerId,
    BuyerUsername = message.BuyerUsername,
    SellerId = message.SellerId,
    SellerUsername = message.SellerUsername,
    ItemTitle = message.ItemTitle,
    WinningBid = message.BuyNowPrice,
    BuyNowCorrelationId = message.CorrelationId
});
```

Factories retain runtime validation, defaults, and domain events. Existing basic
convenience overloads delegate to the record-based factory. Optional domain fields
remain optional, and parameter records do not change persisted entity schemas or
message contracts. Keep cancellation tokens separate from data records.

Import, export, and upload helpers use focused batch/completion objects. Catalog's
brand listing binds a request record through `[AsParameters]`, preserving its query
names, omitted-value defaults, and numeric binding errors. Named notification
creation fields also correct a former positional mix-up between auction, bid,
and order references.

Constructor dependencies are grouped through collaborators with explicit behavior:
`AuctionBidValidator` owns snapshot and remote auction eligibility checks;
`NotificationDelivery` owns outbound delivery and outcome persistence. Their
interfaces are registered with the existing scoped DI lifetimes. Tests cover bid
eligibility/fallback/cancellation, notification reference mapping, delivery outcomes,
and real ASP.NET query binding.

## Service maintenance boundaries

The backend cleanup keeps shared code inside its owning service and names the
steps that coordinate side effects:

| Service | Responsibility boundary |
|---|---|
| Analytics | Period filtering, latest auction selection, and metric calculation are distinct query stages. Select latest event IDs before further SQL filtering/counting; retain source filters before that selection. |
| Auction | Import/export mappings and cancellation notifications are shared locally. Scheduled jobs propagate cancellation to the job host. |
| Bidding | Request locks and auction transaction locks surround a named validation/persistence operation. Keep both locks until bid writing completes. |
| Catalog | Guard clauses retain validation precedence; named mutation arguments make category and brand updates explicit. |
| Identity | Administrator DTO mapping attaches roles in one place; status changes keep event and audit publication after a successful update. |
| Job | Item eligibility checks precede command dispatch; dispatch preserves correlation IDs, retry attempts, and requesting-user ownership. |
| Notification | Job-item validation and recipient failure isolation have separate methods. Channel delivery retains its idempotency checks and serialized record persistence. |
| Payment | Report request parsing precedes generation, storage, and completion reporting. Failed generation returns through the job failure path. |
| Search | Paging validation and Elasticsearch request construction are separate from response handling. Keep filters, highlights, facets, and deterministic sorting in the generated request. |
| Storage | File summaries and image/checksum details have explicit mappings. Upload rollback and committed-report detection remain in the upload workflow. |
| Gateway | Token-validation settings, hub query-token extraction, and local-environment detection have named helpers. Query tokens are accepted only under `/hubs`. |
| Orchestration | Recovery retries keep their transaction and advisory lock in the endpoint; workflow-specific retry helpers enforce the existing recovery-state checks. |

Use `scripts/run-backend-tests.sh` after a Release build to verify results from
every test project. Missing, empty, or failed test reports are errors. See
[backend testing](testing.md) for disposable providers and local skip behavior.

### Auction service

Keep handlers focused on the sequence of the use case. Extract a helper when it
names a distinct responsibility, such as updating item details, resolving catalog
references, or starting a purchase. Avoid generic pipelines that hide domain rules
or side effects. Use named arguments for long command/data mappings.

Import rules and entity creation have one implementation shared by the direct
command handler and both consumers. Map message payloads into application rows at
the consumer boundary. Preserve the message's row numbers; direct imports use the
row's one-based position. Validation returns raw diagnostics, and the response or
event boundary applies the request culture. Pass the current time explicitly to
row validation so date and manufacturing-year boundaries can be tested.

#### Workflow invariants

- A purchase retry must resolve its existing purchase before checking whether the
  auction is available for a new purchase. Correlation ownership includes both
  buyer and auction. The distributed lock must cover the lookup and all writes.
- A new purchase reserves the auction, records the purchase, publishes the saga
  start, updates the aggregate, publishes the audit, and saves. Keep cancellation
  and concurrency handling intact when changing this sequence.
- Updates collect modified fields in the order they are applied. Preserve the
  original audit snapshot and validate catalog references before publishing the
  update event, audit, or save.
- Import counts represent rows, not the number of validation messages. Keep
  insert/save/checkpoint ordering, batch sizes, receipt keys, progress identifiers,
  and completion semantics intact when changing import processing.
- Bulk activation and deactivation use the aggregate's `TryActivate` and
  `TryDeactivate` methods. Activation requires an eligible state and an end time
  strictly after the supplied clock value; reserved purchases and terminal states
  must be left alone. Preserve update counts, batch saves, and request order.
- Direct and queued exports share row mapping. Queued exports use storage's
  returned metadata in completion events and distinguish standalone job progress
  from parent-item results. Upload failures must remain retryable without a
  completed receipt; unsupported formats follow the explicit failure/completion
  path.
- Browse filters run before count, sorting, and paging. Keep tracking behavior
  explicit: read-only browse results use `AsNoTracking`; write repositories retain
  the aggregate tracking required by outbox persistence.
- Cancellation uses the aggregate's shared transition guard and status-change
  event. Preserve event payloads and public contracts during internal refactors.
- Seller-event notifications retain one message per affected auction and group
  titles by bidder in encounter order. Suspension and role-change consumers save
  before queuing notifications. Account deletion queues notifications before its
  final save, which also persists finished-auction anonymization. Keep these
  boundaries explicit when changing the shared notification collector.
- Username synchronization updates only changed seller/winner names and saves
  once after both sets have been processed. A replay with matching names performs
  no writes or saves.

## Runtime and tooling configuration

Design-time EF factories now read `ConnectionStrings__DefaultConnection`. Set it
to the intended service database before running EF tools. Supply the credential
through your environment or secret manager rather than committing it:

```bash
export ConnectionStrings__DefaultConnection="$AUCTION_DATABASE_CONNECTION_STRING"
dotnet ef database update --project src/Services/Auction/Auction.Infrastructure --startup-project src/Services/Auction/Auction.Api
```

When running Identity with `/seed`, configure `SeedData__Password` with a password
satisfying Identity's policy. There is no built-in seed password.
Authentication and CSRF cookies always use `Secure`; browser access outside a
trusted localhost context needs HTTPS. Refresh cookies remain `HttpOnly`.
Production gRPC endpoints need trusted certificates; local gRPC can use the
existing configured HTTP endpoints or a trusted development certificate.
Problem-details fallback types now use a stable `urn:auction-platform:errors`
identifier instead of the placeholder `api.example.com` host; callers may still
supply a documentation base URL.

## Reviewed analyzer exceptions

Seven member-level `SuppressMessage` attributes retain specific intentional
behavior and explain it at the declaration:

- Five `S1144` exceptions preserve setters populated by EF Core materialization,
  relationship fixup, or PostgreSQL concurrency-token refresh. This includes the
  repository test fixture's foreign key. Making those setters public or removing
  them would weaken the model or change persistence behavior.
- Two `S3330` exceptions retain JavaScript-readable double-submit CSRF cookies.
  JavaScript must copy their value into the CSRF header; these cookies contain no
  authentication credentials. Cookie tests verify both the secure flag and this
  intentional distinction from refresh cookies.

The empty EF-generated `PersistItemJson` migration is marked with `GeneratedCode`;
its existing schema history and no-op migration behavior are preserved.

Sonar documents standalone package configuration in the
[official analyzer repository](https://github.com/SonarSource/sonar-dotnet).

## Code and localization conventions

Keep handwritten backend source free of comments; document business rules and
failure-handling rationale in the architecture guide. Preserve transactions,
outbox ordering, and public contracts during refactors. Avoid abstractions whose
only purpose is reducing method or file length.

Backend user-facing messages require matching English (`en-US`) and Japanese
(`ja-JP`) resources. Persist background-job diagnostics unchanged and localize
DTOs in the reader's request culture. Use `LocalizableError.WithDiagnostic` for
nested failure reasons, keeping error codes, API fields, and user content stable.
See [architecture localization](architecture.md#localization) and the
[background jobs guide](background-jobs.md) for message keys and migration order.
