# Site Reliability Engineering — Google

> How Google runs production systems. The foundational text on building and operating reliable, scalable services.

---

## Part I: Principles

---

## Chapter 1: Introduction to SRE

### Key Ideas

**SRE is what happens when you ask a software engineer to design an operations function.**

Traditional IT ops scales linearly with system size (more systems → more ops people). SRE applies software engineering to operations problems, enabling teams to manage massive systems with small teams.

**Core principles**:
1. **Operations is a software problem** → automate everything
2. **SRE teams have a 50% cap on operational work** → the rest is engineering (automation, tooling, architecture)
3. **Move fast by reducing the cost of failure** → error budgets, not "zero incidents"
4. **Shared ownership** → SREs and developers share responsibility for production

**SRE vs. DevOps**:

| Aspect | SRE | DevOps |
|--------|-----|--------|
| Origin | Google (2003) | Industry movement (2008) |
| Focus | Reliability engineering | Culture + collaboration |
| Key metric | Error budgets, SLOs | Lead time, deployment frequency |
| Team structure | Dedicated SRE teams | Embedded in dev teams |
| Philosophy | "Class SRE implements interface DevOps" | Break silos between dev and ops |

### Interview Questions

1. **Explain SRE in one sentence. How does it differ from traditional operations?**
2. **What is the 50% cap on ops work and why does it exist?**
3. **How do you sell the SRE model to a traditional organization?**
4. **What type of systems benefit most from an SRE approach?**
5. **Design the SRE team structure for a startup with 10 microservices.**

---

## Chapter 2: The Production Environment at Google

### Key Ideas

Google's infrastructure provides context for SRE practices. Key architectural principles:

- **Everything runs in containers** (Borg → Kubernetes)
- **Compute, storage, and network are abstracted** — applications don't know which machine they run on
- **Global load balancing** — traffic routed to the nearest healthy datacenter
- **Colossus (GFS v2)** — distributed file system; data replicated across datacenters
- **Spanner** — globally distributed, strongly consistent database with TrueTime

**The stack**:
```
User Request
  → Global Load Balancer (DNS + Anycast)
    → Frontend (reverse proxy)
      → Backend Service (Borg container)
        → Storage (Colossus / Bigtable / Spanner)
        → Messaging (Pub/Sub)
```

### Connections to Your Architecture

| Google | Your Auction Platform |
|--------|---------------------|
| Borg | Kubernetes |
| Colossus | S3 / Azure Blob Storage |
| Spanner | PostgreSQL (simpler, single-region) |
| Pub/Sub | Kafka / RabbitMQ |
| Borgmon | Prometheus + Grafana |
| Global LB | Kubernetes Ingress + CloudFlare |

---

## Chapter 3: Embracing Risk

### Key Ideas

**100% reliability is the wrong target.** It's impossibly expensive and not what users need.

The insight: Users can't tell the difference between 99.99% and 99.999% availability — their ISP, phone, and WiFi are less reliable than either.

**Error budgets**: The allowed amount of unreliability in a given period.

```
SLO: 99.9% availability per quarter
Error budget: 0.1% = 21.6 minutes of downtime per quarter

If error budget is not exhausted:
  → Push new features, take risks
  
If error budget is exhausted:
  → Freeze deployments, focus on reliability
```

**Error budget policy**:
| Budget Status | Action |
|--------------|--------|
| > 50% remaining | Ship features aggressively |
| 25-50% remaining | Ship carefully, extra testing |
| < 25% remaining | Slow down, prioritize reliability work |
| Exhausted | Feature freeze until next period |

### The Risk Equation

Cost of increasing availability increases **exponentially**:
```
99% → 99.9%     : Redundancy, load balancing        ($)
99.9% → 99.99%  : Multi-AZ, failover, testing      ($$$)
99.99% → 99.999%: Multi-region, chaos engineering   ($$$$$)
```

Every additional "9" costs roughly 10x more.

### Modern Backend Application

For the auction platform:
```
Service SLOs:
  BidService:        99.95% (critical path, revenue-impacting)
  SearchService:     99.9%  (important but not transactional)
  NotificationService: 99.5% (best effort, can be delayed)
  AnalyticsService:  99.0%  (internal, not user-facing)

Error budget (per month):
  BidService:        99.95% → 21.6 min downtime allowed
  SearchService:     99.9%  → 43.2 min downtime allowed
  NotificationService: 99.5% → 3.6 hours downtime allowed
```

