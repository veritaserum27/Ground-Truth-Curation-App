# Copilot Commit Message Instructions

## Purpose

This document provides conventions for writing clear, concise, and actionable commit messages in the Ground Truth Curation App repository.

## Guidelines

- Use the imperative mood (e.g., "Add feature" not "Added" or "Adds")
- Start with a short summary (max 50 characters)
- Separate summary from body with a blank line
- In the body, explain the "why" and "what" of the change
- Reference related issues or PRs when relevant (e.g., "Fixes #42")
- Use bullet points for lists of changes
- Keep lines under 80 characters
- Avoid generic messages ("update", "fix bug")
- For breaking changes, start with "BREAKING:" and describe impact

## Examples

```text
Add tagging system for ground truth entries

- Implements color-coded badges for tags
- Supports predefined and custom tags
- Updates UI to allow tag filtering
```

```text
Refactor validation workflow logic

- Moves validation steps to separate module
- Improves error handling and logging
```

```text
BREAKING: Change export format to JSONL

- Removes CSV export option
- Updates documentation and tests
```

## References

- See `.github/instructions/commit-message.md` for additional details
- Follow Microsoft Industry Solutions Engineering best practices
