# System Design & Architecture Study Notes

> Deep-dive study notes structured for senior-level system design understanding.
> Each chapter follows: Key Ideas → Trade-offs → Modern Applications → Examples → Interview Prep → Mistakes → Connections.

## Books Covered

| # | Book | Author | Focus |
|---|------|--------|-------|
| 1 | [Designing Data-Intensive Applications](./01-designing-data-intensive-applications.md) | Martin Kleppmann | Data systems, replication, partitioning, transactions, consensus |
| 2 | [Building Microservices](./02-building-microservices.md) | Sam Newman | Service decomposition, integration, deployment, testing |
| 3 | [Microservices Patterns](./03-microservices-patterns.md) | Chris Richardson | Sagas, CQRS, event sourcing, API gateways, observability |
| 4 | [Designing Distributed Systems](./04-designing-distributed-systems.md) | Brendan Burns | Sidecars, ambassadors, scatter/gather, FaaS, batch patterns |
| 5 | [Site Reliability Engineering](./05-site-reliability-engineering.md) | Google | SLOs, error budgets, toil, monitoring, incident response |

## How to Use These Notes

- **Studying a topic**: Jump to the relevant book and chapter
- **Interview prep**: Each chapter has 5 advanced system design questions
- **Hands-on practice**: Each chapter has 3 practical exercises
- **Avoiding pitfalls**: Each chapter lists common mistakes and anti-patterns
- **Connecting concepts**: Cross-references link related ideas across all 5 books

## Concept Cross-Reference Matrix

| Concept | DDIA | Building MS | MS Patterns | Distributed Sys | SRE |
|---------|------|-------------|-------------|----------------|-----|
| Consistency | Ch 5,7,9 | Ch 4 | Ch 4,6 | — | Ch 7 |
| Availability | Ch 5,8 | Ch 11 | Ch 3 | Ch 3,4 | Ch 3,4 |
| Partitioning/Sharding | Ch 6 | Ch 4 | Ch 6 | Ch 5 | — |
| Replication | Ch 5 | — | Ch 6 | Ch 3,4 | — |
| Transactions | Ch 7 | Ch 4 | Ch 4 | — | — |
| Event-Driven | Ch 11 | Ch 4 | Ch 3,4,6 | Ch 7 | — |
| Caching | Ch 3 | — | Ch 7 | Ch 2 | — |
| Service Discovery | — | Ch 4 | Ch 3 | Ch 2 | — |
| Monitoring | — | Ch 8 | Ch 11 | — | Ch 6,10 |
| Deployment | — | Ch 6 | Ch 12 | Ch 8 | Ch 8 |
| Testing | — | Ch 7 | Ch 9 | — | Ch 17 |
| Security | — | Ch 9 | — | — | — |
