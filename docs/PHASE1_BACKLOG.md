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

**Current state (2026-08-14): Phase 1A is fully Done** (BIMSS-001 through
BIMSS-013, all merged). Phase 1B (Membership Domain) is in progress: its
blocking business questions were confirmed with Buklod on 2026-08-14 (see the
note under Phase 1B below and "Confirmed decisions" in
`docs/DATA_DICTIONARY.md`), and BIMSS-014 (reference/master data tables), BIMSS-015 (Member core aggregate +
`MemberStatusHistory`), BIMSS-016 (`MemberEmployment`), BIMSS-017
(`MemberContact` & `MemberAddress`), BIMSS-018 (`MemberEducation` &
`MemberEligibility`), and BIMSS-019 (`MemberFamilyInformation` &
`MemberChild`) are now Done. BIMSS-020 (`MemberPrivacyConsent`) is next.

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

## Phase 1B — Membership Domain (Not started)

| ID | Title | Status |
|---|---|---|
| BIMSS-014 | Reference/master data tables (CivilStatus, Suffix, OfficeUnit, EducationalAttainment, EligibilityType, RelationshipType, MemberStatusReason) | Done — [PR #18](https://github.com/agurokeendavid/bi-buklod-bimss/pull/18) |
| BIMSS-015 | Member core aggregate + `MemberStatusHistory` | Done — [PR #19](https://github.com/agurokeendavid/bi-buklod-bimss/pull/19) |
| BIMSS-016 | `MemberEmployment` | Done — [PR #20](https://github.com/agurokeendavid/bi-buklod-bimss/pull/20) |
| BIMSS-017 | `MemberContact` & `MemberAddress` | Done — [PR #21](https://github.com/agurokeendavid/bi-buklod-bimss/pull/21) |
| BIMSS-018 | `MemberEducation` & `MemberEligibility` | Done — [PR #22](https://github.com/agurokeendavid/bi-buklod-bimss/pull/22) |
| BIMSS-019 | `MemberFamilyInformation` & `MemberChild` | Done — [PR #23](https://github.com/agurokeendavid/bi-buklod-bimss/pull/23) |
| BIMSS-020 | `MemberPrivacyConsent` | Not started |
| BIMSS-021 | `MemberDocument` metadata + storage abstraction | Not started |
| BIMSS-022 | Member creation use case | Not started |
| BIMSS-023 | Member read/query use cases | Not started |
| BIMSS-024 | Member status transition service | Not started |
| BIMSS-025 | Synthetic membership seed data | Not started |
| BIMSS-026 | Membership schema/constraint integration tests | Not started |

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

## Phase 1C — Membership Administration (Not started)

| ID | Title | Depends on |
|---|---|---|
| BIMSS-027 | Membership admin list (DevExtreme grid) | BIMSS-023 |
| BIMSS-028 | Member details view | BIMSS-023 |
| BIMSS-029 | Create member (admin UI) | BIMSS-022, BIMSS-011 |
| BIMSS-030 | Edit permitted information (officer-direct-edit) | BIMSS-022 |
| BIMSS-031 | Activate/Deactivate/status UI | BIMSS-024 |
| BIMSS-032 | Verification workflow UI + audit/history panel | BIMSS-024, BIMSS-007 |

## Phase 1D — Existing Member Import (Not started)

| ID | Title | Depends on |
|---|---|---|
| BIMSS-033 | ImportBatch/MemberImportStaging/ImportValidationError schema | BIMSS-004 |
| BIMSS-034 | Excel ingestion service | BIMSS-033 |
| BIMSS-035 | Staging validation rules | BIMSS-034 |
| BIMSS-036 | Duplicate detection | BIMSS-035 |
| BIMSS-037 | Promote staging → domain entities | BIMSS-022, BIMSS-036 |
| BIMSS-038 | Import batch admin UI | BIMSS-033–037 |

## Phase 1E — Member Self-Service (Not started)

| ID | Title | Depends on |
|---|---|---|
| BIMSS-039 | Member dashboard shell | BIMSS-011 |
| BIMSS-040 | My Profile (read) | BIMSS-023 |
| BIMSS-041 | `MemberUpdateRequest`/Change schema | BIMSS-004 |
| BIMSS-042 | Member submits update request | BIMSS-041 |
| BIMSS-043 | Officer review/approve/reject | BIMSS-041, BIMSS-030 |
| BIMSS-044 | Direct self-service edit for low-risk fields | BIMSS-042 |
| BIMSS-045 | Update request status/history view | BIMSS-041 |

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
