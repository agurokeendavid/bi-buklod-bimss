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
| BIMSS-005 | ASP.NET Core Identity + first migration | In branch — `feature/bimss-005-identity-initial-migration` |
| BIMSS-006 | Permission/policy authorization model | Not started |
| BIMSS-007 | Audit logging foundation | Not started |
| BIMSS-008 | Global exception handling & typed exceptions | Not started |
| BIMSS-009 | Validation conventions | Not started |
| BIMSS-010 | DI composition conventions | Not started |
| BIMSS-011 | Base layout, navigation shell, template cleanup | Not started |
| BIMSS-012 | Testing foundation (architecture tests, shared integration fixture) | Not started |
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

### BIMSS-005 — ASP.NET Core Identity + first migration (In branch)

Implemented on `feature/bimss-005-identity-initial-migration`, not yet merged.

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

### BIMSS-006 — Permission/policy authorization model

- Purpose: `Permission` catalog (constants, not free text), `RolePermission` table,
  claims transformation at sign-in, named `AuthorizationPolicy` registration.
  Never `[Authorize(Roles = "...")]` string checks — see `AGENTS.md`.
- Dependencies: BIMSS-005.
- Acceptance criteria: a policy-protected action correctly allows/denies based on
  the signed-in user's role→permission mapping; permission catalog includes all
  permissions listed in `docs/ARCHITECTURE.md` (most unused until later phases).
- Tests: unit tests for the policy handler; integration test hitting a protected
  endpoint as authorized/unauthorized users.
- Security: verify unauthenticated requests are rejected before handler logic
  runs; verify server-side enforcement independent of any client-side UI hiding.

### BIMSS-007 — Audit logging foundation

- Purpose: `AuditEvent` entity + `IAuditLogger` service, populated by explicit
  application-service calls at the point a business action happens (not a generic
  `SaveChanges` diff interceptor — see `docs/SECURITY_AND_PRIVACY.md`'s ban on
  beneficiary/address/ballot data in logs).
- Dependencies: BIMSS-005, BIMSS-004.
- Acceptance criteria: a sample audit call round-trips with actor, action, object
  type/id, timestamp, result, remarks.
- Security: call sites must never pass beneficiary data, ballot content, or full
  addresses as "safe metadata."

### BIMSS-008 — Global exception handling & typed exceptions

- Purpose: `IExceptionHandler` + `ProblemDetails` for the API, MVC exception
  handling, shared typed exception hierarchy (`NotFoundException`,
  `ConflictException`, `ForbiddenException`, `DomainValidationException`).
- Acceptance criteria: exceptions map to correct HTTP status codes; no stack
  traces/internal details leak outside Development.

### BIMSS-009 — Validation conventions

- Purpose: DataAnnotations at the DTO/API boundary + Domain guard clauses for
  business rules. No FluentValidation yet — revisit if rule complexity grows.
- Dependencies: BIMSS-008.

### BIMSS-010 — DI composition conventions

- Purpose: `AddBimssApplication()`, `AddBimssInfrastructure()`,
  `AddBimssAuthorization()` extension methods, consolidating what BIMSS-004
  through BIMSS-009 registered piecemeal. No MediatR/CQRS framework.
- Dependencies: BIMSS-004 through BIMSS-009.

### BIMSS-011 — Base layout, navigation shell, template cleanup

- Purpose: Bootstrap layout, auth-aware nav, login/logout; remove
  `WeatherForecastController` and default MVC scaffold content.
- Dependencies: BIMSS-005/006.

### BIMSS-012 — Testing foundation

- Purpose: `NetArchTest.Rules`-based architecture tests enforcing dependency
  direction (Domain has no EF/ASP.NET Core reference, Application has no
  Infrastructure reference); a **shared, reusable** integration test fixture
  (Testcontainers.MsSql collection fixture) — generalizing the one-off container
  setup BIMSS-004's connectivity test used.
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
- **Docker required for integration tests**: `Bimss.IntegrationTests` uses
  `Testcontainers.MsSql`, which needs a running Docker engine (Docker Desktop on
  Windows). Check `docker info` if integration tests fail to start — a
  `DockerUnavailableException` means the daemon isn't running, not a code problem.
- **`Microsoft.OpenApi` pin**: `Bimss.Api.csproj` pins `Microsoft.OpenApi` to
  `2.7.5`. Don't "helpfully" bump it back to a floating/latest version without
  checking it still compiles — 3.x versions currently break
  `Microsoft.AspNetCore.OpenApi` 10.0.10's XML-comment source generator.
