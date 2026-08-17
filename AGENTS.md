# BIMSS Agent Instructions

These instructions apply to AI coding agents and developers working in this repository.

## Project mission

Build and maintain BIMSS, the Buklod Integrated Membership and Services System for the Bureau of Immigration Buklod ng Kawani.

The system manages sensitive membership, beneficiary, contribution, loan, and election information. Correct authorization, auditability, financial integrity, and ballot secrecy are mandatory design requirements.

## Read before changing code

Before implementing a feature, read the relevant project documentation:

- `docs/PROJECT_CONTEXT.md`
- `docs/ARCHITECTURE.md`
- `docs/DOMAIN_WORKFLOWS.md`
- `docs/DATA_DICTIONARY.md`
- `docs/SECURITY_AND_PRIVACY.md`
- `docs/PHASE1_BACKLOG.md` — Phase 1 task status (complete as of 2026-08-17).
- `docs/PHASE2_BACKLOG.md` — current Phase 2 task status; check this first to see
  what's already done and what's next before starting new work.

If the documentation and implementation disagree, do not silently guess. Preserve existing behavior unless the task explicitly changes it, and update documentation when a design decision changes. When a Phase 1 task is completed, update its status in `docs/PHASE1_BACKLOG.md` in the same PR.

## Architecture rules

- Use a modular monolith.
- Keep domain logic out of API controllers, frontend components/pages, and EF Core configurations.
- Controllers coordinate requests; Application services/use cases execute workflows; Domain objects enforce business invariants; Infrastructure handles persistence and external services.
- Avoid introducing microservices, message brokers, distributed caches, CQRS frameworks, or other infrastructure unless a concrete requirement justifies them.
- Prefer simple dependency injection and explicit interfaces.
- Do not create a generic repository abstraction over EF Core unless there is a demonstrated need.
- Use asynchronous database and I/O APIs where appropriate.
- Never use `async void` except event handlers.
- Pass `CancellationToken` through application and infrastructure operations where practical.

## Data access rules

- Use EF Core 10 with the SQL Server provider as the default persistence approach.
- Use a Code First approach: entities and `IEntityTypeConfiguration<T>` classes in C# are the
  source of truth for the schema. Never scaffold/reverse-engineer entities from an existing
  database (`Scaffold-DbContext`/`dotnet ef dbcontext scaffold`), and never hand-edit the
  database schema outside of migrations.
- Use migrations for schema evolution. Every schema change is captured as a migration generated
  from the model (`dotnet ef migrations add`) and committed alongside the entity/configuration
  change that caused it.
- Store monetary amounts with SQL `decimal`, never `float` or `real`.
- Use explicit precision for money values.
- Use database constraints and unique indexes for invariants that must survive concurrent requests.
- Use optimistic concurrency where simultaneous updates are plausible.
- Avoid hard deletes for auditable membership, beneficiary, contribution, loan, and election records.
- Do not expose EF entities directly from API endpoints.
- Do not add N+1 database query patterns.
- Use projections for lists, grids, dashboards, and reports.
- Never execute user-provided raw SQL.
- Dapper/raw SQL may be used only for justified reporting/performance cases and must use parameters.

## Membership rules

- A member has one master identity.
- BI Employee Number should be treated as a business identifier and validated for uniqueness when Buklod confirms the policy.
- Important profile changes should support review/approval rather than silently replacing the official record.
- Maintain status/history for material member changes.
- Beneficiaries are child records, not fixed columns on the member table.
- Beneficiary changes must preserve history.

## Contribution rules

- Contributions are transactions/ledger records.
- Never model January–December as columns.
- Never overwrite historical contribution records to represent corrections. Use an adjustment/reversal approach or a traceable correction workflow.
- Every imported contribution batch must have import metadata and error reporting.
- Reports must be reproducible from persisted transactions.

## Loan rules

- Loan application, approved loan, payment schedule, payment, and status history are separate concepts.
- Status transitions must be validated server-side.
- Every approval/rejection/release action must record actor, timestamp, status, and remarks where applicable.
- Do not compute authoritative loan balances only in browser JavaScript.
- Financial totals must be computed server-side from trusted records.
- Never delete a released loan or posted payment as a normal correction method.

## Election rules

- Enforce voter eligibility on the server.
- Enforce one ballot per eligible voter per election with database-level protection against race conditions.
- Separate participation/eligibility records from secret ballot contents.
- Do not store a direct `MemberId -> CandidateId` relationship for secret elections.
- Do not expose live candidate totals while voting is open unless Buklod explicitly adopts that policy.
- Closing/finalizing an election is an auditable action.
- Published election results must come from finalized persisted results.
- Election integrity code requires extra tests and review.

## Security rules

- Authorization is server-side and policy/permission based.
- Never trust hidden inputs, disabled inputs, client-side validation, or client-side filters as authorization.
- The frontend authenticates via short-lived JWT access tokens (`Authorization: Bearer`), not cookies/anti-forgery tokens — see `docs/SECURITY_AND_PRIVACY.md`'s "Authentication and token handling" section for the storage/refresh/CSRF-tradeoff details.
- Validate uploaded files by extension, content type, size, storage policy, and authorization.
- Never place secrets or production credentials in source control.
- Never log passwords, access tokens, full government/employee identifiers, full addresses, beneficiary details, ballot contents, or sensitive loan data.
- Mask sensitive data in diagnostic logs.
- Use synthetic test data. Do not copy real Buklod member data into tests, issues, AI prompts, or seed files.
- Do not connect AI tools to the production database.

