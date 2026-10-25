# Capacity and failure testing

These are staging test tools, not capacity claims. No viewer count or bids/second
target has been supplied. Start with the defaults, then test the forecast peak
and twice that peak. Install [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/).

```bash
BASE_URL=https://staging.example.com/api VUS=50 DURATION=5m \
  k6 run tests/load/auction-capacity.js
```

For hot-auction contention, seed a fresh live auction with a future end time and
at least two funded, verified bidders who are not its seller. Supply their JWTs
through the environment, not a committed file. Ensure funds cover the largest
generated bid; `START_AMOUNT` must exceed the current highest bid and
`BID_INCREMENT` must satisfy the auction rules.

```bash
# BIDDER_TOKENS is a comma-separated environment secret.
MODE=hot-auction ALLOW_BID_WRITES=true AUCTION_ID=<staging-auction-uuid> \
  BASE_URL=https://staging.example.com/api BIDS_PER_SECOND=20 DURATION=5m \
  START_AMOUNT=100 BID_INCREMENT=10 k6 run tests/load/auction-capacity.js
```

The provisional gates are p95 <500 ms and <1% unexpected responses; replace
these with agreed SLOs. The script counts 400/409 as business rejections and
requires at least one accepted bid. **Inspect rejection reasons**: an invalid
fixture or insufficient funds can make this an invalid capacity measurement.
429s are failures here; account for per-pod rate limits and use enough distinct
test users. Run cold and warm-cache passes and capture Redis hit rate, DB pool
waits, lock wait time per auction, CPU, GC, queue lag and HPA/node scale events.

During the same workload, perform these supervised staging drills:

| Drill | Evidence required |
|---|---|
| Restart one bidding pod | Retries recover; replaying a successful idempotency key creates no extra bid or reservation. |
| Restart one notification pod | Two clients connected through different pods still receive updates through Azure SignalR; reconnect and refetch state. |
| Drain one worker node | At least one healthy replica remains, PDBs allow controlled drain, replacement pods schedule. |
| Provider-supported RabbitMQ leader failover | Quorum queues retain acknowledged messages, consumers reconnect and outbox backlog drains. Include delayed-redelivery messages separately. |
| Provider-supported Elasticsearch node failure | Search recovers within the SLO and indexes remain replicated. |
| Redis interruption | Public browsing falls back to DB. Bid locks/deduplication must retain their existing failure semantics. Measure DB surge. |
| PostgreSQL failover | Connections recover; accepted bids persist and auction locks still serialize conflicting operations. |
| Auction closes during contention | Only eligible bids win; exactly one winning outcome/order, no duplicate wallet holds, no late accepted bid. |

Run the closing-auction case separately with a short-lived fixture and inspect
the final bid, order and wallet records. HTTP latency alone cannot prove these
invariants. Record recovery time, rejected/accepted bids and event delivery
delay. A zone-loss or provider failover drill requires a deployed staging
environment and provider controls; it has not been run locally.
