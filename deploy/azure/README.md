# Deploy the auction platform to Azure

This directory contains the Azure deployment path for the platform:

- `main.bicep` provisions AKS, ACR, PostgreSQL Flexible Server, Azure Managed
  Redis, Key Vault, Blob Storage, identities, private PostgreSQL networking, and
  Log Analytics.
- `bootstrap-key-vault.sh` writes application secrets and service-specific
  PostgreSQL connection strings to Key Vault.
- `render-kubernetes.sh` turns the production Kustomize overlay into a
  credential-free, environment-specific manifest.
- `.github/workflows/cd.yml` builds immutable images in ACR and deploys to AKS
  through GitHub OIDC.

RabbitMQ and Elasticsearch remain in AKS for the first Azure release. Replacing
RabbitMQ with Azure Service Bus or Elasticsearch with another search engine
requires application adapters and a data migration, so those changes are not
hidden inside the infrastructure deployment.

## Prerequisites

- An Azure subscription and permission to create resources and role assignments.
- Azure CLI with Bicep support.
- `kubectl`, Helm, Docker, and a GitHub repository.
- Two DNS names, for example `api.example.com` and `example.com`.
- Stripe keys and production-strength RabbitMQ/JWT secrets.

## 1. Provision Azure

Never place the PostgreSQL password in a tracked parameter file.

```bash
export AZURE_RESOURCE_GROUP=auction-prod-rg
export AZURE_LOCATION=southeastasia
export POSTGRES_ADMIN_PASSWORD='<strong-random-password>'

az login
az group create \
  --name "$AZURE_RESOURCE_GROUP" \
  --location "$AZURE_LOCATION"

az deployment group create \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --template-file deploy/azure/main.bicep \
  --parameters deploy/azure/main.bicepparam.example \
  --parameters postgresqlAdministratorPassword="$POSTGRES_ADMIN_PASSWORD"
```

Save the deployment outputs:

```bash
az deployment group show \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name main \
  --query properties.outputs
```

The actual deployment name can be supplied with `--name auction-platform` and
then used in the preceding command.

## 2. Populate Key Vault

Build the TLS Redis connection string from the deployment outputs:

```bash
export REDIS_NAME='<managedRedisName output>'
export REDIS_HOST='<managedRedisHost output>'
export REDIS_PORT='<managedRedisPort output>'
export REDIS_KEY="$(
  az redisenterprise database list-keys \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --cluster-name "$REDIS_NAME" \
    --query primaryKey \
    --output tsv
)"
export REDIS_CONNECTION="${REDIS_HOST}:${REDIS_PORT},password=${REDIS_KEY},ssl=True,abortConnect=False"
```

Provide the remaining values and run the bootstrap:

```bash
export KEY_VAULT_NAME='<keyVaultName output>'
export POSTGRES_HOST='<postgresqlHost output>'
export POSTGRES_ADMIN_USER=auctionadmin
export RABBITMQ_USER=auction
export RABBITMQ_PASSWORD='<strong-random-password>'
export JWT_SECRET='<at-least-32-random-characters>'
export STRIPE_SECRET_KEY='<stripe-secret-key>'
export STRIPE_WEBHOOK_SECRET='<stripe-webhook-secret>'

deploy/azure/bootstrap-key-vault.sh
```

The account running this command needs permission to set Key Vault secrets.

## 3. Configure GitHub

Create these repository or production-environment variables from the Bicep
outputs and your Azure account:

```text
AZURE_CLIENT_ID=<githubActionsClientId>
AZURE_TENANT_ID=<tenant ID>
AZURE_SUBSCRIPTION_ID=<subscription ID>
AZURE_RESOURCE_GROUP=<resource group>
AKS_CLUSTER_NAME=<aksName>
ACR_LOGIN_SERVER=<acrLoginServer>
KEY_VAULT_NAME=<keyVaultName>
SECRETS_IDENTITY_CLIENT_ID=<secretsIdentityClientId>
STORAGE_ACCOUNT_NAME=<storageAccountName>
STORAGE_IDENTITY_CLIENT_ID=<storageIdentityClientId>
API_DOMAIN=api.example.com
WEB_DOMAIN=example.com
CERT_MANAGER_EMAIL=ops@example.com
```

The Bicep template creates GitHub OIDC federation for the `main` branch and the
GitHub `production` environment. No client secret or kubeconfig is required.

Protect the GitHub `production` environment with required reviewers before
enabling the manual deploy checkbox.

## 4. Deploy

1. Run the CI workflow and restore/recreate backend tests until CI is green.
2. Open **Azure CD** in GitHub Actions.
3. Select **Run workflow** on `main`.
4. Enable the `deploy` checkbox.
5. Watch migration Jobs complete before application rollouts.

The workflow installs External Secrets and cert-manager, renders the production
manifest, refreshes the one-shot migration Jobs, waits for migrations, waits for
all Deployments, and smoke-tests the API and web domains.

## 5. Configure DNS

After the ingress receives a public IP:

```bash
kubectl get ingress \
  --namespace auction-platform \
  auction-platform-ingress
```

Create DNS records for `API_DOMAIN` and `WEB_DOMAIN` pointing to that address.
Certificate issuance will remain pending until DNS resolves to the ingress.

## Local validation

```bash
dotnet restore auction.sln
dotnet build auction.sln --no-restore
dotnet test src/Services/Payment/tests/Payment.Domain.Tests/Payment.Domain.Tests.csproj --no-restore
(cd web && npm run validate && npm run build)
docker compose -f deploy/docker/docker-compose.yml config --quiet
kubectl kustomize deploy/kubernetes/overlays/production >/tmp/auction-production.yaml
az bicep build --file deploy/azure/main.bicep
```

For a placeholder-free render:

```bash
ACR_LOGIN_SERVER=example.azurecr.io \
IMAGE_TAG=local \
KEY_VAULT_NAME=example-vault \
SECRETS_IDENTITY_CLIENT_ID=11111111-1111-1111-1111-111111111111 \
STORAGE_ACCOUNT_NAME=examplestorage \
STORAGE_IDENTITY_CLIENT_ID=22222222-2222-2222-2222-222222222222 \
API_DOMAIN=api.example.com \
WEB_DOMAIN=example.com \
CERT_MANAGER_EMAIL=ops@example.com \
deploy/azure/render-kubernetes.sh >/tmp/auction-rendered.yaml
```

## Remaining production gates

- Expand the restored Payment domain test suite with unit and integration tests
  for every service. CI discovers and runs every `*Tests.csproj` under `src`.
- Triage all NuGet and npm advisories; do not waive high-severity findings
  without documenting exploitability.
- Configure explicit egress destinations for Stripe, email, OAuth, Blob, and
  other external integrations. The current default-deny policy intentionally
  blocks unspecified egress.
- Load-test bid placement, SignalR, RabbitMQ consumers, Redis, PostgreSQL
  connection pools, and migration duration.
- Plan migration from the managed NGINX application-routing add-on to Gateway
  API before the end of its Azure support window.