## API rules

- Use DTOs/contracts for API requests and responses.
- Validate every command on the server.
- Return consistent problem details for errors.
- Version external/integration APIs when compatibility matters.
- Keep member-only endpoints scoped to the authenticated member.
- Administrative endpoints must require explicit permissions.

## Validation rules

- At the DTO/API boundary (`Bimss.Contracts` request types), use
  `System.ComponentModel.DataAnnotations` attributes (`[Required]`,
  `[StringLength]`, `[Range]`, `[EmailAddress]`, etc.). `[ApiController]`
  actions get automatic 400 `ValidationProblemDetails` responses for invalid
  models — no manual `ModelState.IsValid` checks needed in API controllers.
  No FluentValidation yet; revisit only if rule complexity grows enough to
  justify it.
- DataAnnotations catch shape/format errors early, but never replace
  server-side business-rule enforcement — a request can pass DataAnnotations
  and still violate a domain invariant, so Domain/Application code must
  re-validate regardless of what the DTO layer already checked.
- In Domain/Application code, use the built-in guard-clause helpers
  (`ArgumentException.ThrowIfNullOrWhiteSpace`, `ArgumentNullException.ThrowIfNull`,
  `ArgumentOutOfRangeException.ThrowIfNegative`, etc.) for simple single-value
  argument checks — see `Bimss.Application.Auditing.AuditEntry`'s constructor
  for a working example. Don't build a custom `Guard` abstraction; the BCL
  already covers this.
- For genuine business-rule violations — not just a bad argument — throw the
  typed exceptions from `Bimss.Domain.Exceptions` (`NotFoundException`,
  `ConflictException`, `ForbiddenException`, `DomainValidationException`),
  never a bare `Exception`/`InvalidOperationException`. Use
  `DomainValidationException` (with its `Errors` dictionary) when multiple
  field-level violations need to be reported together; use `ConflictException`/
  `ForbiddenException`/`NotFoundException` for their specific situations
  rather than `DomainValidationException` as a catch-all. These map to the
  correct HTTP status codes automatically via the global exception handling
  from BIMSS-008 — don't hand-roll status-code mapping at the controller
  level.

## Frontend rules

- The frontend is a separate Next.js + React (TypeScript) app under `frontend/`, consuming `Bimss.Api` over REST with JWT bearer auth. It is not part of the .NET solution and does not share a process with the backend.
- Use Tailwind CSS for layout/styling and shadcn/ui (this project's install uses Base UI, `@base-ui/react`, not Radix) for components — this replaced Bootstrap/jQuery/DevExtreme (see `docs/PHASE1_BACKLOG.md` for why).
- `docs/design/BIMSS-UI-SPEC.md` is the frontend visual-design source of truth (tokens, typography, spacing, screen layout); `docs/design/README.md` tracks what's actually integrated versus deferred to a later phase. Read it before restyling or adding a screen.
- Use a data-table library (e.g. TanStack Table) for data-heavy grids, filtering, and administrative screens where it adds value; do not reach for a heavy grid component merely to replace simple semantic HTML.
- Keep client-side business logic minimal; authoritative decisions belong on the server. Do not compute authoritative totals, balances, or eligibility client-side.
- Prefer server components/data-fetching patterns where the framework supports them; keep client components focused on interactivity.
- Keep accessibility in mind: labels, keyboard operation, focus behavior, validation messages, and adequate contrast.
- **Server-side authorization must determine which actions succeed even if buttons/menu items are hidden client-side** — this principle is unchanged by the stack switch.
- The access token lives in memory only (never `localStorage`/`sessionStorage`); the refresh token is an httpOnly cookie the frontend never reads directly. See `docs/SECURITY_AND_PRIVACY.md`.
- Add or update Playwright coverage for critical UI workflows, run it when available before declaring UI work complete.

## Testing requirements

For a feature, add tests appropriate to risk:

- Domain/unit tests for rules and calculations
- Integration tests for database constraints and workflow transitions
- Authorization tests for protected endpoints
- End-to-end tests for critical user workflows
- Extra concurrency/integrity tests for voting and financial posting

Before declaring work complete:

1. Build the solution.
2. Run relevant unit/integration tests.
3. Run formatting/static checks used by the repo.
4. Verify migrations when schema changed.
5. For UI workflows, run the relevant Playwright test when available.
6. Summarize what changed and any remaining risk.

## Git workflow

- Never push directly to `main` unless repository policy explicitly allows it for emergency administration.
- Prefer one focused branch per change.
- Keep commits scoped and descriptive.
- Do not combine unrelated refactors with a business feature.
- PR descriptions should include: purpose, scope, validation performed, database changes, security impact, and screenshots for UI changes when useful.

## Definition of done

A task is not done merely because it compiles. A feature is complete when:

- business rules are enforced server-side,
- authorization is correct,
- persistence and concurrency behavior are correct,
- tests cover the important paths,
- sensitive data is not leaked,
- migration/schema changes are reproducible,
- relevant documentation is updated.
