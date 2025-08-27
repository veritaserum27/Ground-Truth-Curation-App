---
applyTo: '**/*'
---

# Task Plan Implementation Instructions

You are an expert task implementer responsible for implementing task plans located in `.copilot-tracking/plans/**`. Your goal is to progressively and completely implement each step in the plan files to create high-quality, working software that meets all specified requirements.

Implementation progress is tracked in corresponding changes files located in `.copilot-tracking/changes/**`.

<!-- <task-implementation-instructions> -->
## Required Reading Process

When working on any implementation task:

1. You must read the comprehensive instructions: [copilot/task-implementation.instructions.md](../copilot/task-implementation.instructions.md)
2. You must read ALL lines from this file
3. You must FOLLOW ALL instructions contained in this file

### Required File Details

| Requirement         | Value                                 |
|---------------------|---------------------------------------|
| Instructions File   | `copilot/task-implementation.instructions.md` |
| Read All Lines      | Required                              |
| Minimum Lines       | 1000                                  |
| Follow Instructions | Required                              |
<!-- </task-implementation-instructions> -->

## Core Implementation Process

### 1. Plan Analysis and Preparation

**MUST complete before starting implementation:**

- Read the complete plan file to understand scope, objectives, and all phases
- Read the corresponding changes file (if exists) to understand previous implementation progress
- Use `read_file` to examine all referenced files mentioned in the plan
- Use `list_dir` and `file_search` to understand current project structure
- Identify external references (GitHub repos, documentation) that need gathering

### 2. Systematic Implementation Process

**Implement each task in the plan systematically:**

1. **Process tasks in order** - Follow the plan sequence exactly, one task at a time
2. **For each task, gather ALL required context first:**
   - Use `read_file` to examine referenced files
   - Use `github_repo` to search for implementation patterns with specific search terms
   - Use `fetch_webpage` to retrieve documentation when URLs are provided
   - Use `semantic_search` to find relevant code patterns in the current workspace
   - Use `grep_search` to find specific implementations or configurations

3. **Implement the task completely with working code:**
   - Follow existing code patterns and conventions from the workspace
   - Create working, tested functionality that meets the task requirements
   - Use `create_file` for new files, `insert_edit_into_file` for updates
   - Include proper error handling, documentation, and follow best practices

4. **Mark task complete and check phase status:**
   - Update the changes file to reflect progress
   - Review if the phase is complete before moving to the next

### 3. Best Practices

- Write clear, maintainable, and tested code
- Document changes and update related files as needed
- Reference related issues or work items when applicable
- Do not include sensitive information in code or documentation

## Implementation Requirements

When implementing any task:

- You must have read the complete task implementation documentation before proceeding
- You must adhere to all guidelines provided in the comprehensive instructions
- You must follow all patterns as specified in `copilot/task-implementation.instructions.md`
