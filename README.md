# Ground Truth Curation App

This application is a tool to curate contextualized ground truth entries for AI projects.
The process is generic and applicable across data structures and problem domains.

This application implements the [Ground Truth Curation Process defined in this article](./docs/GroundTruthCurationProcess.md).

## Problem Statement

Enterprise companies currently struggle with providing high quality ground truth user query/answer pairs
to use during the AI development process.

## Features

The Ground Truth Management System provides the following features:

### 🧩 Metadata & Timestamp Enhancements

- Created / Last Updated Timestamps: Displayed in both list and detail views.
- Date-Time Format: Consistent formatting improves clarity and change tracking.

### 🏷️ Tagging System

- Predefined Tags: Includes tags like `Answerable`, `Unanswerable`, and `Multiple Data Sources`.
- Custom Tags: Users can create their own tags with random color assignments.
- Tag Display: Color-coded badges shown in list and detail views.
- Tag Filtering: Dropdown filters for tag-based navigation.
- Tag Management Component: Add, edit, and remove tags via `TagManager`.

### 👥 Role-Based Editing

- Curator & Validator Roles: Editing permissions restricted to designated roles.
- Validation Workflow: Validators can now edit entries for review and approval.

### 📤 Export Functionality

- Supported Formats: `JSONL`, `CSV`.
- Role-Based Access: Only curators can export data.
- Export Modal: Allows format selection and respects current filters.
- Export Content: Includes user query, context, raw data, response, metadata, and data queries.

### 🔍 Data Validation & Multi-Source Handling

#### Validation Requirements

- Raw data for each query.
- Data-driven answers.
- Complete metadata.

#### Multi-Query Entries

- Unified formatted response synthesizes insights from multiple sources (e.g., SQL, GraphQL).
- Raw data remains separated per query.

### 🧮 Parameter Structure Overhaul

#### Parameter Fields

- `name`
- `value`
- `dataType` (e.g., string, float, datetime)

#### UI Enhancements

- Improved editing and viewing experience.
- Validation and export compatibility for mixed-type queries.

### 💬 Multi-Turn Conversations

- String user queries together to simulate multi-turn interactions with the AI system.
- Reuse existing ground truth definitions or create new definitions for use in a conversation.ß

## Installation instructions

Follow these steps to set up the Ground Truth Curation App:

1. **Check Prerequisites**

- Ensure you have Python and Node.js installed (see future documentation for specific versions).
- Install Git if you plan to contribute.

1. **Clone the Repository**

```sh
git clone https://github.com/veritaserum27/Ground-Truth-Curation-App.git
cd Ground-Truth-Curation-App
```

1. **Install Dependencies**

### Backend (C# .NET 8)

#### App Settings

1. Copy [./backend/src/GroundTruthCuration.API/appsettings.json](./backend/src/GroundTruthCuration.API/appsettings.json) as `appsettings.Development.json`.
1. For each connection string, replace each `server_name` or `account_name` placeholder with the actual names of your resources.
   For the CosmosDB connection, retrieve a valid account key and replace it the placeholder `account_key`. Replace `database_name` with your database name.
   Replace `your_username@your_domain.com` with your Entra ID email associated with the Azure resource group.
1. Save.

#### Running Locally

```sh
# Navigate to the backend directory
cd backend

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run the API (optional - for testing)
dotnet run --project src/GroundTruthCuration.Api
```

### Frontend

The frontend is a Vite + React Router v7 ("react-router" / RSC capable) application located in `frontend/`.
It uses `pnpm` (lockfile present) but you can substitute `npm` or `yarn` if desired.

#### 1. Prerequisites

- Node.js 20+ recommended (features used by Vite 6 and React 18).
- pnpm installed globally (optional): `npm i -g pnpm`.

#### 2. Install Dependencies

```sh
cd frontend
pnpm install   # or: npm install / yarn install
```

#### 3. Configuration

Environment variables (none strictly required yet) can be added via a `.env` file in `frontend/`.
Vite exposes variables prefixed with `VITE_` to the client. A starter file is provided:

