#!/usr/bin/env bash
set -euo pipefail

required_variables=(
  KEY_VAULT_NAME
  POSTGRES_HOST
  POSTGRES_ADMIN_USER
  POSTGRES_ADMIN_PASSWORD
  REDIS_CONNECTION
  RABBITMQ_USER
  RABBITMQ_PASSWORD
  JWT_SECRET
  STRIPE_SECRET_KEY
  STRIPE_WEBHOOK_SECRET
)

for variable_name in "${required_variables[@]}"; do
  if [[ -z "${!variable_name:-}" ]]; then
    echo "Missing required environment variable: ${variable_name}" >&2
    exit 1
  fi
done

if (( ${#JWT_SECRET} < 32 )); then
  echo "JWT_SECRET must contain at least 32 characters." >&2
  exit 1
fi

set_secret() {
  local secret_name="$1"
  local secret_value="$2"
  az keyvault secret set \
    --vault-name "$KEY_VAULT_NAME" \
    --name "$secret_name" \
    --value "$secret_value" \
    --output none
}

postgres_connection() {
  local database_name="$1"
  printf 'Host=%s;Port=5432;Database=%s;Username=%s;Password=%s;SSL Mode=Require;Trust Server Certificate=false' \
    "$POSTGRES_HOST" \
    "$database_name" \
    "$POSTGRES_ADMIN_USER" \
    "$POSTGRES_ADMIN_PASSWORD"
}

set_secret auction-platform-auction-db-connection "$(postgres_connection auction_db)"
set_secret auction-platform-bidding-db-connection "$(postgres_connection bid_db)"
set_secret auction-platform-payment-db-connection "$(postgres_connection payment_db)"
set_secret auction-platform-notification-db-connection "$(postgres_connection notification_db)"
set_secret auction-platform-identity-db-connection "$(postgres_connection identity_db)"
set_secret auction-platform-analytics-db-connection "$(postgres_connection analytics_db)"
set_secret auction-platform-catalog-db-connection "$(postgres_connection catalog_db)"
set_secret auction-platform-storage-db-connection "$(postgres_connection storage_db)"
set_secret auction-platform-job-db-connection "$(postgres_connection job_db)"
set_secret auction-platform-redis-connection "$REDIS_CONNECTION"
set_secret auction-platform-rabbitmq-user "$RABBITMQ_USER"
set_secret auction-platform-rabbitmq-password "$RABBITMQ_PASSWORD"
set_secret auction-platform-jwt-secret "$JWT_SECRET"
set_secret auction-platform-stripe-secret-key "$STRIPE_SECRET_KEY"
set_secret auction-platform-stripe-webhook-secret "$STRIPE_WEBHOOK_SECRET"
set_secret auction-platform-identity-signing-key "$JWT_SECRET"

echo "Azure Key Vault secrets created or updated."
