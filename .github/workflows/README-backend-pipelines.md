## Backend CI/CD Workflows Overview

This repository defines reusable GitHub Actions workflows for the .NET backend API.

### Reusable Templates

| File | Purpose |
|------|---------|
| `build-backend.yml` | Restore, build, publish and upload artifact |
| `deploy-backend-appservice.yml` | Deploy published artifact to Azure App Service (publish profile) |
| `deploy-backend-containerapp.yml` | Build & push container image then update Azure Container App |

### Orchestrating Workflows

| File | Trigger | Jobs |
|------|---------|------|
| `ci-backend.yml` | PR + push (backend paths) | Build (artifact only) |
| `cd-backend-appservice.yml` | Push to `main` | Build → Deploy to App Service |

### Deployment Target Options (Recommended First)

| Target | Use When | Pros | Considerations |
|--------|----------|------|----------------|
| Azure App Service | Standard HTTP API, quick start | Simple, no container registry needed | Less control over base image |
| Azure Container Apps | Need scale-to-zero, sidecars, Dapr, revisions | Modern, event-capable, env-based secrets | Requires container + registry + Azure CLI perms |
| Azure Functions (Isolated) | Event/trigger driven workloads | Consumption scaling | Would require project restructuring |
| AKS (Kubernetes) | Complex multi-service platform | Full control, ecosystem | Operational overhead, not needed early |

Default path implemented: **Azure App Service** (fastest adoption). Container App reusable workflow included for future evolution.

### Required Secrets

App Service:

| Secret | Description |
|--------|-------------|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Export from Azure Portal → App Service → Get publish profile |

Container App:

| Secret | Description |
|--------|-------------|
| `AZURE_CREDENTIALS` | JSON from `az ad sp create-for-rbac --sdk-auth` |
| `REGISTRY_USERNAME` | ACR username (or service principal) |
| `REGISTRY_PASSWORD` | ACR password/access key |

### Usage (App Service)
1. Create Linux App Service (.NET 8) & download publish profile
2. Add secret `AZURE_WEBAPP_PUBLISH_PROFILE` in repo → Settings → Secrets → Actions
3. (Optional) Change `WEBAPP_NAME` in `cd-backend-appservice.yml`
4. Push to `main` → build + deploy automatically

### Usage (Container App)
1. Provision ACR + Container App
2. Create service principal with `AcrPush` & `ContainerApp Contributor` role assignments
3. Add secrets (`AZURE_CREDENTIALS`, `REGISTRY_USERNAME`, `REGISTRY_PASSWORD`)
4. Create orchestrator workflow calling `deploy-backend-containerapp.yml`

### Roadmap Enhancements
* Add test stage (dotnet test) before artifact publish
* CodeQL / SAST integration
* App Service deployment slots for blue/green
* OIDC (federated credentials) to remove publish profile secret