```sh
cp frontend/.env.example frontend/.env
```

Common (planned) variables:

- `VITE_API_BASE_URL` Base URL for the backend API (e.g. <http://localhost:5000> or <https://localhost:5001>).
- `VITE_LOG_LEVEL` Optional log verbosity (e.g. debug|info|warn|error).

If you have not started the backend yet, follow the backend steps first so the API is reachable.

#### 4. Run in Development

From the `frontend/` directory:

```sh
pnpm dev
```

This launches the React Router dev server (default port `3000`, auto-opens browser). Adjust the port in `vite.config.ts` if needed.

Access: <http://localhost:3000>

#### 5. Building for Production

```sh
pnpm build
```

The optimized bundle is emitted to `frontend/build/` (configured via `build.outDir` in `vite.config.ts`). Serve that directory with any static file host.

#### 6. Typical Full Stack Workflow

1. Terminal A: run backend API:

```sh
dotnet run --project backend/src/GroundTruthCuration.Api
```

2. Terminal B: run frontend dev server:

```sh
cd frontend && pnpm dev
```

3. (Optional) Update `VITE_API_BASE_URL` in `frontend/.env` if the backend runs on a non-default port.

#### 7. Project Structure Highlights

- `src/routes.ts` & generated `.react-router/` directory: route definitions (React Router compiler output).
- `src/contexts/` holds in-memory mock data (`DataContext.tsx`). No network calls yet—future integration will replace mock data with API requests using the configured base URL.
- `src/components/ui/` contains reusable UI primitives (largely Radix + Tailwind patterns).

#### 8. Adding API Integration (Future)

When backend endpoints are ready, introduce a lightweight API layer (e.g. `src/lib/api.ts`) that reads `import.meta.env.VITE_API_BASE_URL`. Until then, the app operates entirely on mock data defined in `DataContext`.

#### 9. Linting / Testing (Planned)

Currently no dedicated lint/test scripts are defined. Recommended future additions:

```json
"scripts": {
  "lint": "eslint src --ext .ts,.tsx",
  "preview": "vite preview"
}
```

#### 10. Troubleshooting

- Blank page: ensure Node 20+ and reinstall dependencies.
- Environment variable not showing: confirm it is prefixed with `VITE_` and restart dev server.
- Port conflict: adjust `server.port` in `vite.config.ts`.

Refer to future documentation for advanced workflows (SSR builds, API auth, deployment) as they are implemented.

## Usage instructions

This section provides a basic guide for using the Ground Truth Curation App. More detailed instructions will be added as the project evolves.

### 1. Start the Application

- Run backend and frontend services as described in the installation instructions.
- Access the application via your browser at the provided local address (e.g., [http://localhost:3000](http://localhost:3000) or as specified by your setup).

### 2. Curate Ground Truth Data

- Log in with your assigned role (Curator or Validator).
- Add, edit, or review ground truth entries using the UI.
- Use tagging, metadata, and export features as needed.

### 3. Export Data

- Use the export functionality to download curated data in JSONL or CSV format.

## Contributing guidelines

### AI Coding Agent Instructions

This repository includes a special file: `.github/copilot-instructions.md`.

- This file guides AI coding agents (like GitHub Copilot) on how to work productively in this codebase.
- It documents architecture, workflows, conventions, and integration points specific to this project.
- AI agents automatically read this file to generate code, scripts, and documentation that match project standards.
- Contributors can update this file to clarify rules, add examples, or document new patterns as the project evolves.

**Tip:** Refer to `.github/copilot-instructions.md` for onboarding, troubleshooting, or when asking for code generation in Copilot Chat or similar tools.

### How to Contribute

For invited contributors, please:

1. Create a feature branch
2. Make your changes with tests
3. Submit a pull request

For major changes, please check our [GitHub Project](https://github.com/users/veritaserum27/projects/1)
to see planned work or create an issue to discuss what you'd like to change.

## License information

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

The MIT License is a permissive license that allows for reuse within proprietary software provided
that all copies of the licensed software include a copy of the MIT License terms and the copyright notice.
