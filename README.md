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

TBD - Frontend setup instructions will be added when frontend is implemented.

1. **Configuration**

- Review [infra setup](./infra/README.md).
- Review any `.env.example` or configuration files and create your own `.env` if required.
- Set up environment variables for database, API keys, or other integrations.

1. **Run the Application**

- Start backend and frontend services as described in future usage instructions.

Refer to future documentation for advanced workflows, API usage, and integration details.

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
