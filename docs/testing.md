# Backend testing

## Where tests belong

| Location | Responsibility |
|---|---|
| `src/Services/{Service}/tests/{Service}.Domain.Tests` | Domain invariants and value objects |
| `src/Services/{Service}/tests/{Service}.Application.Tests` | Handlers, validation, mapping, and application behavior |
| `src/Services/{Service}/tests/{Service}.Infrastructure.Tests` | Persistence, consumers, external adapters, and provider integration |
| `src/Services/{Service}/tests/{Service}.Api.Tests` | HTTP-facing behavior and service/API composition |
| `src/Orchestration/tests` | Saga behavior and workflow persistence/recovery |
| `src/Gateway/tests/Gateway.Api.Tests` | Gateway authentication, query-token boundaries, and CORS configuration |
| `src/Tests/Architecture.Tests` | Repository-wide structure, dependencies, and resource conventions |
| `src/Tests/BuildingBlocks.{Layer}.Tests` | Shared-library behavior for the corresponding layer |
| `src/Tests/TestSupport` | Linked test doubles and fixtures for disposable databases and brokers |
| `tests/load` | Whole-platform load scenarios |
| `scripts/tests` | Regression tests for build/test automation |

Add new test projects to `auction.sln`. Service tests stay beside their service;
shared tests stay in `src/Tests`. Database tests should not add Infrastructure
references to an Application test project.
Reference the API project for API tests instead of linking copies of endpoint or
helper source files. Link `TestSupport/TestProxy.cs` separately from the PostgreSQL
fixture when a unit test only needs a test double.

## Build and run

From the repository root:

```bash
dotnet restore auction.sln
dotnet build auction.sln -c Release --no-restore
docker compose -p auction-reliability-tests -f deploy/docker/docker-compose.tests.yml up -d --wait
export BACKEND_TEST_POSTGRES='Host=localhost;Port=15432;Database=postgres;Username=auction_test;Password=auction_test'
export BACKEND_TEST_RABBIT_PORT=15673
export BACKEND_TEST_ELASTICSEARCH='http://localhost:19200'
bash scripts/run-backend-tests.sh
docker compose -p auction-reliability-tests -f deploy/docker/docker-compose.tests.yml down -v
```

The runner uses the existing Release build, discovers test projects under `src`,
and writes TRX and OpenCover reports to a fresh `coverage/backend-*` directory.
Fresh directories prevent a previous successful run from masking missing results.
It continues through projects to collect failure reports, then returns a nonzero
exit code if any test command or result check fails.

CI requires PostgreSQL and RabbitMQ. It rejects missing/invalid reports, empty
test runs, failed runs, and unexpected skipped tests. The existing Search reindex
Elasticsearch integration test may skip only when `BACKEND_TEST_ELASTICSEARCH`
is unset. The Compose test stack supplies PostgreSQL, RabbitMQ, and Elasticsearch;
set `BACKEND_TEST_ELASTICSEARCH=http://localhost:19200` to run all provider tests.

For local work without providers, build first and explicitly allow skips:

```bash
bash scripts/run-backend-tests.sh --allow-skips
```

This still rejects failed or missing results, but permits provider tests to skip.
It does not establish PostgreSQL, RabbitMQ, or Elasticsearch coverage. Configured
providers are still used, so point test environment variables at disposable services.

For a focused iteration, run a single project directly:

```bash
dotnet test src/Services/Analytics/tests/Analytics.Application.Tests/Analytics.Application.Tests.csproj
dotnet test src/Services/Analytics/tests/Analytics.Infrastructure.Tests/Analytics.Infrastructure.Tests.csproj
```

Verify the result checker independently:

```bash
python3 -m unittest discover -s scripts/tests -v
```
