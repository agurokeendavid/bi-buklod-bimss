# BIMSS Phase 1 Backlog

This is the authoritative, in-repo tracker for Phase 1 ("Platform Foundation and
Membership") work — task status, what shipped where, and what's next. It exists so
anyone (a teammate, you on a different machine, or an AI coding agent) can pick the
project up without needing prior chat history.

Read `AGENTS.md` and the rest of `docs/` first for the standing rules this backlog
was built against. This file tracks task-level progress only; it doesn't restate
architecture or security rules that live elsewhere.

## Status legend

- `Done` — merged to `main`.
- `In branch` — implemented and committed, not yet pushed/merged. Branch name given.
- `Not started`

## How to pick this up cold

1. Run `git log --oneline -10 main` and `gh pr list --state all --limit 10` to
   confirm this file still matches reality (it can drift if someone forgets to
   update it — trust git over this doc if they disagree).
2. Find the first `Not started` (or unfinished `In branch`) task below, in order —
   later tasks generally depend on earlier ones.
3. Create a feature branch, implement, verify (build + test + `dotnet format
   --verify-no-changes`), commit, then confirm with whoever's driving before
   pushing/opening a PR.
4. Update this file's status/PR link in the same PR that completes the task.

**Current state (2026-08-16): Phase 1A, Phase 1B, and Phase 1C are fully
Done** (BIMSS-001 through BIMSS-032, plus the frontend-pivot prerequisites
BIMSS-046/047, all merged). Phase 1B's blocking business questions were
confirmed with Buklod on 2026-08-14 (see "Confirmed decisions" in
`docs/DATA_DICTIONARY.md`). Phase 1C (Membership Administration) shipped
on the new frontend pivot (see the note under Phase 1C below) — the
Next.js admin UI now covers the full member lifecycle: list, detail,
create, edit, activate/deactivate/verify with a document-upload gate, and
status/audit history. A cross-cutting frontend design pass (no BIMSS ID —
see the note at the end of the Phase 1C section) then restyled all of
Phase 1C's screens (theme, sidebar/header shell, dark mode, UX audit) —
Phase 1D/1E's own UI work should follow the conventions it established.
A second cross-cutting design pass (no BIMSS ID, 2026-08-16) then integrated
a fuller frontend design system handoff on top of that: design tokens (16px
root, navy sidebar rail), grouped sidebar nav, centralized status-badge
colors, and a re-skin of the five already-built screens (login, dashboard,
membership register, member record, new-member form) — see
`docs/design/README.md`'s "Integration status" for exactly what shipped
versus what stayed deferred (contributions/loans/approvals/reports/settings/
elections screens all need data or endpoints that don't exist yet). The
design system itself now lives at `docs/design/BIMSS-UI-SPEC.md`.
Phase 1D (Existing Member Import) is fully Done as of 2026-08-17
(BIMSS-033 through BIMSS-038) — see that section's note on BIMSS-038 for a
frontend verification gap (no live backend was available to click through
the new screens; covered instead by `ImportBatchesControllerTests`, plus
`npm run lint`/`build`). Phase 1E (Member Self-Service) is fully Done as
of 2026-08-17: BIMSS-039 through BIMSS-045 (member dashboard shell, My
Profile read, `MemberUpdateRequest`/Change schema, member submits update
request, officer review/approve/reject, direct self-service edit of
contact info, update request status/history view) are all Done. Along
the way, two gaps the plan had already implied but never filled got
fixed: `ApplicationUser.MemberId` (BIMSS-040, added in BIMSS-005, never
wired up) and `Permission.Membership.ManageSelf` (BIMSS-042, referenced
by this section's own note but never added). **All of Phase 1
(BIMSS-001 through BIMSS-045) is Done as of 2026-08-17.**

## Phase 1A — Platform Foundation

| ID | Title | Status |
|---|---|---|
| BIMSS-003 | CI build/test workflow | Done — [PR #1](https://github.com/agurokeendavid/bi-buklod-bimss/pull/1) |
| BIMSS-001 | Centralize build settings & `.editorconfig` | Done — [PR #2](https://github.com/agurokeendavid/bi-buklod-bimss/pull/2) |
| BIMSS-002 | Safe local configuration (`appsettings*.json.example`) | Done — [PR #2](https://github.com/agurokeendavid/bi-buklod-bimss/pull/2), revised in [PR #3](https://github.com/agurokeendavid/bi-buklod-bimss/pull/3) |
| BIMSS-004 | EF Core DbContext scaffolding | Done — [PR #5](https://github.com/agurokeendavid/bi-buklod-bimss/pull/5) |
| BIMSS-005 | ASP.NET Core Identity + first migration | Done — [PR #7](https://github.com/agurokeendavid/bi-buklod-bimss/pull/7) |
| BIMSS-006 | Permission/policy authorization model | Done — [PR #9](https://github.com/agurokeendavid/bi-buklod-bimss/pull/9) |
| BIMSS-007 | Audit logging foundation | Done — [PR #10](https://github.com/agurokeendavid/bi-buklod-bimss/pull/10) |
| BIMSS-008 | Global exception handling & typed exceptions | Done — [PR #11](https://github.com/agurokeendavid/bi-buklod-bimss/pull/11) |
| BIMSS-009 | Validation conventions | Done — [PR #12](https://github.com/agurokeendavid/bi-buklod-bimss/pull/12) |
| BIMSS-010 | DI composition conventions | Done — [PR #13](https://github.com/agurokeendavid/bi-buklod-bimss/pull/13) |
| BIMSS-011 | Base layout, navigation shell, template cleanup | Done — [PR #14](https://github.com/agurokeendavid/bi-buklod-bimss/pull/14) |
| BIMSS-012 | Testing foundation (architecture tests, shared integration fixture) | Done — [PR #15](https://github.com/agurokeendavid/bi-buklod-bimss/pull/15) |
| BIMSS-013 | Synthetic seed strategy (Identity portion) | Done — [PR #16](https://github.com/agurokeendavid/bi-buklod-bimss/pull/16) |

Also merged, not part of the original numbered backlog:
- **Code First EF Core + local secrets workflow documentation**
  ([PR #3](https://github.com/agurokeendavid/bi-buklod-bimss/pull/3)) — made the
  Code First approach explicit in `AGENTS.md`/`docs/ARCHITECTURE.md`, and
  simplified the local secrets workflow (see "Secrets convention" below).
- **This backlog-tracking doc itself**
  ([PR #4](https://github.com/agurokeendavid/bi-buklod-bimss/pull/4)).

### BIMSS-003 — CI build/test workflow (Done)

`.github/workflows/ci.yml`: restore/build/test `Bimss.slnx` on push/PR to `main`.
Also fixed a pre-existing `Bimss.Api` build break unrelated to CI itself: pinned
`Microsoft.OpenApi` to `2.7.5` (10.0.10 `Microsoft.AspNetCore.OpenApi` doesn't
compile against `Microsoft.OpenApi` 3.10.0's read-only `IOpenApiMediaType.Example`).

### BIMSS-001 — Centralize build settings & `.editorconfig` (Done)

Root `Directory.Build.props` (`TargetFramework`, `ImplicitUsings`, `Nullable`) and
`.editorconfig` (formatting + C#/.NET style). Normalized source-file encoding to
UTF-8 with BOM to match the repo's existing majority convention, so `dotnet format
--verify-no-changes` passes cleanly.

### BIMSS-002 — Safe local configuration (Done, revised)

Real `appsettings.json`/`appsettings.Development.json` for `Bimss.Web` and
`Bimss.Api` are **git-ignored**, not committed — they're expected to hold real
connection strings/API tokens as the app grows. Committed
`appsettings*.json.example` templates take their place. See "Secrets convention"
below for the full local/deployment workflow — this is a hard rule, confirmed
with the project owner twice; do not revert to committing real config files.

### BIMSS-004 — EF Core DbContext scaffolding (Done)

Merged via [PR #5](https://github.com/agurokeendavid/bi-buklod-bimss/pull/5).
Verified locally before merge (clean build, full test suite including a real
Testcontainers-backed SQL Server connectivity test, `dotnet format
--verify-no-changes`) and CI passed on the PR.

What it contains:
- `Bimss.Infrastructure/Persistence/BimssDbContext.cs` — zero entities so far
  (Membership entities land in Phase 1B); calls
  `modelBuilder.ApplyConfigurationsFromAssembly(...)` so future
  `IEntityTypeConfiguration<T>` classes are picked up automatically.
- `Bimss.Infrastructure/Persistence/BimssDbContextFactory.cs` — design-time
  factory for `dotnet ef` tooling.
- `Bimss.Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs` —
  `AddBimssPersistence(IServiceCollection, IConfiguration)`, wired into both
  `Bimss.Web/Program.cs` and `Bimss.Api/Program.cs`. Reads `ConnectionStrings:Bimss`;
  nothing touches the database until something actually resolves the context.
- `Microsoft.EntityFrameworkCore.SqlServer`/`.Design` pinned to exact `10.0.11`
  (was floating `10.0.*`).
- `.config/dotnet-tools.json` — local `dotnet-ef` tool manifest pinned to `10.0.11`
  (see "Environment notes" below for why).
- `Bimss.Web` is the designated EF Core startup project for tooling purposes (it
  references `Microsoft.EntityFrameworkCore.Design` for this); `Bimss.Api` does
  not need to.
- Verified the whole toolchain with a throwaway `dotnet ef migrations add`/`remove`
  (removed before commit — there's nothing worth migrating with zero entities;
  the first real migration is deferred to BIMSS-005 per the implementation order
  below).
- `tests/Bimss.IntegrationTests/Persistence/BimssDbContextConnectivityTests.cs` —
  connects `BimssDbContext` to a real SQL Server container via
  `Testcontainers.MsSql` and asserts `CanConnectAsync()`. Requires Docker running
  locally (see "Environment notes").

### BIMSS-005 — ASP.NET Core Identity + first migration (Done)

Merged via [PR #7](https://github.com/agurokeendavid/bi-buklod-bimss/pull/7).

- `ApplicationUser : IdentityUser<Guid>` (nullable `MemberId` link, no FK yet —
  `Member` doesn't exist until BIMSS-015) and `ApplicationRole : IdentityRole<Guid>`
  live in `Bimss.Infrastructure/Identity/`, not `Bimss.Domain` — Identity types
  need an ASP.NET Core reference, which `Bimss.Domain` must stay free of for
  BIMSS-012's planned architecture test.
- `BimssDbContext` is now `IdentityDbContext<ApplicationUser, ApplicationRole,
  Guid>`.
- `Bimss.Infrastructure/Identity/IdentityServiceCollectionExtensions.cs` —
  `AddBimssIdentity()`, mirroring the existing `AddBimssPersistence` pattern;
  called from both `Bimss.Web/Program.cs` and `Bimss.Api/Program.cs`, alongside
  `app.UseAuthentication()` added before `app.UseAuthorization()` in both hosts'
  middleware pipelines.
- Password/lockout policy tightened beyond framework defaults: min length 12,
  digit/lower/upper/non-alphanumeric required, 4 required unique chars, lockout
  after 5 failed attempts for 15 minutes, unique email required.
  `SignIn.RequireConfirmedAccount` stays `false` — no email-sending
  infrastructure exists yet to confirm accounts with; revisit once that lands.
- `Bimss.Infrastructure.csproj` needed a `FrameworkReference` to
  `Microsoft.AspNetCore.App` (it's a plain `Microsoft.NET.Sdk` library, not
  `Sdk.Web`, so `AddIdentity`/`IdentityOptions` aren't available without it) plus
  the new `Microsoft.AspNetCore.Identity.EntityFrameworkCore` package, pinned to
  `10.0.11` to match the other EF Core packages. The `FrameworkReference` made
  the existing explicit `Microsoft.Extensions.Configuration.Json`/
  `.EnvironmentVariables` package references redundant (now provided
  transitively); removed them.
- First real migration: `InitialIdentity`, under
  `src/Bimss.Infrastructure/Persistence/Migrations/` (generated with
  `--output-dir Persistence/Migrations` to sit alongside `BimssDbContext`
  instead of EF's default project-root `Migrations/` folder). Creates
  `AspNetUsers` (with `MemberId`), `AspNetRoles`, `AspNetUserRoles`,
  `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`.
- `tests/Bimss.IntegrationTests/Persistence/InitialIdentityMigrationTests.cs` —
  same `IAsyncLifetime`/`Testcontainers.MsSql` pattern as
  `BimssDbContextConnectivityTests.cs`; applies the migration for real and
  queries `Users`/`Roles`.
- `tests/Bimss.UnitTests/Identity/IdentityOptionsConfigurationTests.cs` —
  exercises `ConfigureIdentityOptions` directly against a plain `IdentityOptions`
  instance (no DB/DI needed); `Bimss.UnitTests.csproj` gained its first project
  reference (`Bimss.Infrastructure`).
- No role/permission seeding — that's BIMSS-013.
- Verified locally: clean rebuild, `dotnet build --configuration Release`,
  `dotnet test --configuration Release` (7/7 passing, including the two
  Testcontainers-backed integration tests), `dotnet format --verify-no-changes`.

### BIMSS-006 — Permission/policy authorization model (Done)

Merged via [PR #9](https://github.com/agurokeendavid/bi-buklod-bimss/pull/9).

- `Permission` catalog (`Bimss.Domain/Authorization/Permission.cs`) — constants
  grouped by module, matching `docs/ARCHITECTURE.md`'s list exactly, plus
  `Permission.All` and the `bimss:permission` claim type. Lives in
  `Bimss.Domain` (plain constants, no framework dependency, reachable
  everywhere via the project-reference graph) rather than Infrastructure.
- `RolePermission` join entity (`Bimss.Infrastructure/Identity/`) — composite
  key `(RoleId, PermissionName)`, FK to `AspNetRoles` cascade delete.
  Migration: `AddRolePermissions`.
- `PermissionClaimsTransformation : IClaimsTransformation` — adds one claim
  per permission from the signed-in user's role assignments; guards against
  re-adding claims if it runs more than once per request.
- `AddBimssAuthorization()` (`Bimss.Infrastructure/Authorization/`) —
  registers the claims transformation and a named `AuthorizationPolicy` per
  `Permission.All` entry requiring that permission's claim. Called from both
  hosts' `Program.cs` after `AddBimssIdentity()`. BIMSS-010 will later fold
  this together with `AddBimssIdentity`/`AddBimssPersistence` into the
  consolidated extension methods it describes.
- `DiagnosticsController.AuthorizedPing` (`GET
  /api/diagnostics/authorized-ping`) — minimal `[Authorize(Policy =
  Permission.Audit.View)]` endpoint demonstrating the model end-to-end.
- Tests: `PermissionCatalogTests`/`AuthorizationPolicyRegistrationTests`
  (unit), `PermissionClaimsTransformationTests` (Testcontainers-backed —
  claims correctly derived from seeded role/permission data),
  `DiagnosticsAuthorizationTests` (`WebApplicationFactory<Program>` +
  test-only auth scheme — unauthenticated 401, authenticated-without-
  permission 403, authenticated-with-permission 200). `Bimss.Api/Program.cs`
  gained `public partial class Program;` so the test host can boot it.
- No role/permission seeding — that's BIMSS-013.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (17/17 passing) in
  Release, `dotnet format --verify-no-changes`.

### BIMSS-007 — Audit logging foundation (Done)

Merged via [PR #10](https://github.com/agurokeendavid/bi-buklod-bimss/pull/10).

- `AuditResult` enum (`Bimss.Domain/Auditing/`) — shared vocabulary type.
- `AuditEntry` (`Bimss.Application/Auditing/`) — immutable value object
  (actor, action, object type/id, result, remarks, optional
  `IReadOnlyDictionary<string, string>?` metadata); validates
  `Action`/`ObjectType`/`ObjectId` non-blank at construction.
- `IAuditLogger` (`Bimss.Application/Auditing/`) — the port an application
  service calls; its XML doc restates the no-beneficiary/no-ballot/
  no-full-address rule from `docs/SECURITY_AND_PRIVACY.md`. First real content
  in `Bimss.Application` — removed its scaffold `Class1.cs`.
- `AuditEvent` (`Bimss.Infrastructure/Auditing/`) — EF entity, deliberately no
  FK from `ActorUserId` to `AspNetUsers` so audit history survives identity
  changes. Migration: `AddAuditEvents`.
- `AuditLogger` — `IAuditLogger` implementation; persists immediately via its
  own `SaveChangesAsync` (not bundled into the caller's transaction), using an
  injected `TimeProvider` for a testable timestamp.
- `AddBimssAuditing()` — registers `TimeProvider.System` and the logger;
  called from both hosts' `Program.cs` after `AddBimssAuthorization()`.
- Tests: `AuditEntryTests` (unit — construction guard clauses; note
  `Assert.ThrowsAny<ArgumentException>`, not `Assert.Throws`, since
  `ArgumentException.ThrowIfNullOrWhiteSpace` throws `ArgumentNullException`
  specifically for `null`), `AuditLoggerTests` (Testcontainers-backed — a full
  entry round-trips through persistence with metadata correctly serialized to
  JSON, satisfying the acceptance criterion directly; a failure-result entry
  with no actor/remarks/metadata persists those fields as null).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (29/29 passing) in
  Release, `dotnet format --verify-no-changes`.

### BIMSS-008 — Global exception handling & typed exceptions (Done)

Merged via [PR #11](https://github.com/agurokeendavid/bi-buklod-bimss/pull/11).

- `BimssException` (abstract) + `NotFoundException`/`ConflictException`/
  `ForbiddenException`/`DomainValidationException` (`Bimss.Domain/Exceptions/`)
  — zero framework dependency, throwable from any layer.
  `DomainValidationException` optionally carries field-level errors.
- `ExceptionClassifier`/`ExceptionClassification`
  (`Bimss.Infrastructure/ExceptionHandling/`) — single shared exception →
  (status, title, detail) mapping used by both hosts. Unmapped exceptions
  always classify to 500 with a fixed generic detail — never the original
  `.Message`.
- `Bimss.Api`: `BimssExceptionHandler : IExceptionHandler` writes
  `ProblemDetails`/`ValidationProblemDetails`; registered via
  `AddExceptionHandler`/`AddProblemDetails` and `app.UseExceptionHandler()`
  as the first pipeline middleware. Development-only 500 responses get
  `exceptionType`/`stackTrace` extensions.
- `Bimss.Web`: `HomeController.Error()` classifies the original exception
  (via `IExceptionHandlerPathFeature`) with the same classifier and sets the
  response status code before rendering `/Home/Error` — only reached outside
  Development (existing `!IsDevelopment()` gate); Development keeps using the
  framework's built-in developer exception page.
- `DiagnosticsController.Throw` (`GET /api/diagnostics/throw?type=...`,
  `[AllowAnonymous]`) — test-only endpoint throwing each exception type on
  demand.
- Also added `tests/Bimss.IntegrationTests/xunit.runner.json`
  (`parallelizeTestCollections: false`): running the growing set of
  Testcontainers-backed test classes in parallel exhausted local Docker
  resources and crashed the test host mid-run
  (`MSB4166: Child node exited prematurely`). Sequential execution fixed it;
  BIMSS-012's shared fixture is the real long-term fix.
- Tests: `ExceptionClassifierTests` (unit); `ExceptionHandlingTests`
  (`WebApplicationFactory<Program>`) — each typed exception → correct status;
  an unexpected exception in a `Production`-environment client never leaks
  its message/type/stack trace, while a `Development`-environment client does
  get the exception type, proving the environment gate is real.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (40/40 passing,
  integration tests now ~9 min sequential) in Release,
  `dotnet format --verify-no-changes`.

### BIMSS-009 — Validation conventions (Done)

Merged via [PR #12](https://github.com/agurokeendavid/bi-buklod-bimss/pull/12).

Mostly a documented convention (`AGENTS.md`'s new "Validation rules"
section) rather than a standalone feature — no real business DTO/Domain
aggregate exists yet to attach it to permanently (Phase 1B).

- DTO/API boundary: DataAnnotations on `Bimss.Contracts` request types;
  `[ApiController]`'s automatic 400 `ValidationProblemDetails` means no
  manual `ModelState.IsValid` checks in API controllers. No FluentValidation
  yet.
- DataAnnotations never replace server-side business-rule enforcement.
- Simple argument guards: BCL `ArgumentException.ThrowIfNullOrWhiteSpace`
  etc. (precedent: `Bimss.Application.Auditing.AuditEntry`, BIMSS-007) — no
  custom `Guard` abstraction.
- Genuine business-rule violations: the BIMSS-008 typed exception hierarchy,
  never a bare `Exception` — already maps to correct HTTP status codes via
  BIMSS-008's global exception handling.
- Concrete demonstration of the DTO-boundary half: `ValidationCheckRequest`
  is `Bimss.Contracts`' first real content (removed its scaffold
  `Class1.cs`), validated automatically at
  `DiagnosticsController.ValidateSample`.
- Tests: `ValidationCheckRequestTests` (unit, `Validator.TryValidateObject`
  directly against the DTO) and `ValidationTests` (integration, same cases
  end-to-end through `WebApplicationFactory<Program>`) — valid request → 200;
  missing `Name`, out-of-range `Age`, malformed `Email` → 400 with the
  expected field in `ValidationProblemDetails.Errors`.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (50/50 passing,
  integration ~11 min sequential) in Release,
  `dotnet format --verify-no-changes`.

### BIMSS-010 — DI composition conventions (Done)

Merged via [PR #13](https://github.com/agurokeendavid/bi-buklod-bimss/pull/13).

- `AddBimssInfrastructure(IConfiguration)` (`Bimss.Infrastructure/`) —
  consolidates `AddBimssPersistence`/`AddBimssIdentity`/`AddBimssAuditing`
  (the granular methods still exist individually; only the `Program.cs` call
  sites changed).
- `AddBimssApplication()` (`Bimss.Application/`, its first DI registration
  code) — currently a no-op; `IAuditLogger` etc. are still registered by
  Infrastructure. Placeholder for future Application-layer registrations.
  `Bimss.Application.csproj` gained a
  `Microsoft.Extensions.DependencyInjection.Abstractions` reference.
- `AddBimssAuthorization()` (BIMSS-006) unchanged — stays its own top-level
  call, per the backlog's naming.
- Both hosts' `Program.cs`:
  `AddBimssInfrastructure(configuration)` → `AddBimssApplication()` →
  `AddBimssAuthorization()`. `Bimss.Api` keeps its own
  `AddExceptionHandler`/`AddProblemDetails` (API-specific).
- No MediatR/CQRS framework.
- Tests: `ServiceCollectionCompositionTests` (unit) — builds a bare
  `ServiceCollection`, calls all three, resolves `BimssDbContext`,
  `UserManager<ApplicationUser>`, `RoleManager<ApplicationRole>`,
  `IAuditLogger`, `IAuthorizationPolicyProvider`. Caught a real gap: the bare
  collection needs `AddLogging()` itself (the real hosts get it free from
  `WebApplication.CreateBuilder`) since `UserManager<TUser>` requires
  `ILogger<>`. All prior integration tests continue passing unchanged,
  which is meaningful regression coverage for the consolidated registration
  path.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (51/51 passing) in
  Release, `dotnet format --verify-no-changes`. Local Docker/WSL2 memory
  pressure caused several `dotnet test` runs to crash mid-run this session as
  the Testcontainers-backed test classes multiplied
  (`MSB4166: Child node exited prematurely`, or the process silently killed);
  fixed locally each time by freeing memory (`wsl --shutdown`, killing
  orphaned `dotnet.exe` build-server processes that don't release memory
  between runs) and splitting verification into filtered batches
  (Testcontainers classes vs. `WebApplicationFactory` classes run
  separately). This is a local-machine resource constraint, not a code
  issue — CI's GitHub-hosted runners have a clean environment each run and
  aren't affected. BIMSS-012's shared fixture (one container instead of one
  per test class) is still the real long-term fix.

### BIMSS-011 — Base layout, navigation shell, template cleanup (Done)

Merged via [PR #14](https://github.com/agurokeendavid/bi-buklod-bimss/pull/14).

- `AccountController` (`Bimss.Web/Controllers/`) — `Login` (GET/POST via
  `SignInManager<ApplicationUser>.PasswordSignInAsync`, generic "Invalid
  username or password." on failure — no username enumeration) and `Logout`
  (POST, anti-forgery protected, never a GET).
- `LoginViewModel` — DataAnnotations-validated, per BIMSS-009's convention.
- `_LoginPartial.cshtml` in `_Layout.cshtml`'s nav — "Hello, {UserName}! |
  Logout" when signed in, "Login" link otherwise.
- BIMSS branding replaces default template scaffold; `WeatherForecastController`/
  `WeatherForecast` removed from `Bimss.Api`.
- No self-service registration — BIMSS-013 (seed strategy) creates dev/test
  accounts instead.
- **Also dropped Docker/Testcontainers from `Bimss.IntegrationTests`
  entirely**, at the user's explicit request — see "Testing convention: EF
  Core InMemory, not Testcontainers" under Environment notes below for the
  full rationale, the accepted trade-off, and the DI-override pattern
  `LoginTests` needed. Production code (`Bimss.Web`/`Bimss.Api`) is
  unaffected — still real SQL Server via `AddBimssPersistence`.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (51/51 passing, ~1.5s
  total — no Docker) in Release, `dotnet format --verify-no-changes`. Live
  browser check of the rendered home/login/privacy pages (no local dev DB
  configured, so the actual login submission was verified via `LoginTests`
  instead, not live in the browser).

### BIMSS-012 — Testing foundation (Done)

Merged via [PR #15](https://github.com/agurokeendavid/bi-buklod-bimss/pull/15).

- `LayeringRulesTests` (`Bimss.ArchitectureTests`, `NetArchTest.Rules`) —
  `Bimss.Domain` has no `Microsoft.EntityFrameworkCore`/`Microsoft.AspNetCore`
  dependency; `Bimss.Application` has no `Bimss.Infrastructure` dependency.
  First real content in `Bimss.ArchitectureTests` — removed its scaffold
  `UnitTest1.cs`; added project references to Domain/Application/Infrastructure
  so the tests can inspect those assemblies. Sanity-checked the Domain rule
  actually catches a violation (temporarily added an EF Core dependency to
  `Bimss.Domain`, confirmed it doesn't silently pass, fully reverted).
- `InMemoryBimssDbContextFactory` (`Bimss.IntegrationTests/Support/`) —
  shared helper extracting the repeated `UseInMemoryDatabase(name)`
  construction from `PermissionClaimsTransformationTests`,
  `AuditLoggerTests`, and `LoginTests`'s seeding context. This is the
  "shared, reusable" half of this task's original scope, adapted to
  InMemory (see "Testing convention: EF Core InMemory, not Testcontainers"
  above — the originally-planned Testcontainers.MsSql collection fixture
  was superseded in BIMSS-011).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (52/52 passing, ~2s
  total, no Docker) in Release, `dotnet format --verify-no-changes`.
- Dependencies: BIMSS-004, BIMSS-005.

### BIMSS-013 — Synthetic seed strategy (Identity portion) (Done)

Merged via [PR #16](https://github.com/agurokeendavid/bi-buklod-bimss/pull/16).
Last task in the current Phase 1A run — see "Where to pick this up next" below.

- `DevelopmentIdentitySeeder` (`Bimss.Infrastructure/Identity/Seeding/`) —
  six synthetic roles (`Administrator`, `Member`, `MembershipOfficer`,
  `FinanceOfficer`, `ElectionCommittee`, `Auditor`) with permission
  assignments matching `docs/SECURITY_AND_PRIVACY.md`'s own role examples,
  one synthetic dev account per role (`{role}.dev@bimss.local`, shared
  documented-synthetic password satisfying BIMSS-005's password policy).
  Fully idempotent; wired into both hosts' `Program.cs`, strictly gated on
  `IsDevelopment()`.
- Found and fixed two real bugs along the way:
  1. A classic EF Core InMemory gotcha in the seeder's own unit test: an
     `AddDbContext(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))`
     configure lambda generates a **new** GUID per `DbContext` instantiation
     (once per scope), so the seeding scope and assertion scope silently
     pointed at two different InMemory databases. Fix: capture the database
     name once, outside the lambda — the pattern already used correctly
     elsewhere (`InMemoryBimssDbContextFactory`, `LoginTests`).
  2. `WebApplicationFactory` defaults to the `"Development"` environment
     name, so `DiagnosticsApiFactory`'s consumers and `LoginTests` — none of
     which stand up a real database — started failing once the seeder ran
     on every Development startup and tried to reach a nonexistent real SQL
     Server. Fix: `DiagnosticsApiFactory` and `LoginTests` now set
     environment `"Testing"` instead of leaving the default;
     `ExceptionHandlingTests.Get_IncludesExceptionDetails_InDevelopment`
     (which genuinely needs `"Development"` for an unrelated reason) keeps
     it but adds an InMemory `DbContext` override for the seeder to target.
- Tests: `DevelopmentIdentitySeederTests` — each role has its expected exact
  permission set; each dev user exists, is assigned its role, and its
  password validates; a second `SeedAsync` call is a true no-op.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (55/55 passing, run
  twice to rule out startup-seeding flakiness) in Release,
  `dotnet format --verify-no-changes`.
- Dependencies: BIMSS-005, BIMSS-006.

## Phase 1B — Membership Domain

| ID | Title | Status |
|---|---|---|
| BIMSS-014 | Reference/master data tables (CivilStatus, Suffix, OfficeUnit, EducationalAttainment, EligibilityType, RelationshipType, MemberStatusReason) | Done — [PR #18](https://github.com/agurokeendavid/bi-buklod-bimss/pull/18) |
| BIMSS-015 | Member core aggregate + `MemberStatusHistory` | Done — [PR #19](https://github.com/agurokeendavid/bi-buklod-bimss/pull/19) |
| BIMSS-016 | `MemberEmployment` | Done — [PR #20](https://github.com/agurokeendavid/bi-buklod-bimss/pull/20) |
| BIMSS-017 | `MemberContact` & `MemberAddress` | Done — [PR #21](https://github.com/agurokeendavid/bi-buklod-bimss/pull/21) |
| BIMSS-018 | `MemberEducation` & `MemberEligibility` | Done — [PR #22](https://github.com/agurokeendavid/bi-buklod-bimss/pull/22) |
| BIMSS-019 | `MemberFamilyInformation` & `MemberChild` | Done — [PR #23](https://github.com/agurokeendavid/bi-buklod-bimss/pull/23) |
| BIMSS-020 | `MemberPrivacyConsent` | Done — [PR #24](https://github.com/agurokeendavid/bi-buklod-bimss/pull/24) |
| BIMSS-021 | `MemberDocument` metadata + storage abstraction | Done — [PR #25](https://github.com/agurokeendavid/bi-buklod-bimss/pull/25) |
| BIMSS-022 | Member creation use case | Done — [PR #26](https://github.com/agurokeendavid/bi-buklod-bimss/pull/26) |
| BIMSS-023 | Member read/query use cases | Done — [PR #27](https://github.com/agurokeendavid/bi-buklod-bimss/pull/27) |
| BIMSS-024 | Member status transition service | Done — [PR #28](https://github.com/agurokeendavid/bi-buklod-bimss/pull/28) |
| BIMSS-025 | Synthetic membership seed data | Done — [PR #29](https://github.com/agurokeendavid/bi-buklod-bimss/pull/29) |
| BIMSS-026 | Membership schema/constraint integration tests | Done — [PR #30](https://github.com/agurokeendavid/bi-buklod-bimss/pull/30) |

**Business questions confirmed with Buklod (2026-08-14)** — see "Confirmed
decisions" in `docs/DATA_DICTIONARY.md` for full detail: BI Employee Number is
unique and mandatory (BIMSS-016); self-service direct edit without approval is
limited to contact info only, everything else requires officer review (affects
Phase 1E — BIMSS-030 vs. BIMSS-042/044 split); proof of employment is
mandatory, accepted types PDF/JPG/PNG (BIMSS-021); `MemberChild` requires both
name and birth date (BIMSS-019).

### BIMSS-014 — Reference/master data tables (Done)

Merged via [PR #18](https://github.com/agurokeendavid/bi-buklod-bimss/pull/18).

- `ReferenceDataItem` abstract base (`Bimss.Domain/Membership/ReferenceData/`)
  — the first Membership domain code in the repo, and the first EF-mapped
  entity to live in `Bimss.Domain` rather than `Bimss.Infrastructure` (plain
  POCO, zero EF/AspNetCore dependency, satisfies `LayeringRulesTests`):
  `Id`/`Code`/`Name`/`IsActive`; constructor guards `Code`/`Name` with
  `ArgumentException.ThrowIfNullOrWhiteSpace` (same pattern as
  `Bimss.Application.Auditing.AuditEntry`'s constructor, BIMSS-007);
  `SetActive(bool)` — reference rows are deactivated, never hard-deleted,
  once a Member record references them.
- Seven sealed one-line subclasses: `CivilStatus`, `Suffix`, `OfficeUnit`,
  `EducationalAttainment`, `EligibilityType`, `RelationshipType`,
  `MemberStatusReason`. EF Core binds each via its constructor (parameter
  names match inherited property names) — no parameterless constructor,
  no incidental public setters.
- `ReferenceDataItemConfiguration<T>` abstract base
  (`Bimss.Infrastructure/Membership/ReferenceData/`) — shared
  `IEntityTypeConfiguration<T>` Fluent API (`Code` required/max 50/unique
  index, `Name` required/max 200); seven one-line concrete configs each
  just naming their table (`CivilStatuses`, `Suffixes`, `OfficeUnits`,
  `EducationalAttainments`, `EligibilityTypes`, `RelationshipTypes`,
  `MemberStatusReasons`). `BimssDbContext` gained seven matching `DbSet<T>`
  properties; `ApplyConfigurationsFromAssembly` picked up the new configs
  with no further wiring.
- Migration: `AddMembershipReferenceData`, under
  `src/Bimss.Infrastructure/Persistence/Migrations/` — seven `CreateTable`
  calls plus a unique index on `Code` per table, no changes to unrelated
  tables.
- Scope is schema only, per the task title — no seed rows (BIMSS-025 is the
  synthetic seed-data task) and no admin UI (not in the Phase 1 backlog).
- Tests: `ReferenceDataItemTests` (unit — constructor guard clauses via
  `CivilStatus` as the representative type, `SetActive`, plus a smoke test
  constructing all seven concrete types); `ReferenceDataConfigurationTests`
  (unit, `[Theory]` over all seven types — asserts table name, required/max
  length, and unique index via `DbContext.Model` metadata directly, which
  works correctly under the InMemory provider even though InMemory doesn't
  *enforce* unique indexes at runtime — see "Testing convention: EF Core
  InMemory, not Testcontainers" below); `ReferenceDataPersistenceTests`
  (integration — round-trip add/reload through `InMemoryBimssDbContextFactory`,
  plus a `SetActive` persistence check).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (73/73 passing) in
  Release, `dotnet format --verify-no-changes`. Migration diff reviewed for
  sanity (seven tables, matching unique indexes, nothing else touched).
- Dependencies: BIMSS-004.

### BIMSS-015 — Member core aggregate + `MemberStatusHistory` (Done)

Merged via [PR #19](https://github.com/agurokeendavid/bi-buklod-bimss/pull/19).

- `Member` (`Bimss.Domain/Membership/`) — the first real Membership
  aggregate; core personal identity fields from `docs/DATA_DICTIONARY.md`'s
  Excel mapping (`LastName`, `FirstName`, `MiddleName`, `SuffixId` nullable
  FK, `DateOfBirth`, `PlaceOfBirth`, `CivilStatusId` required FK,
  `JoiningReason`). Constructor guards `LastName`/`FirstName`/`PlaceOfBirth`
  non-blank and `CivilStatusId` non-empty, same
  `ArgumentException.ThrowIfNullOrWhiteSpace` pattern as `ReferenceDataItem`.
  A second, private, EF-only constructor exists purely for materialization
  (see "Environment notes" below — a plain business constructor with an
  unmapped parameter like `occurredAtUtc` breaks EF's constructor binding).
- Status lifecycle for Phase 1, confirmed with the user (2026-08-14, since
  it's still an open question in Buklod's own backlog list — "Are
  retirees/former employees/honorary members possible?"):
  `PendingVerification -> Active -> Inactive`, matching the confirmed
  workflow that proof of employment is mandatory before member verification.
  `Verify`/`Deactivate`/`Reactivate` validate the current state (throwing
  `Bimss.Domain.Exceptions.ConflictException`, BIMSS-008, on an invalid
  transition) and match `Permission.Membership.Verify` and
  `ARCHITECTURE.md`'s audited "member verification" action.
  `Deactivate` requires a non-empty `MemberStatusReason` id. Doesn't
  foreclose retiree/honorary handling later — can be added as a new status
  or an `Inactive` reason once Buklod confirms.
- `MemberStatusHistory` (`Bimss.Domain/Membership/`) — one row per
  transition, including the initial creation row (`FromStatus: null`).
  `internal` constructor: only `Member` can create one, enforcing that
  history stays consistent with actual transitions.
- `MemberConfiguration`/`MemberStatusHistoryConfiguration`
  (`Bimss.Infrastructure/Membership/`) — `Member`'s FKs to `Suffix`/
  `CivilStatus` are `DeleteBehavior.Restrict` (reference rows are
  deactivated, never deleted); `StatusHistory` is a field-backed collection
  navigation (`UsePropertyAccessMode(PropertyAccessMode.Field)`) that
  cascade-deletes with its `Member`. No FK from
  `MemberStatusHistory.ActorUserId` to `AspNetUsers` — same deliberate
  choice as `AuditEventConfiguration` ("audit records must outlive the
  identity they reference"), just an index instead.
- Migration: `AddMemberCoreAggregate` — creates `Members` and
  `MemberStatusHistory`, with FKs to `CivilStatuses`/`Suffixes`/
  `MemberStatusReasons`/`Members`.
- Tests: `MemberTests` (unit — constructor guard clauses; initial state is
  `PendingVerification` with one history row; each transition method's
  success/failure paths); `MemberConfigurationTests` (unit, same
  `DbContext.Model` metadata-inspection style as
  `ReferenceDataConfigurationTests`); `MemberPersistenceTests` (integration
  — round-trip through `InMemoryBimssDbContextFactory`; a second test drives
  `Verify` then `Deactivate` across three separate context reloads and
  confirms `StatusHistory` accumulates all three rows correctly).
- Scope is schema/domain-rule only, per the task title — no Application use
  case, controller, or admin UI (BIMSS-022/023/024, Phase 1C).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (95/95 passing) in
  Release, `dotnet format --verify-no-changes`. Migration diff reviewed for
  sanity (two tables, expected FKs/indexes, nothing else touched).
- Dependencies: BIMSS-014.

### BIMSS-016 — `MemberEmployment` (Done)

Merged via [PR #20](https://github.com/agurokeendavid/bi-buklod-bimss/pull/20).

- `MemberEmployment` (`Bimss.Domain/Membership/`) — BI employment data,
  standalone entity referencing `Member` by FK rather than a nested child in
  `Member`'s aggregate graph (unlike `MemberStatusHistory`): its updates flow
  through a separate officer-review workflow (Phase 1E), not through `Member`'s
  own domain methods, so it doesn't need `Member` to own its lifecycle.
  `EmployeeNumber` (required, unique — confirmed mandatory/unique business
  identifier), `PositionDesignation` (required free text — no reference table
  exists for it yet, per `docs/DATA_DICTIONARY.md`'s "reference candidate"
  note being deferred), `OfficeUnitId` (required FK to `OfficeUnit`, restrict
  delete), `PermanentAppointmentDate` (nullable — a `PendingVerification`
  member may not have one yet). All constructor parameters map 1:1 to
  persisted properties, so (unlike `Member`) no second EF-only constructor
  was needed. `UpdateDetails(...)` guarded mutator for position/office/
  appointment-date changes; `EmployeeNumber` itself is not mutable through
  this entity.
- `MemberEmploymentConfiguration` (`Bimss.Infrastructure/Membership/`) — one
  employment record per member, enforced via a unique index on `MemberId`
  (following the existing "no navigation property, reference by ID" style
  used for `Suffix`/`CivilStatus`/`OfficeUnit` elsewhere, rather than a
  formal EF one-to-one relationship). FK to `Member` is `Cascade` (the row
  is meaningless without its member, same reasoning as `MemberStatusHistory`);
  FK to `OfficeUnit` is `Restrict` (reference data is deactivated, never
  deleted).
- Migration: `AddMemberEmployment` — one table, two unique indexes
  (`EmployeeNumber`, `MemberId`), FKs to `Members` (cascade) and
  `OfficeUnits` (restrict).
- Tests: `MemberEmploymentTests` (unit — constructor/`UpdateDetails` guard
  clauses and success paths); `MemberEmploymentConfigurationTests` (unit,
  same `DbContext.Model` metadata-inspection style as prior Membership
  configuration tests); `MemberEmploymentPersistenceTests` (integration —
  round-trip add/reload, `UpdateDetails` persists across reloads).
- Scope is schema/domain-rule only, per the task title — no Application use
  case, controller, or admin UI (BIMSS-022 onward, Phase 1C).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (111/111 passing) in
  Release, `dotnet format --verify-no-changes`. Migration diff reviewed for
  sanity (one table, expected FKs/indexes, nothing else touched).
- Dependencies: BIMSS-014, BIMSS-015.

### BIMSS-017 — `MemberContact` & `MemberAddress` (Done)

Merged via [PR #21](https://github.com/agurokeendavid/bi-buklod-bimss/pull/21).

- `MemberContact` (`Bimss.Domain/Membership/`) — `Landline` (nullable),
  `MobileNumber` (required), `Email` (required). One record per member
  (unique index on `MemberId`). These three fields are exactly the profile
  area Buklod confirmed members can edit directly without officer review
  (`docs/DATA_DICTIONARY.md`'s "Confirmed decisions" #2) — `UpdateDetails(...)`
  is written as the single guarded entry point a future self-service edit
  path (BIMSS-044) can call.
- `MemberAddress` (`Bimss.Domain/Membership/`) + `MemberAddressType` enum
  (`Present`, `Permanent`) — free-text `AddressLine` per
  `docs/DATA_DICTIONARY.md` ("nvarchar initially; later structured address if
  needed"). One row per `(member, type)`, enforced via a unique composite
  index rather than fixed `PresentAddress`/`PermanentAddress` columns on
  `Member` — keeps the type list open if a third address type is ever needed.
- Both `MemberContactConfiguration`/`MemberAddressConfiguration`
  (`Bimss.Infrastructure/Membership/`) reference `Member` with `Cascade`
  delete (meaningless without their member), same reasoning as
  `MemberStatusHistory`/`MemberEmployment`.
- Migration: `AddMemberContactAndAddress` — two tables, the two unique
  indexes above, FKs to `Members`.
- Tests: `MemberContactTests`/`MemberAddressTests` (unit — constructor/
  update-method guard clauses); `MemberContactConfigurationTests`/
  `MemberAddressConfigurationTests` (unit, `DbContext.Model`
  metadata-inspection style); `MemberContactPersistenceTests`/
  `MemberAddressPersistenceTests` (integration — round-trips, including both
  address types for the same member, and updates persisting across reloads).
- Scope is schema/domain-rule only — no Application use case, controller, or
  admin UI (BIMSS-022 onward, Phase 1C).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (135/135 passing) in
  Release, `dotnet format --verify-no-changes`. Migration diff reviewed for
  sanity (two tables, expected FKs/indexes, nothing else touched).
- Dependencies: BIMSS-015.

### BIMSS-018 — `MemberEducation` & `MemberEligibility` (Done)

Merged via [PR #22](https://github.com/agurokeendavid/bi-buklod-bimss/pull/22).

- `MemberEducation` (`Bimss.Domain/Membership/`) — `HighestAttainmentId`
  (required FK to the existing `EducationalAttainment` reference table) +
  free-text `DegreeCourse`. One record per member (unique index on
  `MemberId`), same "no navigation, unique-index-per-member" style as
  `MemberEmployment`/`MemberContact`.
- `MemberEligibility` (`Bimss.Domain/Membership/`) — `EligibilityTypeId`
  (required FK to the existing `EligibilityType` reference table) + free-text
  `Details` (per `docs/DATA_DICTIONARY.md`: "do not assume numeric" — license
  numbers can contain letters/formatting). Unlike every other Phase 1B child
  entity so far, this is a genuine one-to-many: a member can hold more than
  one eligibility (e.g. both Civil Service Professional and a PRC license),
  so `MemberId` is indexed for lookups but **not** unique — the
  configuration test explicitly asserts this to guard against copy-pasting
  the 1:1 pattern by mistake.
- Both `MemberEducationConfiguration`/`MemberEligibilityConfiguration`
  (`Bimss.Infrastructure/Membership/`) use `Cascade` delete to `Member` and
  `Restrict` delete to their reference tables, consistent with every prior
  Membership task.
- Migration: `AddMemberEducationAndEligibility` — two tables, the FKs/indexes
  above.
- Tests: `MemberEducationTests`/`MemberEligibilityTests` (unit — constructor/
  update-method guard clauses); `MemberEducationConfigurationTests`/
  `MemberEligibilityConfigurationTests` (unit, `DbContext.Model`
  metadata-inspection style, including the not-unique assertion above);
  `MemberEducationPersistenceTests`/`MemberEligibilityPersistenceTests`
  (integration — round-trips, including two eligibility rows for one member,
  and updates persisting across reloads).
- Scope is schema/domain-rule only — no Application use case, controller, or
  admin UI (BIMSS-022 onward, Phase 1C).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (152/152 passing) in
  Release, `dotnet format --verify-no-changes`. Migration diff reviewed for
  sanity (two tables, expected FKs/indexes, nothing else touched).
- Dependencies: BIMSS-014, BIMSS-015.

### BIMSS-019 — `MemberFamilyInformation` & `MemberChild` (Done)

Merged via [PR #23](https://github.com/agurokeendavid/bi-buklod-bimss/pull/23).

- `MemberFamilyInformation` (`Bimss.Domain/Membership/`) — `SpouseFullName`,
  `FatherFullName`, `MotherMaidenName`, `ParentsPresentAddress`, all nullable
  free text. Only `SpouseFullName` is explicitly conditional ("if married")
  per `docs/DATA_DICTIONARY.md`, but none of the others are marked mandatory
  either, so all four stayed optional rather than guessing a requiredness
  rule Buklod hasn't confirmed. One record per member (unique index on
  `MemberId`).
- `MemberChild` (`Bimss.Domain/Membership/`) — `Name` and `DateOfBirth` are
  **both mandatory**, per Buklod's confirmed decision that birth date is not
  optional (the one Phase 1B child field with an explicit mandatory rule
  beyond "required to exist at all"). A member can have multiple children, so
  this is a genuine one-to-many like `MemberEligibility` (BIMSS-018) —
  `MemberId` indexed but not unique.
- Both `MemberFamilyInformationConfiguration`/`MemberChildConfiguration`
  (`Bimss.Infrastructure/Membership/`) use `Cascade` delete to `Member`,
  consistent with every prior Membership task.
- Migration: `AddMemberFamilyInformationAndChild` — two tables, the FKs/
  indexes above.
- Tests: `MemberFamilyInformationTests`/`MemberChildTests` (unit —
  constructor/update-method guard clauses); `MemberFamilyInformationConfigurationTests`/
  `MemberChildConfigurationTests` (unit, `DbContext.Model`
  metadata-inspection style, including the unique-vs-not-unique `MemberId`
  assertion); `MemberFamilyInformationPersistenceTests`/
  `MemberChildPersistenceTests` (integration — round-trips, including two
  children for one member, and updates persisting across reloads).
- Scope is schema/domain-rule only — no Application use case, controller, or
  admin UI (BIMSS-022 onward, Phase 1C).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (169/169 passing) in
  Release, `dotnet format --verify-no-changes`. Migration diff reviewed for
  sanity (two tables, expected FKs/indexes, nothing else touched).
- Dependencies: BIMSS-015.

### BIMSS-020 — `MemberPrivacyConsent` (Done)

Merged via [PR #24](https://github.com/agurokeendavid/bi-buklod-bimss/pull/24).

- `MemberPrivacyConsent` (`Bimss.Domain/Membership/`) — `ConsentGiven` (bool),
  `NoticeVersion` (required string), `ConsentedAtUtc` (required timestamp),
  `Source` (required string, e.g. "Web Form"). Deliberately **immutable** —
  no update method. A new row is appended for every consent event
  (re-consenting to a later notice version, or withdrawing consent) rather
  than editing an existing row, per `AGENTS.md`'s rule against overwriting
  auditable records; a member accumulates one row per event, so `MemberId` is
  indexed but not unique — same shape as `MemberEligibility`/`MemberChild`.
- `docs/DATA_DICTIONARY.md`'s "Proposed core database tables" list also
  sketches a separate `PrivacyNoticeVersions` lookup table, but the
  field-mapping row for this data (#39) describes it as scalar fields ("bool
  + notice version + timestamp + source"), and no backlog task calls for a
  notice-version management table. Kept `NoticeVersion` as a plain string —
  introducing a full lookup/versioning entity for the notice text itself is
  out of this task's scope and can be added later if Buklod needs to manage
  notice *content*, not just track which version a member consented to.
- `MemberPrivacyConsentConfiguration` (`Bimss.Infrastructure/Membership/`)
  uses `Cascade` delete to `Member`, consistent with every prior Membership
  task.
- Migration: `AddMemberPrivacyConsent` — one table, the FK/index above.
- Tests: `MemberPrivacyConsentTests` (unit — constructor guard clauses,
  consent given/withheld); `MemberPrivacyConsentConfigurationTests` (unit,
  `DbContext.Model` metadata-inspection style, including the not-unique
  `MemberId` assertion); `MemberPrivacyConsentPersistenceTests` (integration
  — round-trip, multiple consent events accumulating for the same member).
- Scope is schema/domain-rule only — no Application use case, controller, or
  admin UI (BIMSS-022 onward, Phase 1C).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (181/181 passing) in
  Release, `dotnet format --verify-no-changes`. Migration diff reviewed for
  sanity (one table, expected FK/index, nothing else touched).
- Dependencies: BIMSS-015.

### BIMSS-021 — `MemberDocument` metadata + storage abstraction (Done)

Merged via [PR #25](https://github.com/agurokeendavid/bi-buklod-bimss/pull/25).

- `MemberDocument` (`Bimss.Domain/Membership/`) — metadata only, immutable
  (no update method — a corrected document is a new upload; old metadata
  stays for audit trail). `DocumentType` (free text — no reference table
  exists for it yet), `OriginalFileName` (display only, **never** trusted as
  a storage path per `docs/SECURITY_AND_PRIVACY.md`'s file-upload rules),
  `ContentType` (validated in the constructor against the confirmed accepted
  set — PDF/JPG/PNG — as a domain invariant, defense in depth beyond
  whatever API-level validation a future upload endpoint adds),
  server-generated `StorageKey`, `FileSizeBytes` (must be positive, via
  `ArgumentOutOfRangeException.ThrowIfLessThanOrEqual`), `UploadedAtUtc`,
  `UploadedByUserId` (nullable, no FK to `AspNetUsers` — same reasoning as
  `MemberStatusHistory.ActorUserId`). A member can upload multiple documents,
  so `MemberId` is indexed but not unique.
- `IMemberDocumentStorage` (`Bimss.Application/Membership/`) — the storage
  port, first real content besides `IAuditLogger` in `Bimss.Application`:
  `SaveAsync`/`OpenReadAsync`/`DeleteAsync` keyed by an opaque,
  server-generated storage key. Its doc comment states the "never trust
  caller input for storage names" rule directly.
- `LocalFileMemberDocumentStorage` (`Bimss.Infrastructure/Membership/`) —
  local-disk implementation. Storage keys are always GUIDs generated
  server-side, so a crafted key can't path-traverse. Root path is
  configurable (`DocumentStorage:RootPath`), defaulting to
  `App_Data/MemberDocuments` — outside `wwwroot`/executable locations.
  Registered as a singleton (stateless, no scoped dependencies) via
  `AddBimssMemberDocumentStorage`, folded into `AddBimssInfrastructure`.
  `.gitignore` gained `App_Data/` since uploaded documents must never be
  committed.
- Migration: `AddMemberDocument` — one table, unique index on `StorageKey`,
  non-unique index on `MemberId`, FK to `Members` (cascade).
- Tests: `MemberDocumentTests` (unit — constructor guard clauses, every
  accepted content type, rejection of an unaccepted type);
  `MemberDocumentConfigurationTests` (unit, `DbContext.Model`
  metadata-inspection style); `LocalFileMemberDocumentStorageTests`
  (integration — real disk I/O against a temp directory cleaned up via
  `IDisposable`: save/read round-trip, delete, distinct keys per save);
  `MemberDocumentPersistenceTests` (integration — round-trip, multiple
  documents per member). `Bimss.ArchitectureTests`' existing layering rule
  incidentally confirms the port/implementation split is real (Application
  still has no Infrastructure dependency).
- Scope is schema/domain-rule/storage-abstraction only — no Application use
  case, controller, or admin UI (BIMSS-022 onward, Phase 1C); there is no
  upload endpoint yet, so upload/download authorization is deferred to
  whichever task adds one.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (205/205 passing) in
  Release, `dotnet format --verify-no-changes`. Migration diff reviewed for
  sanity (one table, expected FK/indexes, nothing else touched).
- Dependencies: BIMSS-015.

### BIMSS-022 — Member creation use case (Done)

Merged via [PR #26](https://github.com/agurokeendavid/bi-buklod-bimss/pull/26).
First real Application-layer use case in Phase 1B — BIMSS-014–021 were
schema/domain-rule only.

- **Scope decision**: `CreateMemberCommand` bundles core `Member` fields +
  `MemberEmployment` fields only, not the full Google Form field set
  (contact, address, education, eligibility, family, children, privacy
  consent, documents). BI Employee Number is the only child field with a
  confirmed "required at creation" decision; everything else is added
  afterward through its own operation (the officer-review edit workflow,
  Phase 1E), keeping this task from becoming a single command coupled to
  every Phase 1B table.
- `MemberCreationService` (`Bimss.Application/Membership/`) — pre-checks
  `EmployeeNumber` uniqueness for a friendly `ConflictException` (the unique
  index remains the authoritative, concurrency-safe guard — a same-instant
  race would surface as an unmapped 500, an accepted Phase 1 tradeoff),
  constructs `Member` + `MemberEmployment`, persists both via one
  `IMemberRepository.AddAsync` call (one `SaveChangesAsync`/transaction), and
  logs a `"Member.Create"` audit entry via the existing `IAuditLogger`.
- `IMemberRepository` (`Bimss.Application/Membership/`) — narrow,
  use-case-specific port, not a generic repository (`AGENTS.md` explicitly
  warns against that); the real need is that `Bimss.Application` cannot
  reference `Bimss.Infrastructure`/`BimssDbContext` per the enforced
  layering rule. Implemented by `MemberRepository`
  (`Bimss.Infrastructure/Membership/`).
- DI: new `AddBimssMembership()` (registers `IMemberRepository`) folded into
  `AddBimssInfrastructure`; `MemberCreationService` registered in
  `AddBimssApplication()` — its first real content since BIMSS-010 left it a
  documented no-op.
- Extended `ServiceCollectionCompositionTests` (BIMSS-010) to resolve
  `IMemberDocumentStorage` (BIMSS-021, previously unverified through DI),
  `IMemberRepository`, and `MemberCreationService` — closes a real gap where
  nothing exercised those registrations end to end.
- Tests: `MemberCreationServiceTests` (unit — hand-rolled fakes for
  `IMemberRepository`/`IAuditLogger`/`TimeProvider`, since this repo uses no
  mocking library: success path, duplicate-employee-number conflict with no
  side effects, null-command guard); `MemberRepositoryTests` (integration —
  the real EF-backed repository via `InMemoryBimssDbContextFactory`).
- No controller/UI in this task (BIMSS-029, Phase 1C) — no admin "create
  member" screen exists yet, so who's authorized to call this service is
  deferred to whichever task adds the endpoint (`Permission.Membership.Manage`
  already exists for this).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (211/211 passing) in
  Release, `dotnet format --verify-no-changes`. Architecture tests confirm
  `Bimss.Application` still has zero `Bimss.Infrastructure` dependency
  despite the new port/service. No schema change, no migration.
- Dependencies: BIMSS-015, BIMSS-016, BIMSS-007.

### BIMSS-023 — Member read/query use cases (Done)

Merged via [PR #27](https://github.com/agurokeendavid/bi-buklod-bimss/pull/27).

- `MemberDetail`/`MemberSummary` (`Bimss.Application/Membership/`) —
  projection records for a future detail view and grid (BIMSS-027/028,
  Phase 1C); never the `Member`/`MemberEmployment` EF entities directly
  (`AGENTS.md`'s data access rules).
- `IMemberQueryService` (`Bimss.Application/Membership/`) — `GetByIdAsync`
  (nullable — a controller decides whether "not found" becomes 404) and
  `ListAsync`. Kept separate from `IMemberRepository` (write-focused),
  matching the BIMSS-022/023 split.
- `MemberQueryService` (`Bimss.Infrastructure/Membership/`) — EF-backed.
  `Member` and `MemberEmployment` have no navigation property between them by
  design, so both queries use an explicit LINQ left join (`GroupJoin` +
  `DefaultIfEmpty`) to stay a single SQL statement each rather than querying
  per-member — avoids the N+1 pattern `AGENTS.md` warns against. Employment
  fields are nullable in the projection since the join is optional.
- Registered via the existing `AddBimssMembership()`; extended
  `ServiceCollectionCompositionTests` to resolve `IMemberQueryService`
  through DI.
- Tests: `MemberQueryServiceTests` (integration) — detail with/without an
  employment record, not-found returns null, list returns all members,
  empty list. Confirms the left-join projection actually executes correctly
  under the EF Core InMemory provider, not just that it compiles.
- No controller/UI in this task — no admin list/detail screen exists yet.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (216/216 passing) in
  Release, `dotnet format --verify-no-changes`. No schema change, no
  migration.
- Dependencies: BIMSS-015, BIMSS-016.

### BIMSS-024 — Member status transition service (Done)

Merged via [PR #28](https://github.com/agurokeendavid/bi-buklod-bimss/pull/28).

- `MemberStatusTransitionService` (`Bimss.Application/Membership/`) —
  `VerifyAsync`/`DeactivateAsync`/`ReactivateAsync`: load the member
  (tracked), call the corresponding `Member` domain method, persist, then log
  a `"Member.Verify"`/`"Member.Deactivate"`/`"Member.Reactivate"` audit
  entry — matching `docs/ARCHITECTURE.md`'s explicit "member verification"
  audit requirement. Unknown member id → `NotFoundException`. An invalid
  transition throws `ConflictException` from the domain method itself
  *before* `SaveChangesAsync`/audit logging run, so a rejected transition
  leaves no partial persistence or audit trail. Authorization stays the
  future controller's job (`Permission.Membership.Verify`/`Manage` already
  exist), same convention as `MemberCreationService` (BIMSS-022).
- `IMemberRepository` gained `GetTrackedByIdAsync` (loads a `Member` with
  `StatusHistory` included, for mutation via its own domain methods —
  distinct from `IMemberQueryService`'s untracked projections) and
  `SaveChangesAsync`, implemented in `MemberRepository`.
- Registered in `AddBimssApplication()`; extended
  `ServiceCollectionCompositionTests` to resolve it through DI.
- Tests: `MemberStatusTransitionServiceTests` (unit, hand-rolled fakes) —
  each transition's success + audit-entry content, not-found, and that an
  invalid transition calls neither `SaveChangesAsync` nor the audit logger;
  `MemberStatusTransitionPersistenceTests` (integration) — the real
  EF-backed repository: not-found, and verify → save → reload confirming
  both `Status` and accumulated `StatusHistory` persist correctly.
- No controller/UI in this task — no admin verify/deactivate/reactivate
  screen exists yet (BIMSS-031/032, Phase 1C).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (225/225 passing) in
  Release, `dotnet format --verify-no-changes`. No schema change, no
  migration.
- Dependencies: BIMSS-015.

### BIMSS-025 — Synthetic membership seed data (Done)

Merged via [PR #29](https://github.com/agurokeendavid/bi-buklod-bimss/pull/29).

- `DevelopmentMembershipSeeder` (`Bimss.Infrastructure/Membership/Seeding/`)
  — mirrors `DevelopmentIdentitySeeder`'s idempotent, `IsDevelopment()`-gated
  pattern (BIMSS-013).
- Seeds the seven reference/master data tables with a small synthetic set —
  scope explicitly deferred from BIMSS-014 to this task.
- Seeds three synthetic members **via the existing
  `MemberCreationService`/`MemberStatusTransitionService` Application
  services**, not direct `DbContext` inserts — spanning the full status
  lifecycle (`PendingVerification`, `Active`, `Inactive`). Going through the
  real Application services means seed data exercises the same business
  rules and audit trail as real usage. Idempotency checked via
  `EmployeeNumber`.
- Wired into both hosts' `Program.cs`, right after `DevelopmentIdentitySeeder`.
  Uses only clearly-synthetic data (`DEV-00001`–`DEV-00003`), never real
  Buklod member data.
- Tests: `DevelopmentMembershipSeederTests` — expected reference-data
  counts/codes, correct member statuses, idempotent across repeated calls.
  Confirmed the existing Development-environment `ExceptionHandlingTests`
  test (which now also triggers this seeder) still passes.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (228/228 passing) in
  Release, `dotnet format --verify-no-changes`. No schema change, no
  migration.
- Dependencies: BIMSS-014, BIMSS-022, BIMSS-024.

### BIMSS-026 — Membership schema/constraint integration tests (Done)

Merged via [PR #30](https://github.com/agurokeendavid/bi-buklod-bimss/pull/30).
Last Phase 1B task — Phase 1B is now fully Done.

- `MembershipSchemaConstraintTests` (`Bimss.IntegrationTests/Membership/`) —
  verifies guarantees EF Core InMemory (the convention since BIMSS-011)
  cannot check: real unique-index/FK enforcement and `Database.MigrateAsync()`
  actually applying migrations. Six tests against a real SQL Server:
  migrations apply cleanly to a fresh database; `MemberEmployment.EmployeeNumber`'s
  unique constraint; the one-employment-per-member unique constraint;
  `MemberAddress`'s unique `(MemberId, AddressType)` constraint; `CivilStatus`
  (a Restrict-delete reference row) cannot be deleted while a `Member`
  references it; deleting a `Member` cascades to `MemberStatusHistory` and
  `MemberEmployment`.
- Runs for real only when `BIMSS_TEST_SQLSERVER_CONNECTION_STRING` is set;
  every test no-ops locally where that's normally unset — the only test class
  in the solution with an external dependency, everything else still runs
  standalone.
- **CI**: added a SQL Server service container to `.github/workflows/ci.yml`
  (`mcr.microsoft.com/mssql/server:2022-latest`) plus a "Wait for SQL Server"
  step (GitHub Actions doesn't wait for in-container readiness beyond
  container start, so this polls the mapped port first) and
  `EnableRetryOnFailure()` on the DbContext for the brief window after the
  port opens but before SQL Server accepts logins. This is a **GitHub Actions
  service container**, not Testcontainers — it runs alongside the job via the
  runner's own Docker rather than the test process calling a local Docker
  daemon, so it does not reintroduce the local Docker/WSL2 resource
  exhaustion BIMSS-011 removed Testcontainers for. Verified in this PR's own
  CI run: integration test count went from 56 to 62 and duration from ~2s to
  ~18s, confirming the six new tests actually executed against real SQL
  Server rather than silently no-opping. SA password is a hardcoded,
  CI-only synthetic value discarded with the container at the end of the
  job — never a real credential, same convention as
  `DevelopmentIdentitySeeder`'s `DevPassword`.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (234/234 passing
  locally, the six new tests no-op'd as designed since no local SQL Server
  was available) in Release, `dotnet format --verify-no-changes`. CI run
  confirmed 62/62 integration tests passing against the real database.
- Dependencies: BIMSS-014 through BIMSS-021.

## Phase 1C — Membership Administration (Done)

**Frontend pivot (2026-08-15)**: the user decided to replace the planned
Bootstrap/jQuery/DevExtreme MVC (`Bimss.Web`) admin UI with a decoupled
Next.js + React frontend (shadcn/ui + Tailwind CSS, JWT bearer auth via
`Bimss.Api`) — triggered by a DevExtreme licensing question with no
resolution path at the time. See "Frontend pivot" notes below the table for
the full rationale and scope. This replaces `Bimss.Web`'s UI role entirely
(Phase 1C and the future Phase 1E self-service portal), not just BIMSS-027.
`Bimss.Web` itself is not deleted yet — it stays in the solution, unused,
until BIMSS-027 is proven working on the new stack, then a dedicated
cleanup task removes it. BIMSS-011 ("Base layout, navigation shell,
template cleanup") is **superseded** by BIMSS-047 below — its Bootstrap/Razor
layout work is no longer the frontend's base layout.

Two new prerequisite tasks (IDs assigned after the original Phase
1A–1E numbering was written, listed first since everything else in this
phase depends on them):

| ID | Title | Status | Depends on |
|---|---|---|---|
| BIMSS-046 | JWT authentication backend (`Bimss.Api`) | Done — [PR #31](https://github.com/agurokeendavid/bi-buklod-bimss/pull/31) | BIMSS-005, BIMSS-006 |
| BIMSS-047 | Next.js frontend scaffold (base layout, auth flow, API client) | Done — [PR #32](https://github.com/agurokeendavid/bi-buklod-bimss/pull/32) | BIMSS-046 |
| BIMSS-027 | Membership admin list (data table) | Done — [PR #33](https://github.com/agurokeendavid/bi-buklod-bimss/pull/33) | BIMSS-023, BIMSS-046, BIMSS-047 |
| BIMSS-028 | Member details view | Done — [PR #34](https://github.com/agurokeendavid/bi-buklod-bimss/pull/34) | BIMSS-023, BIMSS-047 |
| BIMSS-029 | Create member (admin UI) | Done — [PR #35](https://github.com/agurokeendavid/bi-buklod-bimss/pull/35) | BIMSS-022, BIMSS-047 |
| BIMSS-030 | Edit permitted information (officer-direct-edit) | Done — [PR #36](https://github.com/agurokeendavid/bi-buklod-bimss/pull/36) | BIMSS-022, BIMSS-047 |
| BIMSS-031 | Activate/Deactivate/status UI | Done — [PR #37](https://github.com/agurokeendavid/bi-buklod-bimss/pull/37) | BIMSS-024, BIMSS-047 |
| BIMSS-032 | Verification workflow UI + audit/history panel | Done — [PR #38](https://github.com/agurokeendavid/bi-buklod-bimss/pull/38) | BIMSS-024, BIMSS-007, BIMSS-047 |

BIMSS-028–032's dependency on BIMSS-011 is dropped (superseded, see above);
BIMSS-047 is their real UI-shell prerequisite now. Each task's detailed
scope is still worked out when it's actually started, same as every prior
task in this backlog — not pre-designed in bulk here.

Phase 1D (BIMSS-033–038) and Phase 1E (BIMSS-039–045)'s UI-facing tasks
(especially BIMSS-038 and all of Phase 1E) will need the same "was
Razor/MVC, now Next.js" scope treatment once they're reached. No
renumbering needed there yet — just a heads-up for whoever picks them up.

### BIMSS-046 — JWT authentication backend (Done)

Merged via [PR #31](https://github.com/agurokeendavid/bi-buklod-bimss/pull/31).
First task of the frontend pivot.

- `AuthController` (`Bimss.Api`) — `POST /api/auth/login`, `/refresh`,
  `/logout`. Login uses `SignInManager.CheckPasswordSignInAsync` (not
  `PasswordSignInAsync`, cookie-oriented) so no cookie is issued; failures
  return a generic "Invalid username or password." (no username
  enumeration), matching `AccountController`'s existing convention.
- Access tokens: short-lived JWTs (15 min, HMAC-SHA256), returned in the
  response body. Refresh tokens: opaque, hashed (SHA-256) at rest —
  never the raw value — returned as an `httpOnly`/`Secure`/`SameSite=None`
  cookie the frontend never touches directly; every refresh **rotates**
  (the presented token is revoked immediately, reuse rejected).
- `PermissionClaimsTransformation` needed **zero changes** — confirmed by
  reading it directly, it's scheme-agnostic and already re-derives
  permission claims from `RolePermissions` on every request via
  `ClaimTypes.NameIdentifier`, which the JWT's `sub` claim auto-maps to.
  Every existing `[Authorize(Policy = Permission.X)]` action keeps working
  unchanged.
- `AddBimssJwtAuthentication` (`Bimss.Infrastructure/Identity/`) registers
  JWT Bearer as `Bimss.Api`'s default scheme only; `Bimss.Web` keeps its
  cookie scheme untouched (it isn't deleted yet).
- **Real bug found and fixed via the new tests**: `JwtBearerOptions` must
  resolve the signing key lazily through `IOptions<JwtOptions>`, not by
  reading `IConfiguration` eagerly inside `AddBimssJwtAuthentication` —
  `WebApplicationFactory`-based tests layer config overrides onto the host
  *after* `Program.cs`'s synchronous top-level statements (including that
  registration call) already ran, so an eagerly-captured value missed them
  entirely (manifested as every JWT-authenticated request 500ing with
  `SymmetricSecurityKey`'s "key.Length == 0" until fixed). Diagnosed by
  temporarily switching a test to the `Development` environment to expose
  the real exception detail (BIMSS-008), then reverted once found.
- Migration: `AddRefreshTokens` — one table, FK to `AspNetUsers` (cascade —
  deliberately different from `AuditEvent`'s "no FK to AspNetUsers"
  convention: a refresh token is meaningless without its user and has no
  audit purpose to outlive it), unique index on the token hash.
- CORS added to `Bimss.Api` for the future Next.js origin
  (`Cors:AllowedOrigins` config, credentialed for the refresh cookie flow).
- Tests: `JwtTokenServiceTests` (unit — issuance, hashed storage, rotation,
  reuse-rejection, revocation); `RefreshTokenConfigurationTests` (unit);
  `AuthControllerTests` (integration, `WebApplicationFactory` + InMemory
  DB — login success/failure, refresh/rotation/reuse-rejection, logout,
  and a protected endpoint correctly authenticating a valid bearer token
  while still enforcing its permission policy).
- Bundled the full frontend-pivot documentation update into this PR — see
  `AGENTS.md` ("Frontend rules"), `docs/ARCHITECTURE.md`,
  `docs/SECURITY_AND_PRIVACY.md` ("Authentication and token handling",
  replacing the old CSRF section), `README.md`,
  `.github/copilot-instructions.md`, and a full rewrite of
  `.github/instructions/frontend.instructions.md`.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (250/250 passing) in
  Release, `dotnet format --verify-no-changes`. Migration diff reviewed.
- Dependencies: BIMSS-005, BIMSS-006.

### BIMSS-047 — Next.js frontend scaffold (Done)

Merged via [PR #32](https://github.com/agurokeendavid/bi-buklod-bimss/pull/32).
Direct replacement for BIMSS-011's now-superseded Bootstrap/Razor layout work.

- New top-level `frontend/` app: Next.js (App Router, TypeScript) + Tailwind
  CSS + shadcn/ui.
- **No CDN font dependency** — `next/font/google` was removed in favor of a
  system font stack. Discovered because this sandbox couldn't reach
  `fonts.gstatic.com` (a real network restriction, not a bug), but kept
  deliberately: a government intranet deployment shouldn't depend on
  Google's CDN being reachable at build/runtime either.
- `AuthProvider` (`lib/auth-context.tsx`) — access token in memory only
  (never `localStorage`/`sessionStorage`); silent session restore on mount
  via `POST /api/auth/refresh` (`credentials: "include"` carries the
  httpOnly cookie); `fetchWithAuth` attaches the bearer header and retries
  once after a refresh on 401.
- `/login` and `/dashboard` (protected layout + nav shell + sign-out): the
  dashboard's placeholder page calls a real protected `Bimss.Api` endpoint
  through `fetchWithAuth` to prove the bearer-token flow end to end —
  BIMSS-027 replaces this placeholder with real Membership screens.
- CI: new `frontend` job in `ci.yml` (`npm ci`, lint, build), separate from
  the .NET job.
- `docs/REPOSITORY_SETUP.md` gained a "Frontend setup (Next.js)" section —
  local dev must use `Bimss.Api`'s **https** launch profile (the refresh
  cookie's `Secure` attribute requires it).
- **Verified live**, not just build/lint: ran both sides against the real
  local SQL Server (migrations applied, `DevelopmentIdentitySeeder` dev
  accounts) and drove an actual browser via Chrome automation — login
  succeeds, a protected API call through `fetchWithAuth` returns 200, a
  full page reload stays authenticated (silent refresh works), sign-out
  clears the session and redirects, and visiting `/dashboard` while logged
  out redirects to `/login`.
- `dotnet build`/`dotnet test` unaffected (250/250 passing) — this task
  touches no .NET code. `npm run lint`/`npm run build` both clean.
- Dependencies: BIMSS-046.

### BIMSS-027 — Membership admin list (data table) (Done)

Merged via [PR #33](https://github.com/agurokeendavid/bi-buklod-bimss/pull/33).
First real Membership screen on the new stack.

- `MembersController` (`Bimss.Api`) — `GET /api/members`, gated on
  `Permission.Membership.Manage`, returns `MemberSummaryResponse[]`
  (`Bimss.Contracts/Membership/`) mapped from the existing
  `IMemberQueryService.ListAsync()` projection (BIMSS-023). `Status` is a
  plain `string` (`member.Status.ToString()`), not a reference to
  `Bimss.Domain.Membership.MemberStatus` — `Bimss.Contracts` has zero
  project references by design, and this keeps the wire contract decoupled
  from Domain internals.
- `/dashboard/members` (frontend) — a TanStack Table + shadcn/ui data table
  (`MembersTable`) with a status `Badge` color-coded per status.
  `/dashboard` now redirects to `/dashboard/members`; `NavHeader` gained a
  "Members" link, the first entry in what grows into a real admin nav as
  Phase 1C adds more screens.
- **Version decision**: `@tanstack/react-table` had just released a v9
  major by default via `npm install` — a genuine API rewrite
  (`useTable`/feature-composition model instead of v8's
  `useReactTable`/`getCoreRowModel()`) with no reliable training-data
  grounding. Since this is the first use of the library and sets the
  pattern every future admin grid in Phase 1C/1D/1E follows, pinned to
  `^8` (stable, well-documented) instead of guessing at v9.
- Tests: `MembersControllerTests` (integration, `WebApplicationFactory` +
  InMemory DB + `TestAuthHandler`, mirroring `DiagnosticsAuthorizationTests`)
  — 401 unauthenticated, 403 without the permission, 200 with real seeded
  data (including the enum-to-string mapping), empty array when no members
  exist.
- **Verified live**: ran the real `Bimss.Api` + local SQL Server + dev seed
  data, logged in via the browser as `admin.dev`, confirmed all three
  seeded members render with correct names, employee numbers, and
  color-coded status badges.
- Verified: `dotnet build`/`dotnet test` (254/254 passing), `dotnet format
  --verify-no-changes`, `npm run lint`/`npm run build` clean.
- Dependencies: BIMSS-023, BIMSS-046, BIMSS-047.

### BIMSS-028 — Member details view (Done)

Merged via [PR #34](https://github.com/agurokeendavid/bi-buklod-bimss/pull/34).

- `MembersController` gained `GET /api/members/{id}`, gated on
  `Permission.Membership.Manage`, returns `MemberDetailResponse`
  (`Bimss.Contracts/Membership/`) mapped from
  `IMemberQueryService.GetByIdAsync()` (BIMSS-023); 404 when the member
  doesn't exist.
- `/dashboard/members/[id]` (frontend) — core identity + employment fields
  in a `Card` grid, status `Badge`, "Back to members" link. The members
  list's last-name cell now links to each row's detail page.
- Tests: `MembersControllerTests` gained `GetById` coverage — 401, 403, 404,
  200 with real seeded data matching the detail response shape.
- **Verified live**: clicked from the members list into a member's detail
  page against the real API + SQL Server + dev seed data, confirmed every
  field rendered correctly, the back link worked, and a nonexistent id
  showed "Member not found." instead of crashing.
- Two things hit and resolved during verification, neither a defect in the
  final code: a stale leftover dev-server process from BIMSS-027's session
  had a crashed Turbopack worker pool (fixed by killing it and starting
  clean — unrelated to this PR); an attempted `<Button asChild>` pattern
  (Radix-style) doesn't work with this project's Base-UI-based `Button` —
  used `buttonVariants()` + `cn()` on the `Link` directly instead. Worth
  remembering for any future `asChild`-style composition in this frontend.
- Verified: `dotnet build`/`dotnet test` (258/258 passing), `dotnet format
  --verify-no-changes`, `npm run lint`/`npm run build` clean.
- Dependencies: BIMSS-023, BIMSS-047.

### BIMSS-029 — Create member (admin UI) (Done)

Merged via [PR #35](https://github.com/agurokeendavid/bi-buklod-bimss/pull/35).

- New `IReferenceDataQueryService`/`ReferenceDataQueryService`
  (`Bimss.Application`/`Bimss.Infrastructure`) — Phase 1B (BIMSS-014) never
  built a query layer over the 7 reference/master-data tables, they were
  schema-only. Deliberately scoped to only the 3 types this form needs
  (CivilStatus, Suffix, OfficeUnit), not all 7 — the other 4 get their own
  query methods when a task actually needs them.
- `ReferenceDataController` (`GET /api/reference-data/{civil-statuses,
  suffixes, office-units}`) and `MembersController.Create` (`POST
  /api/members`), both gated on `Permission.Membership.Manage`. `Create`
  maps `CreateMemberRequest` (`Bimss.Contracts`, DataAnnotations-validated)
  to the existing `CreateMemberCommand`/`MemberCreationService`
  (BIMSS-022) — no new domain logic, just the first caller of a service
  that had been sitting unused behind no endpoint since BIMSS-022 shipped.
- `/dashboard/members/new` (frontend) — full create-member form
  (shadcn `Select`/`Textarea`), loads reference data in parallel on mount,
  redirects to the new member's detail page on success, surfaces a
  specific message on a 409 (duplicate employee number). Required fields
  are now visually marked (`*`) with a legend note, matching server-side
  `[Required]` validation — added after live verification showed no way
  to tell which fields were mandatory before submitting.
- **Bug found and fixed during verification**: Base UI's
  `Select`/`SelectValue` resolves a selected value's label by looking it
  up in the popup's registered items, but items only register once the
  popup has actually opened — so any `Select` with a non-empty default
  value (Suffix defaults to a "None" sentinel) rendered the raw stored
  value (`__none__`, or a GUID once an option was picked) instead of its
  label until the dropdown was opened once. Fixed by giving `SelectValue`
  an explicit children render function that resolves the label from the
  already-loaded reference-data array, on all three selects on this form.
  Worth remembering for any future `Select` with a pre-set default value
  in this frontend.
- Tests: `ReferenceDataQueryServiceTests` (unit, EF InMemory),
  `ReferenceDataControllerTests` (integration — 401/403/200 per reference
  type), `MembersControllerTests` gained `Create_*` coverage (401, 403,
  400 missing required field, 201 created and persisted, 409 duplicate
  employee number).
- **Verified live**: ran the real `Bimss.Api` + local SQL Server + dev
  seed data, logged in as `admin.dev`, filled out and submitted the
  create-member form (including picking real Civil status/Office unit/
  Suffix values), confirmed the member was created and rendered correctly
  on both the detail page and the members list.
- Verified: `dotnet build`/`dotnet test` (271/271 passing), `dotnet format
  --verify-no-changes`, `npm run lint`/`npm run build` clean.
- Dependencies: BIMSS-022, BIMSS-047.

### BIMSS-030 — Edit permitted information (officer-direct-edit) (Done)

Merged via [PR #36](https://github.com/agurokeendavid/bi-buklod-bimss/pull/36).

- Officer-direct-edit means an authorized officer edits directly, no
  approval step — the officer review IS the trust boundary, unlike the
  future member-initiated self-service update-request workflow
  (docs/DOMAIN_WORKFLOWS.md #2, Phase 1E's BIMSS-042/044) where officer
  review is a separate approve/reject step over a member's own proposed
  change. BI Employee Number stays immutable — it's a business identifier
  (AGENTS.md) and this task's request contract has no field that could
  carry a new value for it at all.
- `Member.UpdateProfile` (`Bimss.Domain`) — new mutation method, reuses
  the constructor's own validation; no status/history side effects since
  a profile edit isn't a status transition.
- `MemberEmployment.UpdateDetails` — already existed (added ahead of this
  task during BIMSS-022, unused until now); reused as-is.
- New `IMemberRepository.GetTrackedEmploymentByMemberIdAsync` port method
  — `MemberEmployment` is its own table/row, not a navigation property on
  `Member`, so editing it needs its own tracked load alongside
  `GetTrackedByIdAsync`.
- `MemberProfileUpdateService` (`Bimss.Application`) and `PUT
  /api/members/{id}` (`Bimss.Api`, gated on `Permission.Membership.Manage`)
  — mirrors `MemberCreationService`/`MemberStatusTransitionService`'s
  existing conventions (load tracked, mutate via the aggregate's own
  method, save, log to the audit trail as `"Member.UpdateProfile"`).
- `/dashboard/members/[id]/edit` (frontend) — pre-fills from the existing
  member and reference data, shows BI employee number as a disabled
  input, reuses BIMSS-029's `SelectValue` label-resolution fix. Member
  detail page gained an "Edit" button.
- Tests: `MemberTests.UpdateProfile_*` (domain), new
  `MemberProfileUpdateServiceTests` (application — success + audit log,
  404 when member missing, 404 when employment missing),
  `MembersControllerTests` gained `Update_*` coverage (401, 403, 404, 400
  missing required field, 200 updated-and-persisted, confirms
  EmployeeNumber is unchanged after an update).
- **Verified live** (by the user): opened a member, clicked Edit,
  confirmed the form pre-filled correctly including resolved Select
  labels, changed a field, saved, confirmed the redirect to the detail
  page showed the update and it persisted on refresh. Caught one
  non-code issue: the locally running `Bimss.Api` process predated this
  branch's new `PUT` route and needed a restart to pick it up (405 until
  then) — not a defect in the code.
- Verified: `dotnet build`/`dotnet test` (285/285 passing), `dotnet format
  --verify-no-changes`, `npm run lint`/`npm run build` clean.
- Dependencies: BIMSS-022, BIMSS-047.

### BIMSS-031 — Activate/Deactivate/status UI (Done)

Merged via [PR #37](https://github.com/agurokeendavid/bi-buklod-bimss/pull/37).

- `MemberStatusTransitionService` (`Bimss.Application`) already existed —
  built during BIMSS-015 with `VerifyAsync`/`DeactivateAsync`/
  `ReactivateAsync` fully implemented and unit-tested, but no controller
  called it until this task.
- `POST /api/members/{id}/verify` (`Permission.Membership.Verify`),
  `/deactivate` and `/reactivate` (`Permission.Membership.Manage`,
  `Deactivate` requires a `MemberStatusReason` id) — all three take
  optional `Remarks` and return the updated `MemberDetailResponse`.
- **Authorization bug caught and fixed before merge**: `MembersController`
  had a class-level `[Authorize(Policy = Permission.Membership.Manage)]`.
  ASP.NET Core combines a class-level `[Authorize]` with a method-level
  one using AND, not override — leaving that in place while adding
  `[Authorize(Policy = Permission.Membership.Verify)]` on `Verify` would
  have silently required *both* permissions to verify a member, not just
  `Verify`. The seeded `MembershipOfficer` dev role happens to hold both
  permissions together, which would have hidden the bug in this
  environment specifically. Fixed by removing the class-level attribute
  and stating each action's own policy explicitly. Caught by a test
  (`Verify_ReturnsForbidden_WithoutTheVerifyPermission`, using only
  `Manage`) written specifically to pin this down, not by manual testing.
- `GET /api/reference-data/member-status-reasons` (new
  `IReferenceDataQueryService` method, same pattern as BIMSS-029's other
  reference-data endpoints) backs the Deactivate reason picker.
- Member detail page — status-conditional action buttons (only the one
  valid for the member's current status shows). Clicking one reveals an
  inline form (reason picker for Deactivate, optional remarks for all
  three) rather than a modal dialog — no `Dialog` component exists in
  this project yet and this didn't need one. A 409 (stale/invalid
  transition) surfaces as an inline message asking the user to reload; a
  403 as a permission message.
- Tests: `ReferenceDataQueryServiceTests`/`ReferenceDataControllerTests`
  gained `ListMemberStatusReasons` coverage; `MembersControllerTests`
  gained `Verify_*`/`Deactivate_*`/`Reactivate_*` (401/403/404/409/200 per
  action).
- **Verified live** (by the user): verified a pending member (→ Active),
  deactivated an active member with a reason (→ Inactive), reactivated it
  (→ Active), confirmed only the status-appropriate button shows at each
  stage.
- Verified: `dotnet build`/`dotnet test` (300/300 passing), `dotnet format
  --verify-no-changes`, `npm run lint`/`npm run build` clean.
- Dependencies: BIMSS-024, BIMSS-047.

### BIMSS-032 — Verification workflow UI + audit/history panel (Done)

Merged via [PR #38](https://github.com/agurokeendavid/bi-buklod-bimss/pull/38).
Closes the last two Phase 1C gaps: `docs/DATA_DICTIONARY.md`'s confirmed
"proof of employment mandatory before verification" decision was never
enforced anywhere, and `MemberStatusHistory` (populated since BIMSS-015)
was never queried back out.

- **Scope decision**: "audit/history panel" means a member-scoped history
  (status transitions for this member), not a system-wide audit log
  browser — `IAuditLogger` is write-only with no read capability
  anywhere, and no Auditor-role UI exists or is planned this phase. A
  general audit browser is a separate, bigger future task if Buklod asks
  for it.
- `MemberDocumentUploadService`/`IMemberDocumentQueryService`
  (`Bimss.Application`/`Bimss.Infrastructure`) are the first real callers
  of `IMemberDocumentStorage`/`MemberDocument` (BIMSS-021) — no
  controller had ever wired them up. New `MemberDocumentsController`
  (`api/members/{id}/documents`): `POST` (multipart upload, 10 MB
  `[RequestSizeLimit]` — no size limit was documented anywhere in this
  repo before now), `GET` (list), `GET /{documentId}/download` (streams
  via `OpenReadAsync`). Extension allowlist enforced at the API boundary
  in addition to `MemberDocument`'s own content-type check.
- **`MemberStatusTransitionService.VerifyAsync`** now requires at least
  one uploaded document, throwing `ConflictException` otherwise. Checks
  for *any* document rather than a specific `DocumentType` match —
  `DocumentType` is deliberately free text with no reference table
  (BIMSS-021), so the officer reviewing the document list before
  verifying is the real safeguard for type correctness, not a fragile
  string comparison. Confirmed with the user before implementing.
- `GET /api/members/{id}/status-history` (new
  `IMemberQueryService.ListStatusHistoryAsync`) backs a new Status
  history panel on the member detail page; a new Documents panel handles
  list/upload/authenticated download (via `fetchWithAuth` → blob → a
  temporary `<a download>`, since bearer-token auth can't be attached to
  a plain `<a href>`).
- **Fixed a latent UX gap while here**: the status-action error handler
  previously hardcoded a generic message on any failure. Since
  `BimssExceptionHandler` already puts the real exception message in
  `ProblemDetails.Detail`, it now parses and shows that — so the new
  verification-gate message actually reaches the user instead of a
  generic "status has already changed" string.
- `DevelopmentMembershipSeeder` uploads a synthetic document for each
  `Active`/`Inactive` seed member before verifying them (the seeder now
  goes through the same gate); the `PendingVerification` seed member
  deliberately stays without one so the gate is visible out of the box.
- Tests: `MemberDocumentUploadServiceTests`, `MemberDocumentQueryServiceTests`,
  `MemberDocumentsControllerTests` (integration — real
  `LocalFileMemberDocumentStorage` against a temp directory: upload →
  list → download round-trip, rejected extension, 404 on missing
  document), extended `MemberStatusTransitionServiceTests`/
  `MembersControllerTests` for the gate and the status-history endpoint,
  extended `DevelopmentMembershipSeederTests`.
- **Verified live** (by the user): created a fresh member, confirmed
  Verify was blocked with the new document-required message, uploaded a
  PDF, downloaded it back successfully, verified successfully afterward,
  confirmed the status-history panel showed the transition.
- Verified: `dotnet build`/`dotnet test` (316/316 passing), `dotnet format
  --verify-no-changes`, `npm run lint`/`npm run build` clean.
- Dependencies: BIMSS-024, BIMSS-007, BIMSS-047.

### Frontend design pass (2026-08-16, no BIMSS ID — cross-cutting UI polish, not a new feature)

Merged via [PR #39](https://github.com/agurokeendavid/bi-buklod-bimss/pull/39).
After BIMSS-027–032 shipped functionally complete but visually plain (default
shadcn grayscale theme, one-link top bar), the user asked for a full design
pass across everything Phase 1C had built, using
[NextAdminHQ/nextjs-admin-dashboard](https://github.com/NextAdminHQ/nextjs-admin-dashboard)
(MIT-style license) as real visual/structural reference — fetched via `gh api`,
not guessed. Kept shadcn/ui + Base UI as the component library throughout
(per `AGENTS.md`'s Frontend rules) rather than adopting NextAdmin's own
component set, which would have meant re-migrating every existing screen.

- Blue accent theme (light + dark tokens), a sidebar + header shell
  (`AppSidebar`/`AppHeader`) replacing the old single top bar, dark mode via
  `next-themes` (already a scaffold dependency, wired up for the first
  time), and larger base sizing app-wide (18px root font-size, taller
  inputs/buttons/selects) — an explicit accessibility call since Buklod's
  membership skews senior/non-technical, prioritizing legibility over
  density.
- `/dashboard` became a real overview (live stat cards from `/api/members`,
  no fabricated trend data, deep-linking into a pre-filtered members view)
  instead of an immediate redirect to the members list.
- UX audit implementation: members-table sort/search/status-filter/
  pagination, inline per-field form validation (parsed from the server's
  real `ValidationProblemDetails`, via new `lib/api-errors.ts`), a
  `beforeunload` unsaved-changes warning on create/edit forms, a
  session-expiry toast, breadcrumbs, tooltips on icon-only buttons, a
  visually-distinguished Deactivate button, and a styled file input.
- **Two real bugs found and fixed during live verification**, not just
  cosmetic misses: an infinite render loop that hung the browser tab
  (`members-table.tsx`'s `columnFilters` was built as a fresh array
  literal inline in TanStack Table's controlled `state` on every render —
  fixed with `useMemo`), and the dashboard stat-card filter silently not
  applying when already on the members page (`useState`'s initial value
  only reads once; Next.js reuses the page's component instance across a
  search-param-only navigation — fixed by keying the table on the URL's
  status param). Both patterns, and the Base UI `render`-prop composition
  convention this pass reinforced, are now documented in
  `.github/instructions/frontend.instructions.md` for future work.
- No backend changes, no new BIMSS task — this revisits/polishes
  BIMSS-027–032's existing screens rather than adding scope. Phase 1D/1E's
  own UI work should follow the conventions this pass established rather
  than reverting to shadcn defaults.
- Verified: `npm run lint`/`npm run build` clean throughout; live manual
  verification by the user, including reproducing and confirming the fix
  for the render-loop hang.

### Design system integration (2026-08-16, no BIMSS ID — cross-cutting, not a new feature)

Merged via [PR #40](https://github.com/agurokeendavid/bi-buklod-bimss/pull/40).
The user brought a separate Claude-designed UI handoff (nine screens, high
fidelity) for BIMSS. Moved its docs into `docs/design/` (`BIMSS-UI-SPEC.md`,
`README.md`, the `BIMSS.dc.html` prototype) and integrated it into
`frontend/` on top of the PR #39 design pass above, in the same
cross-cutting spirit — no backend changes.

- **Tokens** (`globals.css`): root font-size 16px (was 18px — the design
  spec's whole type scale assumes 16px; if accessibility testing later
  calls for larger text, prefer a user-toggled preference over reverting
  the global override), `--app-bg`/`--primary-subtle` added, and the
  sidebar tokens replaced with a fixed navy rail (`#0b3b6f`) independent of
  light/dark mode — `--primary` (blue-600/500) is unchanged, the navy is
  sidebar-only per the handoff's own "don't repaint the app navy" rule.
- **Shell**: `nav-items.ts` gained a `group` discriminator
  (`operations`/`administration`); `AppSidebar` renders grouped, navy-styled
  nav (brand roundel, uppercase tracked group labels, `--sidebar-accent`
  active state); `AppHeader` restyled to the spec's 56px sticky bar.
  Deliberately did **not** add nav entries or header controls (search box,
  "Alerts · n", "New application") for screens/features that don't exist
  yet — a link or button with nothing real behind it is worse than not
  showing it.
- **Badges**: `lib/member-status.ts`'s color map now uses the spec's exact
  hex values, plus `dark:` variants (the spec doesn't cover dark mode, but
  dark mode already ships in this app, so badges still need to be legible
  in it).
- **Re-skinned** (not just tokens) the five already-built screens: login
  (two-column navy/white layout), dashboard (tile anatomy), the members
  register (filter pills, avatar-with-initials cell, a real "Verify
  selected" bulk action reusing the existing per-member verify endpoint,
  and a real client-side "Export CSV" — "Assign officer"/"Print IDs" from
  the mockup were left out, not stubbed, since neither is a real feature),
  and the member record page (`Tabs`: Personal/Documents/Audit trail,
  wiring in the two existing panels).
- **Deliberately not built**: the mockup's dashboard charts/pending-queue/
  activity-feed/membership-by-office panels, the register's Contributions
  YTD/Last posted/Office columns, member-record Contributions/Loans/Benefit
  claims tabs, and a 5-step application wizard — all need loan/contribution/
  audit-feed data or endpoints that don't exist yet (Phase 3/4/5/7). Built
  the correct look-and-feel shell now; real wiring lands with each
  capability's own phase, exactly as the handoff's own phase tags intend.
  Full list of what's deferred: `docs/design/README.md`'s "Integration
  status" section.
- `.github/instructions/frontend.instructions.md` and `AGENTS.md`'s
  Frontend rules updated to point at `docs/design/BIMSS-UI-SPEC.md` as the
  visual-design source of truth and to correct the stale 18px-root note.
- Verified: `npm run lint`/`npm run build` clean.

### Claude Design update applied — brand assets, wizard, typography (2026-08-18, no BIMSS ID — cross-cutting, not a new feature)

Follow-up to the 2026-08-16 design-system integration above, applying a
newer round of the same Claude Design handoff (updated `docs/design/`)
to the same five already-built screens — still no backend changes, still
scoped to "re-skin what's built + shared components for future modules,"
not new screens for modules without a backend (Beneficiaries/Loans/
Elections/Notifications stay mockup-only, tracked in
`docs/PHASE2_BACKLOG.md`–`PHASE6_BACKLOG.md`).

- **Real brand assets**: `frontend/public/bi-seal.png` and
  `immigration-bg.jpg` (first image assets in the app) replace the
  placeholder "BI" text-circle on both the sidebar and the login page;
  the login page's left panel now layers the building photo under the
  existing navy gradient overlay.
- **Self-hosted Inter** (`@fontsource-variable/inter`, not
  `next/font/google`) replaces the system-font stack as the primary
  `--font-sans` face — still zero runtime network dependency (font files
  ship in the build), so the 2026-08-16 offline/intranet reasoning holds;
  only the specific font changed, not the constraint. System fonts remain
  the fallback chain.
- **New shared wizard shell** (`components/forms/wizard.tsx` —
  `WizardHeader`/`WizardStepBody`): the new-member form became an actual
  navigable 2-step wizard (Personal information → Employment information,
  with Back/Continue/"Save as draft"/Cancel), and the edit-member page was
  rebuilt to mirror it exactly instead of a single flat form. This is the
  intended pattern for future multi-step flows (Loans apply, Elections
  setup) — build on it rather than a bespoke stepper.
- **Header chrome added** matching the spec: a search input and
  notification bell are now visually present but intentionally inert (no
  cross-entity search or notifications backend yet, per the 2026-08-16
  "don't show a link with nothing behind it" rule — the difference here is
  the user explicitly asked for the chrome to be visible ahead of its
  backend, unlike that rule's default). "New member" is a real, functional
  link.
- **Typography standardized at the primitive level.** Every shared
  `components/ui/*` component (`input`, `select`, `textarea`, `table`,
  `badge`, `card`, `label`, `button`, `tabs`, `alert`, `avatar`,
  `dropdown-menu`) had its default text size corrected to
  `docs/design/BIMSS-UI-SPEC.md`'s type scale — they'd been rendering at
  Tailwind's stock defaults (16px inputs/labels, 14px body/badges) since
  the 2026-08-16 pass only fixed the root font-size, not each primitive's
  own `text-*` class. `Label` was the largest gap (16px everywhere, should
  be 13px). Fixing the primitive defaults means every future screen built
  from these components inherits the correct scale automatically.
- **Sidebar logo enlarged** to fill the rail width (170px) with margin,
  restructured into a vertical logo → "BIMSS" → org-name stack.
- Verified: `npm run lint`/`npm run build` clean; live manual verification
  by the user across the members register, create/edit member wizards, and
  a member detail page.

## Phase 1D — Existing Member Import (Done)

**Rescoped for the frontend pivot (2026-08-16)**: only BIMSS-038 (Import
batch admin UI) has a frontend component — BIMSS-033–037 are pure
backend (schema/ingestion/validation/dedup/promotion) and are unaffected.
BIMSS-038 now depends on BIMSS-047 (Next.js scaffold) instead of any
Razor/MVC prerequisite, and will be a Next.js admin screen under
`frontend/`, following the same pattern as the Phase 1C membership admin
screens (server-side authoritative validation, `fetchWithAuth`, shadcn/
ui data table for reviewing staged rows). Detailed scope is still worked
out when the task is actually started, same as every other task.

| ID | Title | Status | Depends on |
|---|---|---|---|
| BIMSS-033 | ImportBatch/MemberImportStaging/ImportValidationError schema | Done — [PR #42](https://github.com/agurokeendavid/bi-buklod-bimss/pull/42) | BIMSS-004 |
| BIMSS-034 | Excel ingestion service | Done — [PR #43](https://github.com/agurokeendavid/bi-buklod-bimss/pull/43) | BIMSS-033 |
| BIMSS-035 | Staging validation rules | Done — [PR #44](https://github.com/agurokeendavid/bi-buklod-bimss/pull/44) | BIMSS-034 |
| BIMSS-036 | Duplicate detection | Done — [PR #45](https://github.com/agurokeendavid/bi-buklod-bimss/pull/45) | BIMSS-035 |
| BIMSS-037 | Promote staging → domain entities | Done — [PR #46](https://github.com/agurokeendavid/bi-buklod-bimss/pull/46) | BIMSS-022, BIMSS-036 |
| BIMSS-038 | Import batch admin UI | Done — [PR #47](https://github.com/agurokeendavid/bi-buklod-bimss/pull/47) | BIMSS-033–037, BIMSS-047 |

### BIMSS-033 — ImportBatch/MemberImportStaging/ImportValidationError schema (Done)

Merged via [PR #42](https://github.com/agurokeendavid/bi-buklod-bimss/pull/42).

- `ImportBatch` (`Bimss.Domain/Membership/`) — one row per import run
  (docs/DOMAIN_WORKFLOWS.md's "Existing member migration / update"
  workflow). `Created -> Staged -> Validated -> Promoted` lifecycle, plus
  `Cancel` from any non-terminal state, each guarded with
  `ConflictException` on an invalid transition — same pattern as `Member`'s
  status machine. No FK from `UploadedByUserId` to `AspNetUsers` (indexed
  only), matching `MemberStatusHistory.ActorUserId`'s established
  reasoning. Does not hold an in-memory collection of its staging rows — a
  batch can be thousands of rows, so those are queried by `ImportBatchId`
  through a future query service instead (avoids N+1 / loading huge
  aggregates, per `AGENTS.md`).
- `MemberImportStaging` (`Bimss.Domain/Membership/`) — one row per source
  spreadsheet row, holding the ~31 raw values from
  `docs/DATA_DICTIONARY.md`'s Excel field mapping as unvalidated nullable
  strings (nvarchar(max), deliberately unconstrained — truncating raw
  migration data would be silent data loss). Repeating groups (children;
  beneficiaries 1-N) are captured as a single raw value each
  (`ChildrenRaw`, `BeneficiariesRaw`) rather than numbered columns, per the
  data dictionary's own warning against that pattern — the exact
  split/delimiter rule is still an open question deferred to
  BIMSS-034/036. Raw values are bundled into a `MemberImportStagingFields`
  record so the constructor doesn't take 30+ positional parameters.
  `RecordValidation`/`RecordMatch`/`MarkPromoted` enforce the pipeline's
  own invariants (can't promote an unvalidated or already-promoted row)
  without deciding *how* a row is validated or matched — those algorithms
  are BIMSS-035/036/037's job.
- `ImportValidationError` (`Bimss.Domain/Membership/`) — immutable
  row/field-level or batch-level validation finding, same
  create-once-no-update reasoning as `MemberDocument`.
- Configurations (`Bimss.Infrastructure/Membership/`): `MemberImportStaging`
  has a unique `(ImportBatchId, RowNumber)` index (no two rows in a batch
  share a source row number) and a unique filtered index on
  `PromotedMemberId` (a member is promoted from at most one row); both FKs
  to `Member` (`MatchedMemberId`, `PromotedMemberId`) are `Restrict`.
  `ImportBatch`/`MemberImportStaging`/`ImportValidationError` cascade
  through their parent (batch -> staging rows -> validation errors).
- Migration: `AddImportStagingSchema` — three tables
  (`ImportBatches`, `MemberImportStaging`, `ImportValidationErrors`).
- Tests: `ImportBatchTests`/`MemberImportStagingTests`/
  `ImportValidationErrorTests` (unit — constructor guards, transition
  success/failure paths); `ImportBatchConfigurationTests`/
  `MemberImportStagingConfigurationTests`/
  `ImportValidationErrorConfigurationTests` (unit, `DbContext.Model`
  metadata-inspection style); `ImportStagingPersistenceTests` (integration
  — round-trips and multi-reload transition persistence, InMemory
  provider); `MembershipSchemaConstraintTests` gained three real-SQL-Server
  cases (row-number uniqueness, promoted-member uniqueness, batch-delete
  cascade) and its migration smoke-check now points at
  `AddImportStagingSchema`.
- Scope is schema/domain-rule only, per the task title — no Application
  use case, ingestion service, or admin UI (BIMSS-034 onward).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (364/364 passing)
  in Release, `dotnet format --verify-no-changes`. Migration diff reviewed
  for sanity (three tables, expected FKs/indexes, nothing else touched).
- Dependencies: BIMSS-004.

### BIMSS-034 — Excel ingestion service (Done)

Merged via [PR #43](https://github.com/agurokeendavid/bi-buklod-bimss/pull/43).

- Implements docs/DOMAIN_WORKFLOWS.md's first migration step only: "Create
  Import Batch -> Load spreadsheet rows to staging." No row validation,
  member matching, or promotion — those stay BIMSS-035/036/037.
- `IExcelWorkbookReader` (`Bimss.Application/Membership/`) — a narrow port
  ("read this file as rows of named columns") so the ingestion service
  doesn't depend on a concrete Excel library, same reasoning as
  `IMemberDocumentStorage`. `ClosedXmlWorkbookReader`
  (`Bimss.Infrastructure/Membership/`) implements it with ClosedXML (MIT
  license, .xlsx-only — sufficient since the source is a Google Forms
  export, no legacy .xls/CSV support needed).
  `IImportBatchRepository`/`ImportBatchRepository` follow the same
  narrow-port pattern as `IMemberRepository`, with just the one method this
  task needs (`AddBatchWithRowsAsync`) — expected to grow incrementally in
  BIMSS-035/036/037 the same way `IMemberRepository` did across BIMSS-022/
  030/032.
- `ImportBatchIngestionService.IngestAsync` (`Bimss.Application/Membership/`)
  reads the workbook, maps each row's named columns to a
  `MemberImportStagingFields` (matching docs/DATA_DICTIONARY.md's Excel
  field mapping table's "Source field" column verbatim as expected header
  text), creates one `MemberImportStaging` per row plus the owning
  `ImportBatch`, calls `ImportBatch.MarkStaged`, persists both in one
  repository call, and records an `ImportBatch.Ingest` audit entry. Any
  exception from the reader (unparseable file) is translated into a
  `DomainValidationException` with a `File` field error rather than
  surfacing the underlying library's exception type.
- Beneficiaries 1-4 (distinct, unambiguous name/relationship column pairs)
  are losslessly captured as structured JSON in `BeneficiariesRaw`; the
  free-text "Additional Beneficiaries (Beneficiary 5 and above)" column is
  carried through verbatim rather than parsed, per the data dictionary's
  explicit "do not auto-parse until delimiter/format is agreed" — splitting
  it into individual beneficiary rows is still BIMSS-036/037's job once
  Buklod confirms that format. `ChildrenRaw` similarly stays an unparsed
  verbatim capture of its one source cell.
- No controller/API endpoint yet, matching the BIMSS-021 -> BIMSS-032
  precedent (storage/service abstraction lands first; the admin UI's
  controller wires it up later, here BIMSS-038). A future controller should
  add a `[RequestSizeLimit]` the same way `MemberDocumentsController` does
  (10 MB, undocumented elsewhere in the repo) — no import file size limit is
  confirmed with Buklod yet either.
- Tests: `ImportBatchIngestionServiceTests` (unit — hand-rolled fakes for
  `IExcelWorkbookReader`/`IImportBatchRepository`/`IAuditLogger`, same style
  as `MemberCreationServiceTests`; covers column mapping, the beneficiaries
  JSON envelope, an empty workbook, and the reader-exception ->
  `DomainValidationException` translation); `ClosedXmlWorkbookReaderTests`
  (integration — real ClosedXML round-trip: writes a workbook in memory,
  reads it back, same "exercises a real library/real I/O" placement as
  `LocalFileMemberDocumentStorageTests`).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (376/376 passing) in
  Release, `dotnet format --verify-no-changes`.
- Dependencies: BIMSS-033.

### BIMSS-035 — Staging validation rules (Done)

Merged via [PR #44](https://github.com/agurokeendavid/bi-buklod-bimss/pull/44).

- Implements docs/DOMAIN_WORKFLOWS.md's second migration step: "Validate
  required/format fields." Checks only already-confirmed business rules —
  the same fields `Member`/`MemberEmployment`'s own constructors require
  (`LastName`, `FirstName`, `PlaceOfBirth`, `DateOfBirthRaw`, `CivilStatus`,
  `EmployeeNumber`, `PositionDesignation`, `OfficeUnit`) — plus whether the
  raw `CivilStatus`/`OfficeUnit`/`Suffix` text resolves to a known
  reference-data value, since `IReferenceDataQueryService` (BIMSS-029)
  already exists for exactly that lookup. Does **not** check for
  duplicates/existing-member matches — that stays BIMSS-036.
- `ImportBatchValidationService.ValidateAsync` (`Bimss.Application/
  Membership/`) — loads the batch and its staging rows, loads
  `CivilStatuses`/`OfficeUnits`/`Suffixes` once and matches by name
  (case-insensitive, trimmed), runs each row through required/format/
  reference-match checks, records one `ImportValidationError` per finding,
  calls `MemberImportStaging.RecordValidation` per row and
  `ImportBatch.MarkValidated` once for the batch, and logs an
  `ImportBatch.Validate` audit entry. Same load-mutate-`SaveChangesAsync`
  shape as `MemberStatusTransitionService`.
- An unresolved `CivilStatus`/`OfficeUnit` is an `Error` (blocks promotion,
  since `Member`/`MemberEmployment` construction requires a real reference
  id); an unresolved `Suffix` is only a `Warning` (the field is optional and
  nullable on `Member`). Date parsing uses `DateOnly.TryParse` against both
  invariant and current culture rather than a single hard-coded format,
  since no exact source date format is confirmed.
- `IImportBatchRepository` grew (as expected, per BIMSS-034's note) with
  `GetTrackedByIdAsync`, `GetTrackedRowsByBatchIdAsync`,
  `AddValidationErrorsAsync`; `ImportBatchRepository` implements them
  directly against `BimssDbContext`.
- Tests: `ImportBatchValidationServiceTests` (unit — hand-rolled fakes,
  covers the all-valid path, missing-required-fields, bad date format,
  unresolved required reference (Error), unresolved optional `Suffix`
  (Warning-only, row still valid), batch status transition, audit logging,
  and the not-found path); `ImportBatchRepositoryTests` (integration —
  InMemory provider, row ordering by `RowNumber`, validation-error
  persistence).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (388/388 passing) in
  Release, `dotnet format --verify-no-changes`.
- Dependencies: BIMSS-034.

### BIMSS-036 — Duplicate detection (Done)

Merged via [PR #45](https://github.com/agurokeendavid/bi-buklod-bimss/pull/45).

- Implements docs/DOMAIN_WORKFLOWS.md's third and fourth migration steps:
  "Match possible existing member -> Detect duplicate Employee Number /
  identity candidates." `ImportBatchMatchingService.MatchAsync`
  (`Bimss.Application/Membership/`) requires the batch be `Validated` first
  (`ConflictException` otherwise) but runs matching against every row
  regardless of that row's own `ValidationStatus` — a row can be flagged as
  a duplicate independent of unrelated field errors. Matching does not
  advance `ImportBatch.Status`; it only enriches each row via
  `MemberImportStaging.RecordMatch`, since a reviewer needs both validation
  and match results together before deciding whether to promote (still
  BIMSS-037).
- Two-tier matching, in order: an exact (case-insensitive) `EmployeeNumber`
  match against an existing `MemberEmployment` is a `ConfirmedDuplicate` —
  the number is unique and mandatory, so a match is the same person, not a
  lookalike. Failing that, a same `LastName` + `FirstName` +
  `DateOfBirth` match against an existing `Member` is only a
  `PossibleDuplicate` (same name/DOB doesn't prove same identity) — left for
  a reviewer to confirm or dismiss, never auto-resolved. `DateOfBirthRaw` is
  parsed independently here (not reused from BIMSS-035) since a row can
  still have an unrelated validation error while its date parses fine.
- `IImportBatchRepository` grew with `FindMemberIdByEmployeeNumberAsync`/
  `FindMemberIdByNameAndDateOfBirthAsync`; `ImportBatchRepository`
  implements both with `.ToUpper()` comparisons (translatable by both the
  SQL Server and InMemory providers, unlike `StringComparer.OrdinalIgnoreCase`).
- Tests: `ImportBatchMatchingServiceTests` (unit — hand-rolled fakes;
  confirmed-duplicate, possible-duplicate, no-match, unparseable-date,
  wrong-batch-status, not-found, audit logging); `ImportBatchRepositoryTests`
  gained four cases for the two new repository methods (case-insensitive
  match, no-match, and a date-of-birth mismatch).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (399/399 passing) in
  Release, `dotnet format --verify-no-changes`.
- Dependencies: BIMSS-035.

### BIMSS-037 — Promote staging → domain entities (Done)

Merged via [PR #46](https://github.com/agurokeendavid/bi-buklod-bimss/pull/46).

- Implements docs/DOMAIN_WORKFLOWS.md's final migration steps: "Reviewer
  confirms -> Create/update member through normal application services ->
  Record migration audit." `ImportBatchPromotionService.PromoteRowAsync`
  (`Bimss.Application/Membership/`) promotes exactly **one** staging row at
  a time (the workflow expects a reviewer to look at rows individually,
  never a blind bulk commit), and requires `ValidationStatus == Valid` and
  `MatchStatus == NoMatch` — a `ConfirmedDuplicate` or `PossibleDuplicate`
  row throws `ConflictException` and is left for manual review (no
  auto-resolve policy exists, and none is confirmed with Buklod).
- **Deliberately scoped to `Member` + `MemberEmployment` only** — the same
  fields `CreateMemberCommand` already supports. Its own existing comment
  says exactly why: "Contact, address, education, eligibility, family,
  children, privacy consent, and documents are added afterward through
  their own operations (the officer-review edit workflow, Phase 1E), not
  bundled into this command." `MemberContact`/`MemberAddress`/
  `MemberEducation`/`MemberEligibility`/`MemberFamilyInformation`/
  `MemberPrivacyConsent` have **no** Application-layer creation capability
  anywhere in the codebase yet (confirmed by grepping `Bimss.Application`/
  `Bimss.Api` before starting this task) — building that speculatively here
  with no other caller would be exactly the ahead-of-need work AGENTS.md
  warns against. `MemberChild`/`MemberBeneficiary` stay unpromoted for the
  same reason `ChildrenRaw`/`BeneficiariesRaw` stayed unparsed in
  BIMSS-034: no agreed splitting rule yet.
- Resolves `CivilStatus`/`OfficeUnit` text to a reference-data id (required
  — `DomainValidationException` if unresolvable, though BIMSS-035 should
  already have caught that); an unresolvable `Suffix` degrades to `null`
  rather than blocking, same Warning-only treatment as BIMSS-035.
  Re-parses `DateOfBirthRaw`/`PermanentAppointmentDateRaw` independently
  (not reusing BIMSS-035/036's parse results, which weren't persisted) and
  re-checks `EmployeeNumberExistsAsync` as defense in depth alongside the
  unique DB index.
- **Does not reuse `MemberCreationService`** — that service calls
  `SaveChangesAsync` internally on its own, which would split
  member-creation and marking-the-staging-row-promoted into two separate
  units of work sharing one `DbContext`; a failure between them could leave
  a `Member` created but its staging row still unmarked, inviting a
  double-promotion attempt. Instead `IImportBatchRepository.PromoteRowAsync`
  persists `Member` + `MemberEmployment` + the already-mutated staging row
  (`MemberImportStaging.MarkPromoted`, called by the service beforehand) in
  one atomic `SaveChangesAsync`.
- `ImportBatch.MarkPromoted()` (the *batch-level* terminal status) is still
  unwired — it means "this batch's review is fully closed out," which is a
  reviewer/admin-UI action (BIMSS-038), not implied by promoting one row.
  Same "domain method exists, caller lands later" pattern as `Member`'s
  own `Verify`/`Deactivate` between BIMSS-015 and BIMSS-024/031.
- Tests: `ImportBatchPromotionServiceTests` (unit — hand-rolled fakes;
  happy path, optional-Suffix tolerance, audit logging, not-validated,
  not-matched, confirmed-duplicate, possible-duplicate,
  employee-number-exists, unresolvable civil status, unparseable date,
  not-found); `ImportBatchRepositoryTests` gained two cases (not-found,
  atomic member+employment+row persistence).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (412/412 passing) in
  Release, `dotnet format --verify-no-changes`.
- Dependencies: BIMSS-022, BIMSS-036.

### BIMSS-038 — Import batch admin UI (Done)

Merged via [PR #47](https://github.com/agurokeendavid/bi-buklod-bimss/pull/47). Last Phase 1D task —
Phase 1D is now fully Done.

- New `ImportBatchesController` (`Bimss.Api/Controllers/`, `api/import-batches`)
  wires up BIMSS-033–037's Application services for the first time — none of
  them had any endpoint before this task. Gated by
  `Permission.Membership.Manage`, same as the rest of membership
  administration (no dedicated Import permission exists, and this is a
  Membership Officer action per `docs/design/BIMSS-UI-SPEC.md`'s roles
  table). Routes: `GET /`, `GET /{id}`, `GET /{id}/rows`, `GET /{id}/errors`,
  `POST /` (multipart upload, `[RequestSizeLimit(10_485_760)]` — same
  undocumented 10 MB default as `MemberDocumentsController.Upload`),
  `POST /{id}/validate`, `POST /{id}/match`, `POST /{id}/rows/{rowId}/promote`.
- New `IImportBatchQueryService`/`ImportBatchQueryService`
  (`Bimss.Application`/`Bimss.Infrastructure`) — read-only `AsNoTracking`
  projections (`ImportBatchSummary`/`ImportBatchDetail`/
  `MemberImportStagingRowSummary`/`ImportValidationErrorSummary`), same
  style as `MemberQueryService`. New response contracts under
  `Bimss.Contracts/Membership/`.
- Frontend: `frontend/src/app/dashboard/import-batches/` — a list page
  (upload form + batch table, plain `<table>` since the row count here is
  small, not a TanStack grid) and a `[id]` detail page (batch facts,
  Validate/Match action buttons gated on `ImportBatch.Status`, a validation
  issues panel, and a staging-rows table with a per-row Promote button
  enabled only when `ValidationStatus === "Valid" && MatchStatus ===
  "NoMatch"`; an already-promoted row links to its new member record
  instead). New `lib/types/import-batch.ts` and
  `lib/import-batch-status.ts` (centralized badge-color maps, same
  convention as `lib/member-status.ts`). Added "Member imports" to the
  sidebar nav (`lib/nav-items.ts`).
- Tests: `ImportBatchesControllerTests` (integration —
  `WebApplicationFactory` + `TestAuthHandler`, same pattern as
  `MemberDocumentsControllerTests`; unauthenticated/unauthorized, bad
  upload, full ingest→list→detail→rows round trip, validate→match status
  transitions against seeded reference data, and a full promote-to-member
  case). Extracted a shared `ExcelFixtures.BuildWorkbook` test helper
  (`Bimss.IntegrationTests/Support/`) used by both this and
  `ClosedXmlWorkbookReaderTests`.
- **Frontend verification gap**: no live backend/dev database was available
  in this session to click through the new screens in a browser. Verified
  instead via `npm run lint` (clean), `npm run build` (type-checks the
  frontend against the actual API contracts), and the backend's
  `ImportBatchesControllerTests`, which already exercise the exact same
  HTTP endpoints the frontend calls end-to-end (upload → validate → match →
  promote) through `WebApplicationFactory`. A manual click-through with a
  real `.xlsx` file is still recommended before this ships to users.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (419/419 passing) in
  Release, `dotnet format --verify-no-changes`; `npm run lint`/`npm run
  build` clean on the frontend.
- Dependencies: BIMSS-033–037, BIMSS-047.

## Phase 1E — Member Self-Service (Done)

**Rescoped for the frontend pivot (2026-08-16)**: this is a member-facing
portal, distinct from the officer-facing `/dashboard` admin screens Phase
1C built — it needs its own route group and layout in `frontend/` (e.g.
a member-only area gated on `Permission.Membership.ViewSelf` rather than
`Membership.Manage`), not just more screens bolted onto the existing
admin nav. BIMSS-039's dependency on BIMSS-011 (Bootstrap/Razor layout,
superseded — see the Phase 1C note above) is replaced with BIMSS-047.
BIMSS-040/042/044/045 are self-service equivalents of BIMSS-028/029/030
and will reuse their patterns (`fetchWithAuth`, form validation mirroring
server-side DataAnnotations) scoped to `Membership.ViewSelf`/`ManageSelf`
instead of `Manage`. Detailed scope for each is still worked out when
actually started.

| ID | Title | Status | Depends on |
|---|---|---|---|
| BIMSS-039 | Member dashboard shell | Done — [PR #48](https://github.com/agurokeendavid/bi-buklod-bimss/pull/48) | BIMSS-047 |
| BIMSS-040 | My Profile (read) | Done — [PR #49](https://github.com/agurokeendavid/bi-buklod-bimss/pull/49) | BIMSS-023, BIMSS-039 |
| BIMSS-041 | `MemberUpdateRequest`/Change schema | Done — [PR #50](https://github.com/agurokeendavid/bi-buklod-bimss/pull/50) | BIMSS-004 |
| BIMSS-042 | Member submits update request | Done — [PR #51](https://github.com/agurokeendavid/bi-buklod-bimss/pull/51) | BIMSS-041, BIMSS-039 |
| BIMSS-043 | Officer review/approve/reject | Done — [PR #52](https://github.com/agurokeendavid/bi-buklod-bimss/pull/52) | BIMSS-041, BIMSS-030 |
| BIMSS-044 | Direct self-service edit for low-risk fields | Done — [PR #53](https://github.com/agurokeendavid/bi-buklod-bimss/pull/53) | BIMSS-042 |
| BIMSS-045 | Update request status/history view | Done — [PR #54](https://github.com/agurokeendavid/bi-buklod-bimss/pull/54) | BIMSS-041, BIMSS-039 |

### BIMSS-039 — Member dashboard shell (Done)

Merged via [PR #48](https://github.com/agurokeendavid/bi-buklod-bimss/pull/48).

- New `frontend/src/app/my/` route group — its own layout
  (`components/member-header.tsx`: title, dark-mode toggle, avatar/sign-out
  — no sidebar, no admin nav, per this section's "distinct from
  `/dashboard`" note) and a placeholder landing page. Auth gate matches
  `/dashboard/layout.tsx`'s existing pattern exactly (client-side
  authenticated check only; real authorization for actual member data is a
  server-side concern on each endpoint starting BIMSS-040 —
  `Permission.Membership.ViewSelf`/`ManageSelf` — never enforced
  client-side, per `AGENTS.md`).
- Landing page is a placeholder only — there's no "My Profile" data to show
  yet (that's BIMSS-040); it greets the signed-in user by name (decoded
  from the JWT for display only, same `decodeJwtDisplayName` used
  elsewhere) and states more is coming.
- **Deliberately did not touch** the post-login redirect or add role-aware
  routing between `/dashboard` and `/my` — the access token carries no
  role/permission claims by design (`JwtTokenService`'s own comment:
  permission claims are re-derived server-side per request, never embedded
  in the token), so the frontend has no client-side signal to route on yet.
  `/my` is reachable directly for now; deciding how a member vs. an officer
  lands on the right shell is deferred until there's a concrete reason to
  differentiate (e.g. once nav-by-role filtering exists on the admin side
  too, which also isn't implemented yet).
- Verified: `npm run lint` (clean), `npm run build` (succeeds, `/my` route
  compiles). No live backend was available this session to click through
  authenticated — same known gap noted on BIMSS-038.
- Dependencies: BIMSS-047.

### BIMSS-040 — My Profile (read) (Done)

Merged via [PR #49](https://github.com/agurokeendavid/bi-buklod-bimss/pull/49).

- **Discovered gap, fixed as part of this task**: `ApplicationUser.MemberId`
  (added in BIMSS-005, before `Member` existed) was never actually wired to
  anything — no FK/index, never set anywhere, never read anywhere. A
  self-service "My Profile" is impossible without resolving "which member
  does this login belong to," so this task adds what was missing:
  `ApplicationUserConfiguration` (`Bimss.Infrastructure/Identity/`) — a
  unique filtered index (a member has at most one login and vice versa) and
  a `Restrict` FK to `Members` (never `Cascade` — members are never
  hard-deleted). Migration `LinkApplicationUserToMember`.
- `IMemberQueryService.GetMyProfileByUserIdAsync` (new method) +
  `MyProfileDetail` (`Bimss.Application/Membership/`) — a **separate**
  projection from the officer-facing `MemberDetail`: reference values
  (civil status, suffix, office unit) are resolved to display **names**
  server-side, not left as raw ids. This is deliberate, not just a nicer
  DTO — `ReferenceDataController` stays scoped to
  `Permission.Membership.Manage`, so a self-service caller (`ViewSelf`) has
  no permission to look up those names itself; resolving them server-side
  avoids widening that controller's authorization just for this. Requires
  employment to exist (inner join) since every member created through
  `MemberCreationService` — the only creation path today — gets both rows
  atomically.
- `MyProfileController` (`Bimss.Api/Controllers/`, `api/my/profile`) — its
  own controller (not folded into `MembersController`), gated
  `Permission.Membership.ViewSelf`. Resolves "which member" from the
  caller's own user id (JWT `sub`/`NameIdentifier`), never a route
  parameter — there is no way to request another member's profile through
  this endpoint.
- `DevelopmentMembershipSeeder` now links the `member.dev` dev account
  (seeded by `DevelopmentIdentitySeeder`, which runs first) to the
  synthetic Active member (`DEV-00002`), so local Development has a
  working example out of the box. Existing
  `DevelopmentMembershipSeederTests` needed `AddBimssIdentity()` added to
  their test service provider (previously had no Identity services
  registered at all — nothing had needed them before this task).
- Frontend: `/my/page.tsx` now fetches and renders the real profile
  (replacing BIMSS-039's placeholder) — name, status badge, position/office,
  and a fact grid. A 404 (account not yet linked to a member) renders a
  plain explanatory sentence, not an error.
- Tests: `MemberQueryServiceTests` gained three cases for
  `GetMyProfileByUserIdAsync` (resolved names, no-linked-member, null
  `SuffixName`); `MyProfileControllerTests` (integration —
  `WebApplicationFactory` + `TestAuthHandler`; 401/403/404/200 cases).
  `TestAuthHandler` gained an optional `X-Test-UserId` header (defaults to
  a random id, as before) so a test can authenticate as a specific,
  pre-seeded user — needed to exercise the "linked member" success path.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (427/427 passing)
  in Release, `dotnet format --verify-no-changes`; `npm run lint`/`npm run
  build` clean on the frontend. Same frontend verification gap as
  BIMSS-038/039 (no live backend this session to click through
  authenticated).
- Dependencies: BIMSS-023, BIMSS-039.

### BIMSS-041 — `MemberUpdateRequest`/Change schema (Done)

Merged via [PR #50](https://github.com/agurokeendavid/bi-buklod-bimss/pull/50).

- Implements docs/DOMAIN_WORKFLOWS.md's "Member profile update" workflow:
  "Member edits permitted fields -> Submit Update Request -> Pending
  Review -> Membership Officer reviews differences -> Approve / Reject ->
  Approved changes applied -> History/audit recorded." This task models
  submission and the review decision only — actually applying an approved
  change to `Member`/`MemberEmployment` is the reviewing service's job
  (BIMSS-043), same "schema/domain-rule only" scope as every other schema
  task.
- `MemberUpdateRequest` (`Bimss.Domain/Membership/`) — `Pending -> Approved
  /Rejected` (terminal, no further transitions), guarded the same way as
  `ImportBatch`'s state machine. `Reject` requires non-blank remarks
  (docs/design/BIMSS-UI-SPEC.md's business rule: "Return and Deny require
  remarks" — the member needs to know why); `Approve`'s remarks stay
  optional, same asymmetry as the Approvals screen spec. No FK from
  `SubmittedByUserId`/`ReviewedByUserId` to `AspNetUsers` — same
  established reasoning as `MemberStatusHistory.ActorUserId`.
- `MemberUpdateRequestChange` — one field-level diff (`FieldName`/
  `OldValue`/`NewValue`) per requested field; `internal` constructor, only
  `MemberUpdateRequest` creates one (same pattern as `MemberStatusHistory`).
  Field-agnostic by design — **which** fields are actually submittable
  through this is BIMSS-042's concern, not this schema's; `OldValue`/
  `NewValue` stay unconstrained (`nvarchar(max)`) since they hold whatever
  a `Member`/`MemberEmployment` field's value naturally is, and those vary
  widely in length.
- `MemberUpdateRequestChangeInput` (`Bimss.Domain/Membership/`) — bundles
  one proposed change so the constructor takes a collection of these
  instead of parallel arrays, same reasoning as
  `MemberImportStagingFields`.
- Migration: `AddMemberUpdateRequestSchema` — two tables, `Cascade` FK from
  `MemberUpdateRequests` to `Members` (a request belongs wholly to its
  member, same reasoning as `MemberStatusHistory`) and `Cascade` from
  `MemberUpdateRequestChanges` to its parent request.
- Tests: `MemberUpdateRequestTests` (unit — constructor guards, multi-change
  submission, Approve/Reject success/failure paths, remarks-required-on-
  reject); `MemberUpdateRequestConfigurationTests` (unit, metadata
  inspection); `MemberUpdateRequestPersistenceTests` (integration —
  InMemory round-trip + Approve-across-reloads).
  `MembershipSchemaConstraintTests` gained a real-SQL-Server cascade-delete
  case (`MemberUpdateRequest` -> `Changes`) — InMemory doesn't reliably
  simulate cascade delete unless the children are already loaded/tracked,
  so that check belongs with the other real-DB-only constraint tests, not
  the InMemory persistence file (a first attempt there was reverted after
  it failed for exactly that reason).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (446/446 passing,
  local InMemory-provider run — the new real-SQL-Server cascade case runs
  in CI's SQL Server service container) in Release, `dotnet format
  --verify-no-changes`.
- Dependencies: BIMSS-004.

### BIMSS-042 — Member submits update request (Done)

Merged via [PR #51](https://github.com/agurokeendavid/bi-buklod-bimss/pull/51).

- **Discovered gap, fixed as part of this task**: `Permission.Membership`
  only had `ViewSelf`/`Manage`/`Verify` — no `ManageSelf`, even though the
  Phase 1E backlog note already said screens would be "scoped to
  `Membership.ViewSelf`/`ManageSelf` instead of `Manage`" and the seeded
  `Member` role's own permission list never included it either. Added
  `Permission.Membership.ManageSelf` (submitting/tracking one's own update
  requests, and BIMSS-044's direct edit), wired into `Permission.All`, the
  `Member` role's seed list, `docs/ARCHITECTURE.md`'s example list, and
  `PermissionCatalogTests`' expected set.
- **Second discovered gap**: `ReferenceDataController` (civil statuses,
  suffixes, office units) was gated to `Permission.Membership.Manage`
  only — but the edit form here needs those same lookups to populate its
  Selects, and a self-service caller has only `ManageSelf`. Reference data
  isn't member-specific or sensitive, so rather than widen a single
  `Permission`, added a combined named policy,
  `AuthorizationPolicies.ReferenceDataRead` (`Manage` OR `ManageSelf` via
  `RequireAssertion` — stacking `[Authorize]` attributes AND-combines,
  per `MembersController`'s own comment on that exact pitfall, so a
  second named policy was the only way to express "either"). Also
  extended `MyProfileDetail`/`MyProfileResponse` (BIMSS-040) with the raw
  `SuffixId`/`CivilStatusId`/`OfficeUnitId` alongside the already-resolved
  names — the edit form needs both (names for the read view, ids to
  pre-select each Select), matching what `MemberDetail` already does for
  the officer-facing edit form.
- `MemberUpdateRequestSubmissionService.SubmitAsync`
  (`Bimss.Application/Membership/`) reuses `UpdateMemberCommand` — the same
  shape the officer-direct-edit flow (BIMSS-030) already applies
  immediately — as the "proposed values" input, since the editable field
  set is identical; only the workflow differs (apply immediately vs. queue
  for review). Compares each field against the member's current value and
  records only the ones that actually changed; throws
  `DomainValidationException` if nothing changed. `EmployeeNumber` and
  contact information (phone/email/mailing address) are excluded — the
  former was never in `UpdateMemberCommand` to begin with, the latter is
  BIMSS-044's direct-edit path per `docs/DATA_DICTIONARY.md`'s confirmed
  decision, not this approval workflow.
- `MyUpdateRequestsController` (`api/my/update-requests`, its own
  controller) resolves "which member" from the caller's own user id, same
  pattern as `MyProfileController` — no way to submit on another member's
  behalf.
- Frontend: `/my/update-request/page.tsx` — pre-fills from `/api/my/profile`,
  reuses the `FormSection`/`FormFooter` shell from BIMSS-038's create-member
  page. Linked from `/my`'s "Request a profile change" button.
- Tests: `MemberUpdateRequestSubmissionServiceTests` (unit — only-changed-
  fields recorded, multi-field changes, audit logging, no-op rejection,
  not-found); `MemberQueryServiceTests`/`MyProfileControllerTests` gained
  cases for `GetMemberIdByUserIdAsync` and the new id fields;
  `MyUpdateRequestsControllerTests` (integration — 401/403/404/400/200);
  `AuthorizationPolicyRegistrationTests` gained a case proving
  `ReferenceDataReadPolicy` accepts `Manage` or `ManageSelf` but nothing
  else.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (462/462 passing)
  in Release, `dotnet format --verify-no-changes`; `npm run lint`/`npm run
  build` clean on the frontend (both `/my` routes compile). Same frontend
  verification gap as BIMSS-038/039/040 (no live backend this session).
- Dependencies: BIMSS-041, BIMSS-039.

### BIMSS-043 — Officer review/approve/reject (Done)

Merged via [PR #52](https://github.com/agurokeendavid/bi-buklod-bimss/pull/52).

- Implements docs/DOMAIN_WORKFLOWS.md's remaining "Member profile update"
  steps: "Membership Officer reviews differences -> Approve / Reject ->
  Approved changes applied -> History/audit recorded."
  `MemberUpdateRequestReviewService.ApproveAsync`/`RejectAsync`
  (`Bimss.Application/Membership/`). `RejectAsync` only records the
  decision. `ApproveAsync` additionally **reuses
  `MemberProfileUpdateService`** (BIMSS-030's officer-direct-edit path,
  already validated/tested) to actually apply the change: it replays the
  request's per-field `NewValue` diffs onto the member's *current* values
  to reconstruct the same `UpdateMemberCommand` shape that service already
  persists, rather than re-implementing field-by-field mutation here. The
  decode logic mirrors BIMSS-042's `BuildChanges` encode exactly (same
  `nameof(...)` field-name keys, same `"O"` round-trip date format) — this
  coupling is intentional and documented in both files' comments, not
  hidden. Approving logs **two** separate audit entries
  (`Member.UpdateProfile` from the reused service, `MemberUpdateRequest.Approve`
  from this one) since they're two distinct auditable facts: the review
  decision, and the resulting profile change.
- `IMemberUpdateRequestQueryService`/`MemberUpdateRequestQueryService`
  (list with optional status filter + detail-with-changes) — LINQ-joins
  `Member` in for display (no navigation property between the two
  entities, by design, same as `MemberQueryService`).
- `MemberUpdateRequestsController` (`api/update-requests`, its own flat
  controller — not nested under `/members/{id}`, since this is a
  cross-member review queue, same shape as the Approvals screen in
  `docs/design/BIMSS-UI-SPEC.md`) gated `Permission.Membership.Manage`.
  `Reject` returns 400 if remarks are blank (checked at the controller
  boundary before the domain guard fires, so the officer gets a normal
  validation response rather than an unhandled exception path).
- Frontend: `frontend/src/app/dashboard/update-requests/` — a list page
  (defaults to `?status=Pending`) and a detail page (submitted/reviewed
  timestamps, an old-value/new-value changes table, and Approve/Reject
  actions with a shared remarks textarea, only shown while the request is
  still Pending). Added "Update requests" to the sidebar nav.
- Tests: `MemberUpdateRequestReviewServiceTests` (unit — hand-rolled fakes
  plus a *real* `MemberProfileUpdateService` instance wired to the same
  fakes, since it's a concrete class the review service genuinely depends
  on, not an interface to mock; covers single/multi-field apply, both audit
  entries, not-pending/not-found guards for both actions);
  `MemberUpdateRequestQueryServiceTests` (integration — list, status
  filter, detail-with-changes, not-found);
  `MemberUpdateRequestsControllerTests` (integration —
  401/403/400-missing-remarks/200-reject/200-approve-applies-the-change/
  list-then-get-round-trip).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (478/478 passing)
  in Release, `dotnet format --verify-no-changes`; `npm run lint`/`npm run
  build` clean on the frontend (both new routes compile). Same frontend
  verification gap as BIMSS-038 onward (no live backend this session).
- Dependencies: BIMSS-041, BIMSS-030.

### BIMSS-044 — Direct self-service edit for low-risk fields (Done)

Merged via [PR #53](https://github.com/agurokeendavid/bi-buklod-bimss/pull/53).

- Implements `docs/DATA_DICTIONARY.md`'s confirmed decision: "Self-service
  direct edit (no officer review) is limited to contact information only
  (phone, email, mailing address)" — everything else on `Member`/
  `MemberEmployment` stays on the BIMSS-041/042/043 review-request path.
- `MemberContactSelfServiceUpdateService`
  (`Bimss.Application/Membership/`) upserts `MemberContact` plus both
  `MemberAddress` rows (Present and Permanent — both treated as "mailing
  address" for this rule, since `docs/DATA_DICTIONARY.md` doesn't
  distinguish them and neither carries identity/employment implications)
  atomically in one `SaveChangesAsync`. A blank submitted address value
  leaves an existing address untouched — no "clear" support, matching
  `MemberAddress.UpdateAddressLine`'s own non-blank guard.
  `IMemberRepository` gained four new tracked-load/add methods
  (`GetTrackedContactByMemberIdAsync`, `AddContactAsync`,
  `GetTrackedAddressesByMemberIdAsync`, `AddAddressAsync`) implemented in
  `MemberRepository`; the `Add*Async` methods deliberately don't call
  `SaveChangesAsync` inline (unlike `AddDocumentAsync`'s pattern) so the
  service can commit contact+both addresses in a single transaction.
- `IMemberQueryService.GetMyContactByUserIdAsync`/`MyContactDetail`
  (`Bimss.Application/Membership/`) — a multi-optional-join projection
  (Users -> Members -> MemberContacts[optional] -> MemberAddresses
  filtered Present[optional] -> MemberAddresses filtered
  Permanent[optional]), same "resolve from the caller's own user id"
  shape as `GetMyProfileByUserIdAsync`.
- `MyContactController` (`api/my/contact`) — GET gated
  `Permission.Membership.ViewSelf`, PUT gated `ManageSelf`, resolves
  "which member" from the caller's own user id via
  `IMemberQueryService.GetMemberIdByUserIdAsync` (BIMSS-042), same
  pattern as `MyProfileController`/`MyUpdateRequestsController` — no way
  to edit another member's contact info.
- Frontend: `frontend/src/app/my/contact/page.tsx` — reuses the
  `FormSection`/`FormFooter` shell from BIMSS-038/042. Linked from `/my`
  as "Update contact info", alongside BIMSS-042's "Request a profile
  change" button. `/my/update-request`'s own description already said
  "Contact details are not covered here — those update directly," written
  in BIMSS-042 anticipating this task.
- Tests: `MemberContactSelfServiceUpdateServiceTests` (unit — not-found,
  add-when-none-exist, update-in-place for existing contact/address,
  blank-value-leaves-address-untouched); `MyContactControllerTests`
  (integration — 401/403/404/200-get, 403/400-missing-mobile/200-put with
  a persisted round-trip check).
- Verified: clean rebuild, `dotnet build`/`dotnet test` (489/489 passing:
  2 architecture, 307 unit, 180 integration) in Release, `dotnet format
  --verify-no-changes`; `npm run lint`/`npm run build` clean on the
  frontend (`/my/contact` compiles as a static route). Same frontend
  verification gap as BIMSS-038 onward (no live backend this session).
- Dependencies: BIMSS-042.

### BIMSS-045 — Update request status/history view (Done)

Merged via [PR #54](https://github.com/agurokeendavid/bi-buklod-bimss/pull/54).

- The last Phase 1 task in this backlog. Gives a member their own
  read-only view of the update requests they've submitted (BIMSS-042) and
  how each was resolved (BIMSS-043) — the self-service counterpart to the
  officer-facing `dashboard/update-requests` queue, reusing the exact same
  `MemberUpdateRequestSummary`/`Detail` projections and
  `MemberUpdateRequestSummaryResponse`/`DetailResponse`/`ChangeResponse`
  contracts (no new DTOs needed — the shape was already right, only the
  scoping differs).
- `IMemberUpdateRequestQueryService.ListByMemberIdAsync`
  (`Bimss.Application`/`Bimss.Infrastructure/Membership/`) — the same
  join as the officer queue's `ListAsync`, filtered to one `MemberId`
  instead of `Status`.
- `MyUpdateRequestsController` gained `GET` (list) and `GET {id}` actions.
  **Refactored its authorization from a class-level `[Authorize(Policy =
  ManageSelf)]` to per-action attributes** — stacking a
  `[Authorize(Policy = ViewSelf)]` on the new read actions underneath a
  class-level `ManageSelf` would have AND-combined the two policies
  (`[Authorize]` attributes never OR), the exact pitfall
  `AuthorizationPolicies.ReferenceDataRead`'s own comment already
  documents from BIMSS-042. `GetById` checks `request.MemberId` against
  the caller's own resolved member id and returns 404 (not 403) on a
  mismatch, same "don't confirm another record's existence" reasoning as
  the rest of the `api/my/*` surface — a member can never distinguish "not
  mine" from "doesn't exist" by status code.
- Frontend: `frontend/src/app/my/update-requests/page.tsx` (list, no
  status filter — a member has few enough requests that filtering isn't
  needed yet, unlike the officer queue) and
  `frontend/src/app/my/update-requests/[id]/page.tsx` (detail — read-only
  mirror of `dashboard/update-requests/[id]`, no Approve/Reject controls,
  since that stays an officer-only action gated
  `Permission.Membership.Manage`). Both reuse
  `lib/types/member-update-request.ts` and
  `lib/member-update-request-status.ts` as-is. Linked from `/my` as "My
  update requests," alongside BIMSS-042/044's buttons.
- Tests: `MemberUpdateRequestQueryServiceTests` gained
  `ListByMemberIdAsync_ReturnsOnlyThatMembersRequests`;
  `MyUpdateRequestsControllerTests` gained
  401/403/200-list-scoped-to-caller and
  404-for-another-members-request/200-detail cases for the two new
  actions.
- Verified: clean rebuild, `dotnet build`/`dotnet test` (495/495 passing:
  2 architecture, 307 unit, 186 integration) in Release, `dotnet format
  --verify-no-changes`; `npm run lint`/`npm run build` clean on the
  frontend (both new `/my/update-requests` routes compile). Same frontend
  verification gap as BIMSS-038 onward (no live backend this session).
- Dependencies: BIMSS-041, BIMSS-039.

**Phase 1 (all 45 backlog tasks, BIMSS-001 through BIMSS-045) is Done as
of 2026-08-17.**

## Secrets convention

Confirmed with the project owner (not up for silent revisiting): real
`appsettings.json`/`appsettings.Development.json`/`appsettings.Production.json`
are **never committed** — `dotnet user-secrets` is deliberately not used either.
Copy the committed `appsettings*.json.example` templates to their real names and
edit them directly with real values, including secrets; those files stay
git-ignored regardless. On IIS, place a real `appsettings.Production.json`
directly on the server as part of deployment (ASP.NET Core picks it up
automatically from `ASPNETCORE_ENVIRONMENT`). Full detail in
`docs/REPOSITORY_SETUP.md`.

## Environment notes

- **`dotnet-ef` version**: a global `dotnet-ef` install may be outdated relative
  to this repo's EF Core version. A local, repo-pinned tool manifest
  (`.config/dotnet-tools.json`) exists for this reason — run `dotnet tool restore`
  once per clone and use `dotnet tool run dotnet-ef ...` (or `dotnet ef ...` after
  restore makes it the default) rather than relying on whatever's installed
  globally.
- **Testing convention: EF Core InMemory, not Testcontainers.** `Bimss.IntegrationTests`
  used `Testcontainers.MsSql` (real SQL Server via Docker) through BIMSS-010,
  chosen specifically so unique-index/FK/concurrency-token tests would be
  meaningful. It was replaced with the EF Core InMemory provider during
  BIMSS-011 (2026-08-14): repeated local Docker/WSL2 memory exhaustion made
  the dev loop unreliable (see BIMSS-010/011's verification notes above for
  what that looked like), and spinning up a fresh SQL Server container per
  test class was slow even when it worked. No test in the repo needs Docker
  anymore — `dotnet test` runs standalone. **Known trade-off, accepted
  deliberately**: EF Core InMemory does not enforce real SQL unique
  indexes/FK constraints and does not support `Database.MigrateAsync()`
  (there's no relational migrations pipeline), so tests that specifically
  verified those — `BimssDbContextConnectivityTests` (real SqlServer
  connectivity) and `InitialIdentityMigrationTests` (migration applies
  cleanly) — were removed outright rather than ported, since InMemory can't
  meaningfully stand in for what they checked. If schema-level correctness
  (constraint enforcement, migration application) needs testing again later,
  that requires a real SQL Server target again — LocalDB or a
  CI-workflow-level SQL Server service container are lower-overhead options
  than re-adding Testcontainers.
  - **Implemented in BIMSS-026**: `MembershipSchemaConstraintTests`
    (`Bimss.IntegrationTests/Membership/`) uses the CI-workflow-level SQL
    Server service container option. `.github/workflows/ci.yml` runs
    `mcr.microsoft.com/mssql/server:2022-latest` as a GitHub Actions service
    container (started via the runner's own Docker before any test code
    runs — not Testcontainers, which called Docker *from* the test process
    and was what actually caused BIMSS-011's local resource exhaustion). A
    "Wait for SQL Server" workflow step polls the mapped port before
    `dotnet test` runs, since GitHub Actions doesn't wait for in-container
    readiness beyond container start; `EnableRetryOnFailure()` on the
    `SqlServer` provider covers the remaining brief window before SQL
    Server actually accepts logins. Tests read
    `BIMSS_TEST_SQLSERVER_CONNECTION_STRING` and no-op when it's unset
    (i.e. always locally) — reuse this same pattern for any future
    Loan/Contribution/Election constraint or concurrency test that
    genuinely needs a real database, rather than re-adding Testcontainers.
  - Pattern for tests needing a `BimssDbContext`: give each test its own
    `Guid.NewGuid().ToString()` database name passed to `UseInMemoryDatabase(...)`
    for isolation; no `IAsyncLifetime`/async setup needed.
  - Pattern for `WebApplicationFactory`-based tests needing a real
    `BimssDbContext` behind the app (e.g. `LoginTests`): in
    `ConfigureServices`, `RemoveAll<DbContextOptions<BimssDbContext>>()` **and**
    `RemoveAll<IDbContextOptionsConfiguration<BimssDbContext>>()` before
    re-adding with `UseInMemoryDatabase(...)` — removing only the first still
    leaves the app's original `UseSqlServer` configuration accumulated
    alongside the override (EF Core's `AddDbContext` composes configuration
    via `IDbContextOptionsConfiguration<TContext>` rather than replacing it),
    which throws "Services for database providers ... have been registered"
    at first use.
- **`Microsoft.OpenApi` pin**: `Bimss.Api.csproj` pins `Microsoft.OpenApi` to
  `2.7.5`. Don't "helpfully" bump it back to a floating/latest version without
  checking it still compiles — 3.x versions currently break
  `Microsoft.AspNetCore.OpenApi` 10.0.10's XML-comment source generator.
- **Child entities with client-generated Guid keys added to an already-loaded
  parent's collection need `ValueGeneratedNever()` on the key** (BIMSS-015,
  `MemberStatusHistoryConfiguration`). Without it: load a parent (e.g.
  `Member`) with `.Include(...)` of a collection navigation, append a new
  child to the backing field (e.g. inside a domain method like
  `Member.Verify`), call `SaveChangesAsync()` — EF's reachability fixup sees
  the child's non-default Guid key and assumes it already exists in the
  database, tracking it as `Modified` instead of `Added`, and the save fails
  with `DbUpdateConcurrencyException: Attempted to update or delete an
  entity that does not exist in the store`. This only bites entities
  discovered via graph/collection fixup rather than an explicit
  `dbContext.Set<T>().Add(...)` call — `ReferenceDataItem`-derived entities
  don't hit it because they're always added explicitly. Also a design note
  for aggregate roots with a business constructor that takes non-persisted
  parameters (e.g. `Member`'s `occurredAtUtc`, used only to build the
  initial `MemberStatusHistory` row): EF's constructor-binding at design
  time fails outright ("No suitable constructor was found") if every
  constructor has at least one parameter that doesn't map to a mapped
  property, so such aggregates need a second, private, EF-only constructor
  whose parameters bind 1:1 to persisted properties.
