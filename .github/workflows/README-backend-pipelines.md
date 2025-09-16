## Backend CI/CD Workflows Overview

This repository defines reusable GitHub Actions workflows for the .NET backend API.

### Reusable Templates

| File             | Purpose                    |
| ---------------- | -------------------------- |
| `ci-backend.yml` | Restore, build backend api |

### Orchestrating Workflows

| File             | Trigger                   | Jobs                  |
| ---------------- | ------------------------- | --------------------- |
| `ci-backend.yml` | PR + push (backend paths) | Build (artifact only) |

### Deployment Target Options (Recommended First for Hack purposes)

| Target                     | Use When                                      | Pros                                     | Considerations                                  |
| -------------------------- | --------------------------------------------- | ---------------------------------------- | ----------------------------------------------- |
| Azure App Service          | Standard HTTP API, quick start                | Simple, no container registry needed     | Less control over base image                    |
| Azure Container Apps       | Need scale-to-zero, sidecars, Dapr, revisions | Modern, event-capable, env-based secrets | Requires container + registry + Azure CLI perms |
| Azure Functions (Isolated) | Event/trigger driven workloads                | Consumption scaling                      | Would require project restructuring             |
| AKS (Kubernetes)           | Complex multi-service platform                | Full control, ecosystem                  | Operational overhead, not needed early          |
