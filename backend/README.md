# Ground Truth Curation Backend Three-Project Structure

We use three projects to adhere to the [Clean Architecture approach](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture).

## 1. GroundTruthCuration.Core (Domain Layer)

- **Purpose:** Contains your business logic, entities, and interfaces
- **Dependencies:** No dependencies on external frameworks or infrastructure
- **Contents:**

  - Entities (GroundTruthEntry, Tag, User, etc.)
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

## Asynchronous Background Jobs

Long-running operations (exports, data queries, response generation) are executed asynchronously using an in-memory job queue + hosted background processor.

### Architecture Components
- `BackgroundJob` entity with lifecycle fields
- Repository abstraction `IBackgroundJobRepository` (in-memory implementation)
- Queue abstraction `IBackgroundJobQueue` (channel-based implementation)
- Service `IBackgroundJobService` for submission, status, cancellation, result access
- Hosted processor `BackgroundJobProcessor` that dequeues and executes jobs via `IBackgroundJobExecutor`

### Job Lifecycle
Queued -> Running -> (Succeeded | Failed | Canceled)

### Endpoints
| Method | Route | Description |
| ------ | ----- | ----------- |
| POST | `/api/jobs` | Submit a new job (returns 202 + Location header) |
| GET | `/api/jobs/{id}` | Retrieve job metadata/status |
| GET | `/api/jobs` | List jobs (optional `?status=Queued&status=Running`) |
| DELETE | `/api/jobs/{id}` | Cancel a queued job |
| GET | `/api/jobs/{id}/result` | Retrieve result JSON (200), 202 if pending, 404 if not found/terminal without result |

### Sample Workflow
1. Submit: `POST /api/jobs { "type": "Export" }`
2. Poll status: `GET /api/jobs/{id}` until `Succeeded`
3. Fetch result: `GET /api/jobs/{id}/result`

### Extension Strategy
- Add new job types to `BackgroundJobType`
- Implement logic in `BackgroundJobExecutor` (or future specialized executors/dispatcher)
- Replace in-memory repository with persistent store by implementing `IBackgroundJobRepository`

### Future Enhancements (Planned)
- Persistent storage (database) implementation
- Progress streaming (SignalR/WebSockets)
- Authorization / multi-tenant job scoping
- Externalized large result storage with signed URL retrieval

### Notes
Current implementation is memory-only and suitable for development/testing. All jobs are lost on process restart.
