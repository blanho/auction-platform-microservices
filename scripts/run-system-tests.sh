#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
results="${SYSTEM_TEST_RESULTS_DIR:-$PWD/system-test-artifacts}"
mkdir -p "$results"
results="$(cd "$results" && pwd)"
config="$results/compose.json"
export STRIPE_SECRET_KEY=sk_test_validation_no_external_calls
export STRIPE_WEBHOOK_SECRET=whsec_validation_no_external_calls
unset POSTGRES_USER POSTGRES_PASSWORD RABBITMQ_USER RABBITMQ_PASS JWT_SECRET REPORT_STORAGE_API_KEY FRONTEND_URL
docker compose --env-file /dev/null -f deploy/docker/docker-compose.yml config --format json |
  python3 scripts/prepare-system-compose.py > "$config"
compose=(docker compose -p auction-system-validation -f "$config")
cleanup() {
  "${compose[@]}" ps -a > "$results/containers.txt" || true
  "${compose[@]}" logs --no-color > "$results/services.log" 2>&1 || true
  "${compose[@]}" down -v > "$results/cleanup.log" 2>&1 || true
}
trap cleanup EXIT
if [ "${SYSTEM_TEST_SKIP_BUILD:-0}" != 1 ]; then
  while IFS= read -r service; do
    "${compose[@]}" build "$service"
  done < <("${compose[@]}" config --services) > "$results/build.log" 2>&1
fi
"${compose[@]}" up -d --no-build --wait --wait-timeout 240 > "$results/startup.log" 2>&1
"${compose[@]}" run --rm --no-deps identity-api /seed > "$results/seed.log" 2>&1
(cd web && SYSTEM_TEST_URL=http://localhost:3000 npm run test:system)