### Interview Questions

1. **Why is 100% availability the wrong target?**
2. **Explain error budgets. How do they resolve the tension between developers and ops?**
3. **Set SLOs for the five services in an auction platform. Justify each number.**
4. **Your team has exhausted its error budget. The PM wants to ship a critical feature. What do you do?**
5. **How do you calculate the cost of downtime for a service?**

### Practical Exercises

1. Calculate the error budget for your BidService at 99.95% SLO. Track actual downtime over a week. Are you within budget?
2. Create a dashboard that shows error budget burn rate. Alert when burn rate predicts budget exhaustion before the end of the month.
3. Draft an error budget policy document for your team. Define the actions at each budget level.

### Common Mistakes

- Setting SLOs at 100% — impossible to achieve, demoralizes the team
- Setting the same SLO for all services — different services have different reliability requirements
- Not having a written error budget policy — the budget becomes meaningless without enforcement
- Measuring availability as uptime instead of successful request ratio

---

## Chapter 4: Service Level Objectives (SLOs)

### Key Ideas

**SLI (Service Level Indicator)**: A quantitative measure of service behavior. The metric.
**SLO (Service Level Objective)**: The target value for an SLI. The goal.
**SLA (Service Level Agreement)**: The business contract with consequences for missing the SLO.

```
SLI: "Proportion of requests served successfully" (measured)
SLO: "99.9% of requests succeed" (target)
SLA: "If < 99.5%, customer gets service credits" (contract)
```

**Choosing good SLIs**:

| Service Type | Key SLIs |
|-------------|---------|
| Request-driven (API) | Availability (success rate), Latency (p99), Throughput |
| Storage | Durability, Availability, Latency |
| Pipeline/Batch | Freshness (how stale is the data), Completeness, Throughput |
| Streaming | Freshness, Coverage, Correctness |

**SLO best practices**:
- Keep it simple: 1-3 SLIs per service
- Measure from the user's perspective (not the server's)
- Start conservative (lower SLO), tighten as you gain confidence
- Revisit SLOs quarterly

### SLO Example: Auction Platform

```
BidService SLOs:
  Availability: 99.95% of bid placements succeed (HTTP 2xx or 4xx)
  Latency: 99% of bid placements complete within 500ms
  Latency: 99.9% of bid placements complete within 2000ms

SearchService SLOs:
  Availability: 99.9% of search queries return results
  Latency: 90% of searches complete within 200ms
  Freshness: New auctions appear in search within 30 seconds

NotificationService SLOs:
  Availability: 99.5% of notifications delivered
  Latency: 95% of notifications sent within 5 seconds
  Freshness: 99% of outbid notifications sent within 10 seconds
```

### SLO-Based Alerting

```
Traditional alerting:
  "Alert if CPU > 90%"        → Noisy, not user-facing
  "Alert if error rate > 1%"  → Better, but arbitrary threshold

SLO-based alerting:
  "Alert if error budget burn rate exceeds 10x normal"
  → Means: at this rate, we'll exhaust our monthly error budget in 3 days
  → Actionable: investigate now, not after users complain
```

**Multi-window burn rate alerting**:
- Fast burn (2% budget in 1 hour): Page immediately
- Medium burn (5% budget in 6 hours): Page during business hours
- Slow burn (10% budget in 3 days): Create a ticket

### Interview Questions

1. **Define SLI, SLO, and SLA. Give an example of each for a bidding service.**
2. **How do you choose the right SLIs for a microservice? What makes a good SLI?**
3. **Design SLOs for a real-time auction notification service.**
4. **Explain burn-rate-based alerting. Why is it better than static threshold alerting?**
5. **Your service has a 99.9% SLO. Your actual availability is 99.999%. Is that a problem?**

### Practical Exercises

1. Define SLIs and SLOs for each service in your auction platform. Create a Grafana dashboard showing compliance.
2. Implement burn-rate alerting in Prometheus: Alert when the 1-hour error rate predicts SLO violation within 24 hours.
3. Conduct a quarterly SLO review: Analyze last quarter's data, discuss whether SLOs are too tight or too loose, and adjust.

### Common Mistakes

