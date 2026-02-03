# Auction Platform - Operations Runbook

## Table of Contents
1. [Service Overview](#service-overview)
2. [Common Issues & Resolutions](#common-issues--resolutions)
3. [Deployment Procedures](#deployment-procedures)
4. [Scaling Procedures](#scaling-procedures)
5. [Incident Response](#incident-response)
6. [Backup & Recovery](#backup--recovery)

---

## Service Overview

### Architecture
```
Internet → Ingress → Gateway API → Backend Services → Databases
                                 ↓
                            RabbitMQ (async)
```

### Service Dependencies
| Service | Depends On | Critical Level |
|---------|------------|----------------|
| Gateway | All services | P0 |
| Bidding | PostgreSQL, Redis, RabbitMQ, Auction | P0 |
| Auction | PostgreSQL, Redis, RabbitMQ | P0 |
| Payment | PostgreSQL, Stripe, RabbitMQ | P1 |
| Identity | PostgreSQL, Redis | P0 |
| Notification | RabbitMQ, Email/SMS providers | P2 |
| Analytics | PostgreSQL, Elasticsearch | P3 |

### Health Check Endpoints
- `/health` - Overall health
- `/health/live` - Liveness (is the service running?)
- `/health/ready` - Readiness (can it accept traffic?)
- `/health/detail` - Detailed health with dependency status

---

## Common Issues & Resolutions

### 1. High Latency on Bidding Service

**Symptoms:**
- P95 latency > 1s
- Alert: `HighLatency` firing

**Diagnosis:**
```bash
# Check pod resources
kubectl top pods -n auction-platform -l app=bidding-api

# Check database connections
kubectl exec -it deploy/bidding-api -n auction-platform -- \
  curl localhost:8080/health/detail | jq '.checks[] | select(.name=="npgsql")'

# Check Redis connection
kubectl exec -it deploy/bidding-api -n auction-platform -- \
  curl localhost:8080/health/detail | jq '.checks[] | select(.name=="redis")'
```

**Resolution:**
1. If CPU > 80%: Scale horizontally
   ```bash
   kubectl scale deployment bidding-api -n auction-platform --replicas=5
   ```
2. If DB connections exhausted: Restart pods gradually
   ```bash
   kubectl rollout restart deployment/bidding-api -n auction-platform
   ```
3. If Redis slow: Check memory usage and eviction

### 2. Database Connection Pool Exhausted

**Symptoms:**
- Alert: `DatabaseConnectionPoolExhausted`
- 500 errors with "pool exhausted" in logs

**Diagnosis:**
```bash
# Check connection count
kubectl exec -it deploy/postgres -n auction-platform -- \
  psql -U postgres -c "SELECT count(*) FROM pg_stat_activity;"

# Check waiting queries
kubectl exec -it deploy/postgres -n auction-platform -- \
  psql -U postgres -c "SELECT * FROM pg_stat_activity WHERE wait_event IS NOT NULL;"
```

**Resolution:**
1. Kill idle connections:
   ```bash
   kubectl exec -it deploy/postgres -n auction-platform -- \
     psql -U postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE state = 'idle' AND query_start < NOW() - INTERVAL '10 minutes';"
   ```
2. Increase pool size (requires restart)
3. Check for connection leaks in application logs

### 3. RabbitMQ Queue Backlog

**Symptoms:**
- Alert: `RabbitMQQueueBacklog`
- Messages not being processed

**Diagnosis:**
```bash
# Check queue status
kubectl exec -it deploy/rabbitmq -n auction-platform -- \
  rabbitmqctl list_queues name messages consumers

# Check consumer status
kubectl logs -l app=notification-api -n auction-platform --tail=100 | grep -i "consumer"
```

**Resolution:**
1. Scale consumers:
   ```bash
   kubectl scale deployment notification-api -n auction-platform --replicas=5
   ```
2. If consumers are stuck, restart them:
   ```bash
   kubectl rollout restart deployment/notification-api -n auction-platform
   ```

### 4. Pod CrashLoopBackOff

**Diagnosis:**
```bash
# Get pod events
kubectl describe pod <pod-name> -n auction-platform

# Check logs
kubectl logs <pod-name> -n auction-platform --previous

# Check if it's OOM
kubectl get events -n auction-platform --field-selector reason=OOMKilled
```

**Resolution:**
1. If OOM: Increase memory limits
2. If dependency failure: Check dependent services
3. If config error: Check ConfigMaps and Secrets

---

## Deployment Procedures

### Standard Deployment
```bash
# 1. Verify current state
kubectl get deployments -n auction-platform

# 2. Apply new version
cd deploy/kubernetes
kubectl kustomize overlays/production | kubectl apply -f -

# 3. Watch rollout
kubectl rollout status deployment -n auction-platform --timeout=300s

# 4. Verify health
curl -f https://api.auction-platform.com/health
```

### Rollback Procedure
```bash
# Rollback all deployments
kubectl rollout undo deployment -n auction-platform

# Rollback specific deployment
kubectl rollout undo deployment/bidding-api -n auction-platform

# Rollback to specific revision
kubectl rollout undo deployment/bidding-api -n auction-platform --to-revision=3
```

### Database Migration
```bash
# 1. Scale down affected service
kubectl scale deployment auction-api -n auction-platform --replicas=0

# 2. Run migration job
kubectl apply -f deploy/kubernetes/jobs/db-migration.yaml

# 3. Wait for completion
kubectl wait --for=condition=complete job/db-migration -n auction-platform --timeout=300s

# 4. Scale back up
kubectl scale deployment auction-api -n auction-platform --replicas=3
```

---

## Scaling Procedures

### Horizontal Scaling
```bash
# Manual scale
kubectl scale deployment bidding-api -n auction-platform --replicas=10

# Update HPA limits
kubectl patch hpa bidding-api-hpa -n auction-platform \
  --type='json' \
  -p='[{"op": "replace", "path": "/spec/maxReplicas", "value": 20}]'
```

### Vertical Scaling
```bash
# Update resource limits (requires pod restart)
kubectl patch deployment bidding-api -n auction-platform \
  --type='json' \
  -p='[{"op": "replace", "path": "/spec/template/spec/containers/0/resources/limits/memory", "value": "2Gi"}]'
```

---

## Incident Response

### Severity Levels
| Level | Response Time | Example |
|-------|---------------|---------|
| P0 | 15 min | Complete outage, data loss |
| P1 | 30 min | Major feature broken |
| P2 | 2 hours | Minor feature affected |
| P3 | 24 hours | Non-critical issue |

### Incident Checklist
- [ ] Acknowledge alert
- [ ] Assess impact and severity
- [ ] Page on-call if P0/P1
- [ ] Start incident channel
- [ ] Apply mitigation
- [ ] Communicate status
- [ ] Perform root cause analysis
- [ ] Document postmortem

---

## Backup & Recovery

### Database Backup
```bash
# Manual backup
kubectl exec -it deploy/postgres -n auction-platform -- \
  pg_dumpall -U postgres > backup-$(date +%Y%m%d).sql

# Verify backup
kubectl exec -it deploy/postgres -n auction-platform -- \
  pg_restore --list backup.sql | head -20
```

### Restore Procedure
```bash
# 1. Scale down all services
kubectl scale deployment --all -n auction-platform --replicas=0

# 2. Restore database
kubectl exec -it deploy/postgres -n auction-platform -- \
  psql -U postgres < backup.sql

# 3. Scale services back up
kubectl scale deployment --all -n auction-platform --replicas=2
```

---

## Contact Information

| Role | Contact | Escalation |
|------|---------|------------|
| On-call Engineer | PagerDuty | Automatic |
| Database Admin | @dba-team | P0/P1 |
| Platform Team | @platform | P0 |
| Security | @security | Security incidents |
