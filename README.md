# BIMSS — Buklod Integrated Membership and Services System

BIMSS is the proposed centralized web system for the Bureau of Immigration Buklod ng Kawani.

## Working product name

**BIMSS — Buklod Integrated Membership and Services System**

Member-facing branding may use **MyBuklod**.

## Primary capabilities

1. Membership registration, verification, updating, and status management
2. Member self-service profile
3. Beneficiary records and beneficiary history
4. Monthly contribution history
5. Loan application, review, approval, release, repayment, and history
6. Buklod election setup, candidate management, eligible-voter management, secure voting, tallying, and result publication
7. Administrative dashboards and reports
8. Notifications and announcements
9. Audit trail and security administration
10. Initial migration from the Buklod Google Forms/Excel membership update file

## Planned technology stack

- .NET 10
- ASP.NET Core MVC
- ASP.NET Core Web API
- C#
- SQL Server / MSSQL
- Entity Framework Core 10 as the default application ORM
- Bootstrap
- jQuery
- DevExtreme jQuery
- IIS deployment
- GitHub
- GitHub Actions
- Claude Code
- GitHub Copilot
- OpenAI Codex

## Architecture direction

Use a **modular monolith**. Keep one solution and one primary SQL Server database, while separating domain modules in code.

Suggested projects:

```text
Bimss.sln
src/
  Bimss.Web/
  Bimss.Api/
  Bimss.Application/
  Bimss.Domain/
  Bimss.Infrastructure/
  Bimss.Contracts/
tests/
  Bimss.UnitTests/
  Bimss.IntegrationTests/
  Bimss.ArchitectureTests/
e2e/
  Bimss.E2E/
docs/
```

The Web and API projects are separate presentation layers but must reuse the same Application, Domain, and Infrastructure rules.

## Start here

AI coding agents and developers should read these files before implementing features:

1. `AGENTS.md`
2. `docs/PROJECT_CONTEXT.md`
3. `docs/ARCHITECTURE.md`
4. `docs/DOMAIN_WORKFLOWS.md`
5. `docs/DATA_DICTIONARY.md`
6. `docs/SECURITY_AND_PRIVACY.md`
7. `docs/DEVELOPMENT_ROADMAP.md`
8. `docs/PHASE1_BACKLOG.md` — current task status; see what's done and what's next

## Important data rule

The original membership spreadsheet is a migration source containing personal information. Do **not** commit the source workbook, exports, screenshots, real names, employee numbers, contact information, beneficiary information, or loan data to Git.

Use synthetic development data only.