- SLIs measured on the server side (not from the user/client perspective) → miss network and LB issues
- Too many SLIs → alert fatigue, confusion about what matters
- SLO set without understanding actual performance → either trivially easy or impossible to meet
- SLO = SLA → no safety margin; you're always at risk of contractual breach
- Over-achieving SLOs → wasting money on unnecessary reliability; users develop expectations you can't always meet

---

## Part II: Practices

---

## Chapter 5: Eliminating Toil

### Key Ideas

**Toil**: Manual, repetitive, automatable, tactical, devoid of lasting value operational work.

| Characteristic | Toil | NOT Toil |
|---------------|------|----------|
| Manual | Yes (human clicks, types, runs) | Automated scripts/pipelines |
| Repetitive | Same task repeatedly | One-time projects |
| Automatable | Could be scripted | Requires human judgment |
| Tactical | Reactive (alert → fix) | Proactive (design better system) |
| No lasting value | System returns to prior state | Permanent improvement |
| Scales linearly | More traffic = more toil | More traffic = same effort |

**Examples of toil in the auction platform**:
- Manually restarting a service after memory leak
- Running database migrations by hand
- Manually scaling services during high-traffic auctions
- Rotating certificates by hand
- Manually investigating every failed bid

**The 50% rule**: SRE teams should spend no more than 50% of time on toil. The rest should be spent on engineering (automation, tooling, architecture improvements).

**How to eliminate toil**:
1. Measure it (track hours spent on operational tasks)
2. Identify the highest-toil tasks
3. Automate them (self-healing, auto-scaling, auto-remediation)
4. If you can't automate, simplify (reduce the number of manual steps)

### Interview Questions

1. **What is toil? Give three examples from a microservices platform.**
2. **How do you measure toil? What metrics do you track?**
3. **Your team spends 80% of time on toil. How do you reduce it?**
4. **Compare "toil" with valuable operational work. Where's the line?**
5. **Design an auto-remediation system that eliminates the most common toil tasks.**

### Practical Exercises

1. Audit your team's last week. Categorize every task as "toil" or "engineering." Calculate the toil percentage.
2. Identify the top 3 toil items. Write automation scripts to eliminate them (e.g., auto-restart on OOM, auto-scale on queue depth).
3. Create a "toil budget" dashboard. Track weekly toil hours per team member. Alert when approaching 50%.

---

## Chapter 6: Monitoring Distributed Systems

### Key Ideas

**Monitoring**: Collecting, processing, aggregating, and displaying real-time quantitative data about a system.

**The Four Golden Signals** (for every request-driven service):

| Signal | What It Measures | How to Measure |
|--------|-----------------|----------------|
| **Latency** | Time to serve a request | Histogram: p50, p95, p99 |
| **Traffic** | Demand on the system | Requests per second |
| **Errors** | Rate of failed requests | Error count / total requests |
| **Saturation** | How "full" the service is | CPU utilization, queue depth, memory usage |

**Why "up/down" monitoring isn't enough**:
- A service can be "up" but responding slowly
- A service can be "up" but returning errors
- Saturation can creep up until it hits a cliff (nonlinear degradation)

**Monitoring philosophy**:
- Alert on symptoms (users affected), not causes (CPU high)
- Use dashboards for causes (drill down after alert)
- Every alert should be actionable — if you can't do anything, it shouldn't page

**Black-box vs. White-box monitoring**:
| Type | What | Example | When |
|------|------|---------|------|
| Black-box | Test from user's perspective | Synthetic probes, uptime checks | Detect user-visible failures |
| White-box | Internal metrics, logs | Prometheus, application metrics | Diagnose root cause |

### Modern Backend Application: Auction Platform Monitoring

```
Four Golden Signals per Service:

BidService:
  Latency: histogram_quantile(0.99, bid_request_duration_seconds)
  Traffic: rate(bid_requests_total[5m])
  Errors:  rate(bid_requests_total{status="5xx"}[5m]) / rate(bid_requests_total[5m])
  Saturation: container_memory_usage / container_memory_limit

Alert examples:
  [PAGE] BidService error rate > 1% for 5 minutes
  [PAGE] BidService p99 latency > 2s for 5 minutes
  [TICKET] BidService memory utilization > 80% for 15 minutes
  [LOG] BidService traffic dropped > 50% in 5 minutes (possible upstream issue)
```

