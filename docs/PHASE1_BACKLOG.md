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
| BIMSS-013 | Synthetic seed strategy (Identity portion) | Not started |

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

### BIMSS-013 — Synthetic seed strategy (Identity portion)

- Purpose: seeding for synthetic roles/permissions/dev accounts, Development-only.
- Dependencies: BIMSS-005, BIMSS-006.

## Phase 1B — Membership Domain (Not started)

| ID | Title |
|---|---|
| BIMSS-014 | Reference/master data tables (CivilStatus, Suffix, OfficeUnit, EducationalAttainment, EligibilityType, RelationshipType, MemberStatusReason) |
| BIMSS-015 | Member core aggregate + `MemberStatusHistory` |
| BIMSS-016 | `MemberEmployment` |
| BIMSS-017 | `MemberContact` & `MemberAddress` |
| BIMSS-018 | `MemberEducation` & `MemberEligibility` |
| BIMSS-019 | `MemberFamilyInformation` & `MemberChild` |
| BIMSS-020 | `MemberPrivacyConsent` |
| BIMSS-021 | `MemberDocument` metadata + storage abstraction |
| BIMSS-022 | Member creation use case |
| BIMSS-023 | Member read/query use cases |
| BIMSS-024 | Member status transition service |
| BIMSS-025 | Synthetic membership seed data |
| BIMSS-026 | Membership schema/constraint integration tests |

Open business questions that block/shape some of these (needs Buklod
confirmation — see `docs/DATA_DICTIONARY.md`): whether BI Employee Number is
unique/mandatory (BIMSS-016), which profile fields a member may change without
approval (affects Phase 1E), whether proof of employment is mandatory and what
file types are accepted (BIMSS-021), whether children need birth dates or names
only (BIMSS-019).

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
