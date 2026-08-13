# AI Tooling, Skills, and MCP Recommendations

The goal is to give Claude Code, GitHub Copilot, and OpenAI Codex the same project rules while adding specialized workflows only when they are needed.

## 1. Shared project instructions

### Codex
Uses root `AGENTS.md`.

Repository skills are stored under:

```text
.agents/skills/<skill-name>/SKILL.md
```

### Claude Code
Uses root `CLAUDE.md`.

`CLAUDE.md` imports `AGENTS.md`, so shared rules are not duplicated.

Claude-specific project skills are stored under:

```text
.claude/skills/<skill-name>/SKILL.md
```

### GitHub Copilot
Uses:

```text
.github/copilot-instructions.md
.github/instructions/*.instructions.md
AGENTS.md
```

Copilot can also recognize project Agent Skills. The repository includes skills in both `.agents/skills/` and `.claude/skills/` so Codex and Claude Code have their native project locations while Copilot can use either supported location.

## 2. Recommended project skills

This starter kit includes:

### `bimss-feature`
Use for implementing a normal BIMSS feature end-to-end.

### `bimss-security-review`
Use when reviewing authorization, PII, documents, API exposure, logging, or sensitive data handling.

### `bimss-database-review`
Use for entities, EF Core mappings, migrations, SQL schema, indexes, constraints, and concurrency.

### `bimss-election-integrity`
Use for election setup, voter eligibility, voting, ballot secrecy, finalization, and results.

### `bimss-test-verify`
Use after significant changes to determine and run the appropriate test/verification set.

## 3. Claude Code built-in capabilities to use

Use Claude Code's built-in/bundled debugging, code-review, run, and verification capabilities when available in your installed version.

Project-specific procedures should remain in project skills so the team gets the same behavior.

Official docs:
- https://code.claude.com/docs/en/skills
- https://code.claude.com/docs/en/memory
- https://code.claude.com/docs/en/hooks-guide

## 4. GitHub Copilot customization to use

Recommended:
- repository-wide instructions
- path-specific instructions
- Agent Skills
- optional custom agents for specialized review
- GitHub MCP Server

Official docs:
- https://docs.github.com/copilot/customizing-copilot/adding-custom-instructions-for-github-copilot
- https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-skills

## 5. Codex customization to use

Recommended:
- `AGENTS.md`
- project Agent Skills under `.agents/skills`
- GitHub integration/MCP when needed

Official docs:
- https://developers.openai.com/codex/agent-configuration/agents-md
- https://developers.openai.com/codex/build-skills

## 6. Must-have MCP servers

### A. GitHub MCP Server — HIGH PRIORITY

Use GitHub's official MCP server for repository, issue, PR, Actions, and security context.

Recommended minimum toolsets:

```text
repos
issues
pull_requests
actions
code_security
```

Avoid enabling every tool unless you need it.

Official:
- https://github.com/github/github-mcp-server

### B. Microsoft Learn MCP Server — HIGH PRIORITY

Very useful for this stack because .NET 10, ASP.NET Core, EF Core 10, SQL Server, IIS, and Visual Studio documentation changes over time.

Endpoint:

```text
https://learn.microsoft.com/api/mcp
```

Official:
- https://learn.microsoft.com/en-us/training/support/mcp

### C. Playwright MCP — RECOMMENDED FOR LOCAL/UI TESTING

Use it against local/dev/staging BIMSS to let an AI agent inspect and exercise the real browser UI and help create reliable end-to-end tests.

Do not point automated browser agents at sensitive production records.

Official:
- https://github.com/microsoft/playwright-mcp

## 7. Database MCP guidance

A SQL/database MCP can be convenient during development, but it is **not required** to start BIMSS.

If you use one:
- connect only to a local/dev database
- use least-privilege credentials
- do not give the AI production database credentials
- do not expose real member/loan/election data
- prefer read-only credentials for exploratory tools when possible

The codebase itself, EF migrations, integration tests, and SQL Server tools remain the source of truth.

## 8. Plugins: keep the initial set small

Do not install dozens of third-party AI plugins at repository creation.

Start with:
1. GitHub MCP
2. Microsoft Learn MCP
3. Playwright MCP
4. the repository-owned skills in this starter kit

Add more only when a repeated workflow justifies them.

## 9. Optional GitHub Copilot custom agents

This starter includes two optional profiles:
- BIMSS security reviewer
- BIMSS database reviewer

Use them for focused reviews rather than as the default implementation agent.

## 10. Security note for all AI tools

Never send real:
- member profile data
- employee numbers
- addresses/contact numbers
- beneficiary information
- uploaded proofs
- contribution details
- loan records
- ballot choices

Use synthetic fixtures in AI-assisted development.
