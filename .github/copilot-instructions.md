# Copilot Coding Agent Instructions for Ground Truth Curation App

## Big Picture Architecture
- The app is designed for curation and management of ground truth data for AI models.
- Major features: metadata/timestamp tracking, tagging, role-based editing, export, and validation workflows.
- Data flows: Users (Curators/Validators) interact via a UI to manage entries, apply tags, validate, and export data. Multi-source queries are supported, with unified responses and separated raw data.
- Key files: `README.md` (feature overview), `docs/GroundTruthCurationProcess.md` (process), `docs/GroundTruthERD.md` (data model).

## Developer Workflows
- **Setup:** Follow step-by-step instructions in `README.md` (Python, Node.js, Git required).
- **Linting:** Markdown and Python linting via `.github/workflows/markdown-lint.yml` and `python-lint.yml`.
- **Scripts:** Utility Python scripts live in `/scripts/` and follow conventions in `.github/instructions/python-instructions.md` and `copilot/python-script.md`.
- **Deployment:** For blueprints/CI, see `copilot/deploy.md` and always read referenced blueprint README files before running IaC commands.
- **Getting Started:** Use `copilot/getting-started.md` for onboarding and stepwise guidance.

## Project-Specific Conventions
- Use markdown checklists for user prompts and parameter collection.
- Always ask one question at a time and confirm before running commands (see `copilot/getting-started.md`).
- For Python scripts, use argument parsing, error handling, and keep scripts self-contained.
- Tagging system uses color-coded badges and supports both predefined and custom tags.
- Role-based access: Only curators can export; validators review/approve entries.

## Integration Points & Dependencies
- Data export supports JSONL and CSV formats.
- Multi-source queries (e.g., SQL, GraphQL) are unified in responses but raw data is kept separate.
- Environment variables and `.env` files are used for configuration.
- External dependencies: Python packages (see requirements files), Node.js modules (see package.json if present).

## Key Patterns & Examples
- For onboarding, always start with a short repo intro and ask for user intent.
- For deployment, always confirm environment and IaC framework (prefer Bicep if unsure).
- For scripts, always read and follow conventions in `copilot/python-script.md`.
- For markdown, keep lines <80 chars, use headings/bullets, and code blocks for samples.

## References
- `README.md` — Feature overview, setup, usage
- `docs/` — Process, ERD, flow diagrams
- `.github/instructions/` — Coding, markdown, requirements, and commit message conventions
- `copilot/` — Getting started, deployment, python script instructions

---

**If any section is unclear or missing, ask the user for clarification or examples before proceeding.**
