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