### Dashboard Hierarchy

```
Level 1: Executive Dashboard
  - Overall platform health (green/yellow/red)
  - SLO compliance per service
  - Error budget remaining

Level 2: Service Dashboard
  - Four golden signals per service
  - Dependency health
  - Recent deployments

Level 3: Debug Dashboard
  - Distributed traces
  - Log search
  - Database queries / slow queries
  - Container resource usage
```

### Interview Questions

1. **Explain the Four Golden Signals. Design the monitoring for BidService.**
2. **Why should you alert on symptoms, not causes?**
3. **Your auction platform is "slow." Walk me through how you diagnose it using monitoring.**
4. **Design a monitoring strategy for a Kafka-based event pipeline.**
5. **How do you monitor a distributed saga across 5 services?**

### Practical Exercises

1. Instrument your BidService with Prometheus metrics for all four golden signals. Create a Grafana dashboard.
2. Implement synthetic monitoring: A cron job that places a test bid every minute and measures success rate and latency.
3. Create a runbook for the alert "BidService error rate > 1%": Steps to diagnose, common causes, resolution procedures.

### Common Mistakes

- Monitoring only infrastructure (CPU, disk) and not application metrics (error rate, latency)
- Creating dashboards nobody looks at
- Alert fatigue from noisy, non-actionable alerts
- Not correlating monitoring data with deployments (was it a deploy that caused the spike?)

---

## Chapter 7: The Evolution of Automation in System Administration

### Key Ideas

Automation evolves through levels of sophistication:

```
Level 0: No automation
  → Human runs commands manually

Level 1: Scripts
  → Human triggers a script that performs the task

Level 2: Triggered automation
  → System detects condition, human approves action

Level 3: Autonomous
  → System detects condition and acts automatically

Level 4: Autonomous + learning
  → System adapts behavior based on past outcomes
```

**Value of automation**:
- **Consistency**: Automated processes don't make typos or forget steps
- **Speed**: Seconds vs. minutes/hours for manual processes
- **Platform**: Centralized automation becomes a platform that others can extend
- **Auditability**: Every automated action is logged

**What to automate (priority order)**:
1. Incident response (auto-remediation for known issues)
2. Deployments and rollbacks
3. Capacity management (scaling)
4. Configuration management
5. Data management (backups, migrations)

### Modern Backend Application

Automation levels in the auction platform:
```
Level 0: SSH into server, restart service manually
Level 1: kubectl rollout restart deployment/bid-service
Level 2: PagerDuty alert → human approves → auto-restart
Level 3: Kubernetes liveness probe → auto-restart
         KEDA → auto-scale based on queue depth
         ArgoCD → auto-deploy on git push
Level 4: ML-based anomaly detection → predictive scaling
         Automated canary analysis → auto-promote or rollback
```

---

## Chapter 8: Release Engineering

### Key Ideas

Release engineering is the discipline of building and delivering software reliably and repeatedly.

**Key principles**:
- **Self-service**: Developers ship their own changes through automated pipelines
- **High velocity**: Small, frequent releases are safer than large, rare ones
- **Hermetic builds**: Build output depends only on input (same commit → same artifact)
- **Enforcement of policies**: Code review, testing, approval — automated, not manual

**Release strategies**:

| Strategy | Risk | Speed | Complexity |
|----------|------|-------|-----------|
| All-at-once | Highest | Fastest | Lowest |
| Rolling | Medium | Medium | Low |
| Blue-green | Low | Medium | Medium |
| Canary | Lowest | Slowest | Highest |
| Feature flags | Lowest | Fastest | Highest (code) |

