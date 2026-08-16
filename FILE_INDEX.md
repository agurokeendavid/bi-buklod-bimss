# BIMSS AI Starter Kit File Index

## Shared instructions
- `AGENTS.md` — source-of-truth instructions for Codex/agents
- `CLAUDE.md` — Claude Code entry point importing `AGENTS.md`
- `.github/copilot-instructions.md` — GitHub Copilot repository instructions

## Project documentation
- `docs/PROJECT_CONTEXT.md`
- `docs/ARCHITECTURE.md`
- `docs/DOMAIN_WORKFLOWS.md`
- `docs/DATA_DICTIONARY.md`
- `docs/SECURITY_AND_PRIVACY.md`
- `docs/DEVELOPMENT_ROADMAP.md`
- `docs/REPOSITORY_SETUP.md`
- `docs/AI_TOOLING.md`
- `docs/design/BIMSS-UI-SPEC.md` — frontend visual-design source of truth (tokens, typography, screens)
- `docs/design/README.md` — design handoff overview and integration status
- `docs/design/prototype/BIMSS.dc.html` — interactive HTML prototype (reference only, not production code)

## Skills
Five project skills are included in both:
- `.agents/skills/` for Codex
- `.claude/skills/` for Claude Code

GitHub Copilot can also use Agent Skills from supported repository skill locations.

## Optional Copilot custom agents
- `.github/agents/bimss-security-reviewer.agent.md`
- `.github/agents/bimss-database-reviewer.agent.md`
