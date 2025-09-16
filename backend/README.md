# Ground Truth Curation Backend Three-Project Structure

We use three projects to adhere to the [Clean Architecture approach](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture).

## 1. GroundTruthCuration.Core (Domain Layer)

- **Purpose:** Contains your business logic, entities, and interfaces
- **Dependencies:** No dependencies on external frameworks or infrastructure
- **Contents:**

  - Entities (GroundTruthEntry, Tag, Comment, GroundTruthDefinition,
    DataQueryDefinition)
  - Business rules and domain services
  - Interfaces for repositories and external services
  - DTOs for data transfer

## 2. GroundTruthCuration.Infrastructure (Infrastructure Layer)

- **Purpose:** Implements external concerns like database access, file systems, APIs
- **Dependencies:** References Core project, Entity Framework, database drivers
- **Contents:**

  - Database context and migrations
  - Repository implementations
  - External API clients
  - File storage services

## 3. GroundTruthCuration.Api (Presentation Layer)

- **Purpose:** HTTP API endpoints and web-specific concerns
- **Dependencies:** References both Core and Infrastructure
- **Contents:**

  - Controllers
  - Middleware
  - API models/DTOs
  - Dependency injection configuration

---

## Developer Quick Start (Hackathon Focus)

## Prerequisites

- .NET 8 SDK
- Docker Desktop (for container build/run)
- (Optional) Azure CLI if you want to manually interact with the deployed Container App.

## Restore & Build Locally

```pwsh
dotnet restore GroundTruthCuration.sln
dotnet build GroundTruthCuration.sln -c Release
```

## Run the API Locally (No Docker)

From the repo root (or `backend/` folder):

```pwsh
dotnet run --project src/GroundTruthCuration.Api/GroundTruthCuration.Api.csproj
```

Default ASP.NET binding (if not overridden) is <http://localhost:5000> or <https://localhost:7000>. Our container build exposes port 8080; you can align local dev by setting:

```pwsh
$env:ASPNETCORE_URLS = 'http://localhost:8080'
dotnet run --project src/GroundTruthCuration.Api/GroundTruthCuration.Api.csproj
```

## Docker Build & Run

From repository root (pass the backend folder as build context):

```pwsh
docker build -f backend/Dockerfile -t ground-truth-api:local backend
docker run --name ground-truth-api --rm -p 8080:8080 ground-truth-api:local
```

Already inside `backend/` directory (context is already correct):

```pwsh
docker build -t ground-truth-api:local .
docker run --name ground-truth-api --rm -p 8080:8080 ground-truth-api:local
```

Detached mode & logs:

```pwsh
docker run -d --name ground-truth-api -p 8080:8080 ground-truth-api:local
docker logs -f ground-truth-api
```

Stop & cleanup (if detached without --rm):

```pwsh
docker rm -f ground-truth-api
```

### Test an Endpoint

Adjust path based on actual controller route (example assumes `HelloController`):

```pwsh
curl http://localhost:8080/hello
```

## GitHub Workflows Summary

| Workflow                          | Purpose                                                   | Trigger                                       |
| --------------------------------- | --------------------------------------------------------- | --------------------------------------------- |
| `ci-backend.yml`                  | Restore, build, Docker build (no push, no tests)          | Pull Requests (main, develop) + manual        |
| `deploy-backend-containerapp.yml` | Reusable: build & push image + update Azure Container App | Called by future orchestrator (merge to main) |
| `markdown-lint.yml`               | Lints Markdown docs                                       | PR / push (depending config)                  |

Deprecated placeholders (scheduled for deletion): `deprecated-build-backend`, `deprecated-cd-backend-appservice`, `deprecated-deploy-backend-appservice`.

## Expected Container App Deployment Flow (Current State)

1. CI validates build + Docker image (no tests) on PR via `ci-backend.yml`.
2. On merge to `main` (or a future orchestrator workflow), call the reusable `deploy-backend-containerapp.yml` passing parameters:

   Parameters:

   - `containerapp-name`
   - `resource-group`
   - `dockerfile-path` (e.g. `backend/Dockerfile`)
   - `image-name` (e.g. `myregistry.azurecr.io/ground-truth-api:sha-<commit-sha>`)

3. Workflow logs in to Azure & ACR, builds, pushes, and updates the Container App with the new image.

## Image Tagging Guidance

Use immutable commit-based tags: `myregistry.azurecr.io/ground-truth-api:sha-<short-sha>`.
Optional moving tag (e.g. `:main`) for quick rollback reference.

## Rollback (Manual)

```pwsh
az acr repository show-tags -n <ACR_NAME> --repository ground-truth-api --top 10 --orderby time_desc
az containerapp update --name <APP_NAME> --resource-group <RG> --image myregistry.azurecr.io/ground-truth-api:sha-<previous-sha>
```

## Re-introducing Tests (Optional Post-Hackathon)

1. Create a test project:

   ```pwsh
   dotnet new xunit -n GroundTruthCuration.Core.Tests -o backend/tests/GroundTruthCuration.Core.Tests
   dotnet sln backend/GroundTruthCuration.sln add backend/tests/GroundTruthCuration.Core.Tests/GroundTruthCuration.Core.Tests.csproj
   ```

2. Add a sample test:

   ```csharp
   using Xunit;
   namespace GroundTruthCuration.Core.Tests;
   public class SmokeTests
   {
       [Fact]
       public void AlwaysTrue() => Assert.True(true);
   }
   ```

3. Update `ci-backend.yml` to re-enable a `dotnet test` step before the Docker build.

Run tests locally:

```pwsh
dotnet test GroundTruthCuration.sln --no-build --verbosity minimal
```

## Health Endpoints

Implemented minimal endpoints for fast container readiness checks:

- `/` (root) returns a simple identifier string.
- `/healthz` plain health probe.
- `/api/healthz` namespaced variant for future versioning.

Quick manual checks:

```pwsh
curl http://localhost:8080/
curl http://localhost:8080/healthz
```

When adding a Dockerfile health check later you can use:

```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD curl -f http://localhost:8080/healthz || exit 1
```

## Local CI Emulation Script

Use `scripts/local-ci-backend.ps1` to mimic the GitHub Actions build (restore,
build, Docker image build with commit SHA tagging logic).

```pwsh
pwsh ./scripts/local-ci-backend.ps1
```

## Common Issues

| Issue                         | Symptom              | Fix                                                                     |
| ----------------------------- | -------------------- | ----------------------------------------------------------------------- |
| Docker build fails on restore | NuGet timeout / rate | Re-run; ensure network; consider local `~/.nuget` cache                 |
| API not responding on 8080    | Connection refused   | Confirm `ASPNETCORE_URLS` inside container, port mapping `-p 8080:8080` |
| Tests skipped                 | No test project      | Confirm test csproj added to solution & CI updated                      |
| Rollback needed               | Bad deploy           | Use previous commit image tag via `az containerapp update`              |

## TODOS (Post-Hackathon?)

- Add Docker `HEALTHCHECK` instruction (endpoints already exist).
- Add integration tests using `WebApplicationFactory`.
- Move to OIDC-based Azure login (remove stored SP secret).
- Key Vault for secrets and managed identity for downstream services.
- Add coverage reporting & quality gates.

---

For any questions during the hack, drop them in the team chat with a link to the relevant file path.