**Cherry-pick model** (Google's approach):
- Main branch is always releasable
- Release branches are created from main
- Bug fixes are cherry-picked from main to release branch
- No direct commits to release branches

### Release Pipeline: Auction Platform

```
Developer pushes to feature branch
  → PR review + approval
  → Merge to main
  → CI pipeline:
      → Build (.NET restore + build)
      → Unit tests
      → Integration tests (Docker Compose)
      → Contract tests (Pact)
      → Security scan (Trivy)
      → Build container image
      → Push to container registry (tagged with commit SHA)
  → CD pipeline:
      → Deploy to dev (auto)
      → Smoke tests
      → Deploy to staging (auto)
      → Performance tests
      → Deploy to production (canary 5%)
      → Automated canary analysis (compare error rate & latency)
      → If healthy: promote to 100%
      → If unhealthy: auto-rollback
```

### Interview Questions

1. **Design the release pipeline for a 12-service auction platform.**
2. **Compare canary deployments with feature flags. When would you use each?**
3. **How do you handle database schema migrations in a zero-downtime deployment?**
4. **What is a hermetic build and why does it matter?**
5. **Your release caused a 2% increase in errors. How do you detect and respond?**

---

## Chapter 9: Simplicity

### Key Ideas

**Software systems become complex over time.** SRE's job is to fight accidental complexity.

**Types of complexity**:
- **Essential complexity**: Inherent in the problem domain (auction rules, payment processing)
- **Accidental complexity**: Introduced by implementation choices (unnecessary abstractions, tech debt)

**Sources of accidental complexity in microservices**:
- Too many services (500 services for a 10-person team)
- Unnecessary indirection layers
- Premature abstraction
- Inconsistent patterns across services
- Dead code and unused features

**The SRE approach to simplicity**:
- Treat every line of code as a liability, not an asset
- Regularly prune dead code and unused features
- Minimal API surfaces (don't expose more than needed)
- Boring technology: prefer proven, well-understood tools over cutting-edge

### Interview Questions

1. **How do you distinguish essential complexity from accidental complexity?**
2. **Your platform has 50 microservices. 20 have < 1 request per day. What do you do?**
3. **What is "boring technology" and why should you prefer it?**
4. **How do you prevent systems from becoming more complex over time?**
5. **Design a simplicity review process for your architecture.**

---

## Part III: Practices (Continued)

---

## Chapter 10: Practical Alerting

### Key Ideas

**Alerting rules** (practical):

| Alert Type | Urgency | Channel | Example |
|-----------|---------|---------|---------|
| Page (critical) | Wake someone up | PagerDuty | Error budget burn rate critical |
| Ticket (important) | Fix this week | Jira/Linear | Memory usage trending up |
| Log (informational) | FYI | Dashboard | Elevated latency after deploy |

**Alert quality criteria**:
- **Precise**: Low false-positive rate. If it pages, there's a real problem.
- **Recall**: Catches real problems. Low false-negative rate.
- **Timely**: Fires quickly enough to prevent user impact.
- **Actionable**: Responder knows what to do. Linked to a runbook.

**Alerting on SLO burn rate** (best practice):

```
Multi-window, multi-burn-rate alerts:

Fast burn (14.4x budget consumption):
  Alert if: (error rate in last 1h * 14.4 > error budget rate)
  AND: (error rate in last 5m * 14.4 > error budget rate)
  → Page immediately

Slow burn (3x budget consumption):  
  Alert if: (error rate in last 6h * 3 > error budget rate)
  AND: (error rate in last 30m * 3 > error budget rate)
  → Create ticket

This approach:
  ✓ Pages for fast-burn incidents (lots of errors right now)
  ✓ Creates tickets for slow-burn issues (steady elevated error rate)
  ✗ Doesn't page for brief transient spikes
```

### Interview Questions

1. **Design the alerting strategy for the auction platform. What pages vs. what creates tickets?**
2. **Explain multi-window burn rate alerting. Why is it better than static thresholds?**
3. **Your on-call engineer gets 10 pages a night. How do you fix this?**
4. **What makes a good runbook? Design one for "BidService high error rate."**
5. **How do you handle alert fatigue in a team?**

### Practical Exercises

1. Implement SLO-based alerting in Prometheus for BidService. Define fast-burn and slow-burn alert rules.
2. Write a runbook for the three most common alerts in your system. Include: symptoms, diagnosis steps, resolution, escalation.
3. Audit your current alerts: For each alert that fired in the last month, categorize as true positive (actionable) or false positive. Calculate precision.

---

## Chapter 11: Being On-Call

### Key Ideas

**On-call responsibilities**:
- Available to respond to production incidents within minutes
- Diagnose and mitigate issues (not necessarily root cause fix)
- Escalate when needed
- Document incident for postmortem

**Sustainable on-call**:
- Maximum 25% of time on-call (at least 4 people in rotation)
- Maximum 2 incidents per 12-hour shift (more = system is too unreliable)
- Compensatory time off after intense on-call shifts
- Clear escalation paths and playbooks

**On-call balance**:
```
Too many pages → burnout, turnover, mistakes
Too few pages → skills atrophy, complacency
Sweet spot: 1-2 actionable incidents per shift, clear runbooks, postmortem learning
```

### Interview Questions

1. **Design an on-call rotation for a team of 6 running 10 microservices.**
2. **What is a reasonable on-call load? How do you know if it's too high?**
3. **How do you prepare a new team member for on-call?**
4. **What does a good escalation policy look like?**
5. **Your on-call engineer gets 5 pages per shift for the same issue. What do you do?**

---

## Chapter 12-13: Effective Troubleshooting & Emergency Response

### Key Ideas

**Troubleshooting methodology**:
```
1. Triage: Is the system on fire? Mitigate first, investigate later.
2. Examine: Look at monitoring data (four golden signals).
3. Diagnose: Form hypotheses. Test them systematically.
4. Test/Treat: Apply a fix. Verify it works.
5. Cure: Address root cause (not just symptoms).
```

**Emergency response priorities** (in order):
1. **Mitigate**: Stop the bleeding. Rollback, disable feature, add capacity.
2. **Communicate**: Status page update, stakeholder notification.
3. **Diagnose**: Find the root cause.
4. **Fix**: Apply permanent fix.
5. **Document**: Postmortem.

**Key principle**: Mitigation over diagnosis during incidents. A 10-minute rollback is better than a 2-hour debugging session while users are affected.

**Incident management roles**:
| Role | Responsibility |
|------|---------------|
| Incident Commander (IC) | Coordinates response, makes decisions |
| Operations Lead | Executes technical mitigation |
| Communications Lead | Updates status page, stakeholders |
| Scribe | Documents timeline and decisions |

### Modern Backend Application

Incident scenario: "Bids are failing during a high-profile auction"
```
T+0m: Alert fires (BidService error rate > 5%)
T+1m: On-call acknowledges, declares incident
T+2m: IC assigned, war room opened
T+3m: Comms lead updates status page: "Investigating bid placement issues"
T+5m: Ops lead checks: recent deploy? Yes, 15 minutes ago.
T+6m: Decision: rollback the deploy
T+8m: Rollback complete
T+10m: Error rate dropping
T+15m: Error rate at baseline. Incident mitigated.
T+20m: Comms lead: "Issue resolved. Bids are processing normally."

Next day: Postmortem → root cause: new bid validation rule rejected valid bids
Action items: Add integration test for bid validation, improve canary analysis
```

### Interview Questions

1. **Walk me through how you'd respond to "bidding is down" during a peak auction.**
2. **Why is mitigation more important than diagnosis during an incident?**
3. **What is an incident commander and why is this role essential?**
4. **Design the incident response process for a 24/7 auction platform.**
5. **How do you prevent the same incident from happening again?**

---

## Chapter 14-15: Postmortem Culture

### Key Ideas

**Postmortems** (also called retrospectives or incident reviews) are the most important tool for learning from failures.

**Blameless postmortem principles**:
- Focus on systems and processes, not individuals
- Assume everyone acted with the best intentions and information available
- Ask "What conditions allowed this to happen?" not "Who did this?"
- Actions lead to system improvements, not personnel actions

**Postmortem template**:
```
Title: BidService Outage During Celebrity Auction
Date: 2026-02-28
Duration: 23 minutes
Impact: 12,000 failed bid attempts, estimated $45K in lost revenue
Authors: [team]

Summary:
  A new bid validation rule rejected valid bids containing special characters
  in the auction title reference.

Timeline:
  14:00 - Deploy bid-service v2.4.1 (includes new validation)
  14:15 - Error rate begins climbing
  14:18 - Alert fires (error budget burn rate critical)
  14:19 - On-call acknowledges
  14:22 - Rollback initiated
  14:24 - Rollback complete
  14:27 - Error rate at baseline

Root Cause:
  Regex validation for bid references didn't account for Unicode characters
  in auction titles. The new validation rejected ~15% of valid bids.

Contributing Factors:
  - Test data didn't include Unicode characters
  - Canary analysis only checked error rate, not error types
  - No integration test for bid validation with real auction data

Action Items:
  [ ] Add Unicode test data to bid validation tests (P0, owner: Alex)
  [ ] Improve canary analysis to check error subtypes (P1, owner: Sam)
  [ ] Create integration test suite using production-like data (P1, owner: Jordan)
  [ ] Add pre-deploy validation in staging (P2, owner: Taylor)

Lessons Learned:
  - Edge cases in validation rules need production-like test data
  - Canary analysis should be more granular than just error rate
```

### Interview Questions

1. **What makes a postmortem "blameless"? Why is this important?**
2. **Design a postmortem process for your team.**
3. **How do you ensure action items from postmortems actually get completed?**
4. **When should you NOT write a postmortem?**
5. **How do you share learnings from postmortems across teams?**

### Common Mistakes

- Blaming individuals → team stops reporting incidents
- Writing postmortems but never following up on action items
- Only writing postmortems for major incidents → miss learning from smaller ones
- Postmortem becomes a formality → no real investigation or action items

---

## Chapter 16-17: Tracking Outages & Testing for Reliability

### Key Ideas

**Tracking outages systematically** reveals patterns:
- Are most outages caused by deployments? → Improve release process
- Are most outages in one service? → That service needs reliability investment
- Are outages happening at certain times? → Capacity planning issue

**Testing for reliability**:

| Test Type | What It Catches | When |
|-----------|---------------|------|
| Unit tests | Logic bugs | Every commit |
| Integration tests | Service interaction bugs | Every PR |
| Load tests | Performance limits, bottlenecks | Weekly / before major launches |
| Stress tests | Breaking point, degradation behavior | Monthly |
| Chaos tests | Failure handling, resilience gaps | Ongoing |
| Disaster recovery tests | Backup/restore, failover procedures | Quarterly |
| Game days | Full incident response practice | Semi-annually |

**Chaos engineering** principles:
1. Start with a hypothesis ("If Redis goes down, BidService uses the fallback cache")
2. Introduce the failure in a controlled way
3. Observe the actual behavior
4. Improve the system if behavior doesn't match hypothesis
5. Automate the test for continuous verification

### Interview Questions

1. **Design a chaos engineering program for the auction platform.**
2. **What's the difference between load testing, stress testing, and chaos testing?**
3. **How often should you test disaster recovery? What do you test?**
4. **Design a "game day" exercise for a major outage scenario.**
5. **How do you introduce chaos engineering to a team that's never done it?**

### Practical Exercises

1. Implement chaos testing: Use Chaos Monkey (or LitmusChaos for Kubernetes) to randomly kill BidService pods. Verify the system self-heals within 30 seconds.
2. Run a load test: Use k6 or Locust to simulate 10,000 concurrent bidders. Find the breaking point. Identify the first bottleneck.
3. Conduct a tabletop disaster recovery exercise: "The primary database is corrupted. Walk through the recovery process step by step."

---

## Chapter 18-21: Software Engineering in SRE

### Key Ideas

**SRE as software engineering**:
- SREs write code to solve operational problems
- Common SRE engineering projects:
  - Automation frameworks
  - Monitoring and alerting systems
  - Capacity planning tools
  - Deployment pipelines
  - Incident management tools

**Capacity planning**:
```
1. Forecast demand (traffic projections)
2. Determine resource requirements (CPU, memory, storage per request)
3. Plan provisioning (lead time for hardware/cloud resources)
4. Validate with load testing
5. Maintain headroom (N+2 for critical services)

Example: BidService
  Current: 1000 req/s, 10 pods, each handling 100 req/s
  Growth: 2x traffic in 6 months
  Plan: Need 20 pods by Q3, with 2 extra for burst → 22 pods
  HPA: Set max replicas to 25 for unexpected spikes
  Validate: Load test at 2500 req/s, verify latency stays within SLO
```

**N+2 redundancy**: For critical services, have enough capacity that if 2 instances fail during peak load, the remaining instances can still handle all traffic.

### Interview Questions

1. **Design the capacity planning process for an auction platform expecting 3x growth.**
2. **What is N+2 redundancy and why is it important?**
3. **How do you forecast traffic for a platform with highly variable load (auctions end at specific times)?**
4. **Design a tool that automatically recommends capacity changes based on traffic trends.**
5. **How do SREs contribute to software architecture decisions?**

---

## Chapter 24-26: Distributed Consensus & Data Integrity

### Key Ideas

**Distributed consensus** (from SRE perspective):
- Use existing consensus systems (ZooKeeper, etcd, Consul) — don't build your own
- Consensus is needed for: leader election, distributed locks, configuration management, service discovery
- Consensus is expensive — minimize the operations that need it

**Data integrity**:
- **Defense in depth**: Multiple layers of protection (backups, replication, checksums, access controls)
- **Backup testing**: Backups that aren't tested are not backups — they're hopes
- **Recovery time vs. Recovery point**: RTO (how fast can you recover?) vs. RPO (how much data can you lose?)

| Concept | Definition | Example |
|---------|-----------|---------|
| RPO (Recovery Point Objective) | Maximum acceptable data loss | "Lose at most 5 minutes of bids" → 5 min RPO |
| RTO (Recovery Time Objective) | Maximum acceptable downtime | "Service restored within 1 hour" → 1 hour RTO |

**Data integrity strategies for the auction platform**:
```
BidService (critical):
  RPO: 0 (no bid data loss) → synchronous replication
  RTO: 5 minutes → automated failover with standby replica
  Backup: Continuous WAL archiving + daily full backup
  Test: Monthly restore test to verify backups

SearchService (rebuildable):
  RPO: 30 minutes → async replication
  RTO: 30 minutes → rebuild index from event log
  Backup: Not needed (derived data, rebuilt from events)

Payment records (legal requirement):
  RPO: 0 → synchronous replication + WAL archiving
  RTO: 15 minutes → automated failover
  Backup: Daily full + continuous WAL, encrypted, stored offsite
  Retention: 7 years (legal)
  Test: Quarterly restore test with verification
```

### Interview Questions

1. **Define RPO and RTO. Set them for each service in the auction platform.**
2. **Design the backup and recovery strategy for the bidding service.**
3. **How do you test that your backups actually work?**
4. **"Your primary database is corrupted. Walk me through the recovery."**
5. **How does event sourcing simplify disaster recovery compared to traditional databases?**

---

## Summary: Key Takeaways from SRE

| Chapter | One-Line Takeaway |
|---------|-------------------|
| 1 | SRE applies software engineering to operations — automate, don't accumulate toil |
| 2 | Infrastructure should be abstracted — applications don't know which machine they run on |
| 3 | 100% is the wrong reliability target — error budgets resolve the dev vs. ops tension |
| 4 | SLI → SLO → SLA: Measure what matters to users, set targets, enforce them |
| 5 | Toil is the enemy — cap it at 50%, automate the rest |
| 6 | Monitor the four golden signals: latency, traffic, errors, saturation |
| 7 | Automate progressively — from scripts to autonomous systems |
| 8 | Small, frequent releases are safer than big, rare ones |
| 9 | Fight accidental complexity — prefer boring technology |
| 10 | Alert on SLO burn rate, not arbitrary thresholds |
| 11 | Sustainable on-call: max 25% of time, max 2 incidents per shift |
| 12-13 | Mitigate first, diagnose later — rollback beats debugging during an outage |
| 14-15 | Blameless postmortems are how organizations learn from failure |
| 16-17 | Test for reliability: load tests, chaos tests, game days |
| 18-21 | SRE is engineering — build tools for capacity planning, automation, and monitoring |
| 24-26 | Data integrity: defense in depth, test your backups, know your RPO/RTO |

### The SRE Mindset Applied to the Auction Platform

```
┌─────────────────────────────────────────────────────────────┐
│                    SRE Operating Model                        │
│                                                               │
│  Define SLOs per service                                      │
│       ↓                                                       │
│  Instrument with Four Golden Signals                          │
│       ↓                                                       │
│  Alert on SLO burn rate (not CPU/memory)                      │
│       ↓                                                       │
│  On-call responds, mitigates, documents                       │
│       ↓                                                       │
│  Blameless postmortem → action items                          │
│       ↓                                                       │
│  Automate to eliminate toil                                   │
│       ↓                                                       │
│  Error budget remaining?                                      │
│       ↓              ↓                                        │
│  Yes: Ship features   No: Reliability sprint                  │
│       ↓                      ↓                                │
│  Canary deploy → Monitor → Next cycle                         │
└─────────────────────────────────────────────────────────────┘
```
