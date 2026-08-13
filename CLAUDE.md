@AGENTS.md

# Claude Code Notes

Use the repository instructions in `AGENTS.md` as the shared source of truth.

Use project skills under `.claude/skills/` when the task matches them.

Prefer Claude Code's planning behavior for:
- database schema changes,
- authentication/authorization changes,
- loan calculations or financial posting,
- election/voting changes,
- changes spanning more than one domain module.

For significant changes, inspect the affected code and tests before editing. Do not rewrite an existing module from scratch merely to match a preferred style.

When a feature is complete, use build/test verification and, for browser workflows, the repository's browser verification workflow when available.
