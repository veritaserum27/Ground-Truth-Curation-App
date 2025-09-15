# Ground Truth Curation App – Architecture Overview

- Author: Laura Lund
- Last Updated: 2025-09-15
- Tool: [Mermaid Chart](https://mermaid.js.org/)

## High-Level Components

![High-Level Archiecture of Ground Truth Application](./assets/Architecture.png)

### Front End

ReactJS user interface to faciliate workflows for the Subject Matter Expert (SME) and Data Curator personas.

### Back End

C# .Net back end to implement programmatic workflows for building and retrieving ground truth definitions and interactions with datastores. See the [Back End README](../backend/README.md) for more details

The Ground Truth database stores the actual ground truth definition datasets.

### Existing System Datastores

These datastores house datasets that will be queried by the AI System being tested.
The Ground Truth Application queries them as part of the ground truth definition and curation process.
Any type of datastore can be supported as long as there is a querying language to retrieve the relevant records.

## Sequence Diagrams

```mermaid
sequenceDiagram
    participant User
    participant Front End
    participant Back End
    participant DB as Ground Truth DB
    participant DS as System Datastore

    User->>Front End: Open app / login
    Front End->>Back End: Fetch ground truth entries
    Back End->>DB: Query entries
    DB-->>Back End: Return entries
    Back End-->>Front End: Return entries
    Front End-->>User: Display entries
    User->>Front End: Select entry / edit
    Front End->>Back End: Submit update
    Back End->>DS: Query raw data
    DS-->>Back End: Return raw data
    Back End->>DB: Update entry
    DB-->>Back End: Confirm update
    Back End-->>Front End: Update success
    Front End-->>User: Show update
    User->>Front End: Add tag to entry
    Front End->>Back End: Submit tag
    Back End->>DB: Update entry / tag table
    DB-->>Back End: Confirm update
    Back End-->>Front End: Update success
    Front End-->>User: Show update
    User->>Front End: Change validation status
    Front End->>Back End: Submit validation status
    Back End->>DB: Update validation status
    DB-->>Back End: Confirm validation status
    Back End-->>Front End: Validation status success
    Front End-->>User: Show validation status
    User->>Front End: Export data
    Front End->>Back End: Request export
    Back End->>DB: Fetch export data
    DB-->>Back End: Return export data
    Back End-->>Front End: Provide export file
    Front End-->>User: Download file
```

## Component Blocks

### 1. Users

- **Curator**: Adds, edits, tags, and exports ground truth entries.
- **Validator**: Reviews, approves, or requests revisions for entries.

### 2. Ground Truth Curation App

- **Entry Management**: Handles creation and updates of ground truth definitions and entries.
- **Tagging System**: Supports predefined and custom tags with color coding and filtering.
- **Validation Workflow**: Manages review, approval, and revision requests.
- **Export Module**: Exports curated data in JSONL/CSV formats.

### 3. Data Sources

- **SQL DB**: Structured data queries.
- **GraphQL API**: Flexible queries for many data models.
- **Other Sources**: Extensible for new data providers.

## Data Flow

1. **User submits or reviews entries via UI.**

2. **App processes entries, applies tags, and manages validation.**

3. **App queries data sources for raw data and context.**

4. **App exports validated entries for downstream use.**

```mermaid
sequenceDiagram
    actor User
    participant GTUI as Front End
    participant App as Back End
    participant Data as Data Sources

    GTUI->>App: Create/update entry
    App->>Data: Query for raw/context data
    Data->>App: Return raw data
    User->>GTUI: Submit or review entry
    GTUI->>App: Send entry data
    User->>GTUI: Apply tags
    User->>GTUI: Manage validation
    User->>GTUI: Export validated entry
    GTUI->>App: Retrieve ground truth definitions
    App->>GTUI: Provide ground truths for use in experiments
```

## References

- See [docs/GroundTruthCurationProcess.md](./GroundTruthCurationProcess.md) for process details.
- See [docs/GroundTruthERD.md](./GroundTruthERD.md) for entity relationships.
