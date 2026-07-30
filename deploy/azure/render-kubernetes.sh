#!/usr/bin/env bash
set -euo pipefail

required_variables=(
  ACR_LOGIN_SERVER
  IMAGE_TAG
  KEY_VAULT_NAME
  SECRETS_IDENTITY_CLIENT_ID
  STORAGE_ACCOUNT_NAME
  STORAGE_IDENTITY_CLIENT_ID
  API_DOMAIN
  WEB_DOMAIN
  CERT_MANAGER_EMAIL
)

for variable_name in "${required_variables[@]}"; do
  if [[ -z "${!variable_name:-}" ]]; then
    echo "Missing required environment variable: ${variable_name}" >&2
    exit 1
  fi
done

if [[ ! "$ACR_LOGIN_SERVER" =~ ^[a-z0-9]+\.azurecr\.io$ ]]; then
  echo "ACR_LOGIN_SERVER must be an Azure Container Registry login server." >&2
  exit 1
fi

if [[ ! "$IMAGE_TAG" =~ ^[A-Za-z0-9._-]{1,128}$ ]]; then
  echo "IMAGE_TAG contains unsupported characters." >&2
  exit 1
fi

if [[ ! "$KEY_VAULT_NAME" =~ ^[A-Za-z0-9-]{3,24}$ ]]; then
  echo "KEY_VAULT_NAME is invalid." >&2
  exit 1
fi

if [[ ! "$SECRETS_IDENTITY_CLIENT_ID" =~ ^[0-9a-fA-F-]{36}$ ]] ||
   [[ ! "$STORAGE_IDENTITY_CLIENT_ID" =~ ^[0-9a-fA-F-]{36}$ ]]; then
  echo "Workload identity client IDs must be GUIDs." >&2
  exit 1
fi

domain_pattern='^([A-Za-z0-9-]+\.)+[A-Za-z]{2,63}$'
if [[ ! "$API_DOMAIN" =~ $domain_pattern ]] || [[ ! "$WEB_DOMAIN" =~ $domain_pattern ]]; then
  echo "API_DOMAIN and WEB_DOMAIN must be valid DNS host names." >&2
  exit 1
fi

if [[ ! "$CERT_MANAGER_EMAIL" =~ ^[^[:space:]@]+@[^[:space:]@]+\.[^[:space:]@]+$ ]]; then
  echo "CERT_MANAGER_EMAIL must be a valid email address." >&2
  exit 1
fi

kubectl kustomize deploy/kubernetes/overlays/production |
  sed \
    -e "s|REPLACE_WITH_ACR_LOGIN_SERVER|${ACR_LOGIN_SERVER}|g" \
    -e "s|:v1\\.0\\.0|:${IMAGE_TAG}|g" \
    -e "s|REPLACE_WITH_KEYVAULT_NAME|${KEY_VAULT_NAME}|g" \
    -e "s|REPLACE_WITH_MANAGED_IDENTITY_CLIENT_ID|${SECRETS_IDENTITY_CLIENT_ID}|g" \
    -e "s|REPLACE_WITH_STORAGE_IDENTITY_CLIENT_ID|${STORAGE_IDENTITY_CLIENT_ID}|g" \
    -e "s|REPLACE_WITH_STORAGE_ACCOUNT|${STORAGE_ACCOUNT_NAME}|g" \
    -e "s|api\\.auction-platform\\.com|${API_DOMAIN}|g" \
    -e "s|auction-platform\\.com|${WEB_DOMAIN}|g" \
    -e "s|REPLACE_WITH_API_DOMAIN|${API_DOMAIN}|g" \
    -e "s|REPLACE_WITH_WEB_DOMAIN|${WEB_DOMAIN}|g" \
    -e "s|REPLACE_WITH_CERT_EMAIL|${CERT_MANAGER_EMAIL}|g"
