# GitHub Copilot Instructions for BIMSS

BIMSS is a .NET 10 modular-monolith system for Bureau of Immigration Buklod ng Kawani membership, beneficiaries, contributions, loans, and organizational elections.

Read `AGENTS.md` and the relevant files under `docs/` before implementing non-trivial changes.

Key rules:

- Use ASP.NET Core MVC + Web API, C#, EF Core 10, SQL Server, Bootstrap, jQuery, and DevExtreme jQuery.
- Keep business logic in Domain/Application layers, not controllers/views/JavaScript.
- Use DTOs for API contracts; do not return EF entities.
- Use policy/permission-based authorization.
- Use server-side validation for all business rules.
- Use `decimal` with explicit precision for money.
- Treat contributions and loan payments as auditable transactions.
- Beneficiaries are normalized child records with history.
- Preserve loan status history and approval audit information.
- For elections, separate voter participation from ballot contents and prevent a direct member-to-candidate mapping.
- Enforce one vote per eligible member server-side and at the database level.
- Never use real Buklod member information in tests, seeds, examples, screenshots, issues, or generated fixtures.
- Never log sensitive PII, secrets, or ballot contents.
- Never connect coding agents to production databases.
- Add tests for business rules, authorization, database constraints, and high-risk workflows.
- Avoid unnecessary infrastructure and over-engineering. This is a modular monolith, not microservices.

When generating code, prefer existing repository patterns over introducing new libraries. If a new production dependency is required, explain why before adding it.
