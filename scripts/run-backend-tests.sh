#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
: "${BACKEND_TEST_POSTGRES:?Set BACKEND_TEST_POSTGRES to a disposable PostgreSQL server}"
: "${BACKEND_TEST_RABBIT_PORT:?Set BACKEND_TEST_RABBIT_PORT to a disposable RabbitMQ port}"
configuration="${CONFIGURATION:-Release}"
results_root="${TEST_RESULTS_DIR:-$PWD/coverage}"
projects=()
while IFS= read -r project; do projects+=("$project"); done < <(find src -type f -name '*Tests.csproj' | sort)
if [ "${#projects[@]}" -eq 0 ]; then
  echo 'No backend test projects found' >&2
  exit 1
fi
status=0
for project in "${projects[@]}"; do
  name="$(basename "$project" .csproj)"
  mkdir -p "$results_root/$name"
  rm -f "$results_root/$name/$name.trx"
  dotnet test "$project" --no-restore -m:1 -c "$configuration" \
    --collect:'XPlat Code Coverage' --results-directory "$results_root/$name" \
    --logger "trx;LogFileName=$name.trx" \
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover || status=1
done
python3 scripts/verify-test-results.py "$results_root" "${projects[@]}" || status=1
exit "$status"
