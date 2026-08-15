# BIMSS Architecture

## Architecture style

**Modular Monolith**

The system is one business platform and should initially remain one deployable solution/domain boundary set rather than microservices.

## Suggested solution

```text
src/
  Bimss.Web/
    Retired as a UI host (frontend pivot, see docs/PHASE1_BACKLOG.md) —
    kept only until its retirement cleanup task lands.

  Bimss.Api/
    REST API controllers and integration endpoints — the only backend
    presentation layer the frontend talks to.

  Bimss.Application/
    Use cases, commands/queries, application services, validation,
    authorization requirements, DTO mapping orchestration

  Bimss.Domain/
    Entities, value objects, domain rules, domain services, status transitions

  Bimss.Infrastructure/
    EF Core, SQL Server, file storage, email/notification implementations,
    external integrations, migrations

  Bimss.Contracts/
    Shared request/response contracts where justified

tests/
  Bimss.UnitTests/
  Bimss.IntegrationTests/
  Bimss.ArchitectureTests/

frontend/
  Next.js + React (TypeScript), shadcn/ui + Tailwind CSS. A separate app,
  not part of the .NET solution, consuming Bimss.Api over REST with JWT
  bearer auth.

e2e/
  Bimss.E2E/
```

## Domain modules

### Identity & Access
Users, roles, permissions, account-member link, login/audit.

### Membership
Member master record, employment, contact, addresses, education, eligibility, membership status, member update requests.

### Beneficiaries
Beneficiary records, effective dates, approval/history.

### Contributions
Contribution batches, contribution transactions, adjustments, member ledger.

### Loans
Loan types, applications, application status history, approvals, released loans, schedules, payments, adjustments.

### Elections
Elections, positions, candidates, eligible voters, participation records, anonymous ballots/votes, finalization, results.

### Documents
Proofs, attachments, generated forms, metadata, secure access.

### Notifications
In-app/email notices and delivery history.

### Reporting
Read models and exports. Reporting must not bypass authorization.

### Audit
Security/business audit events.

## Dependency direction

```text
Api
  -> Application
      -> Domain

Infrastructure
  -> Application/Domain abstractions

Frontend (separate process/app)
  -> Api (over REST, JWT bearer)
```

Domain must not depend on Infrastructure, ASP.NET Core, EF Core, or
JavaScript. The frontend is a separate process, not an in-process consumer
of `Application` — it only ever reaches business logic through `Bimss.Api`'s
HTTP contracts, never by referencing .NET assemblies directly.

## Database strategy

Use one SQL Server database initially. Organize EF mappings and optionally SQL schemas by module if helpful.

Do not create a separate database for each module unless future operational requirements demand it.

EF Core is used Code First: the database schema is derived from `Bimss.Domain` entities and
`Bimss.Infrastructure` `IEntityTypeConfiguration<T>` classes, with migrations generated from that
model (`dotnet ef migrations add`) tracking every schema change alongside its code change. The
schema is never reverse-engineered from an existing database, and never hand-edited outside a
migration.

## API strategy

`Bimss.Api` is the single backend presentation layer. The Next.js frontend
(member/admin UI) consumes it over REST with JWT bearer auth, the same way
any future approved external consumer or integration would — there is no
separate, privileged "internal" API surface for the frontend.

Do not duplicate business logic in API controllers. Controllers coordinate
requests; the same `Bimss.Application` use cases are the single place
business rules live, regardless of which client calls them.

## Authentication direction

ASP.NET Core Identity (`UserManager`/`SignInManager`) remains the credential
store and password/lockout policy engine. `Bimss.Api` issues short-lived JWT
access tokens plus rotating refresh tokens (BIMSS-046) rather than cookies —
see `docs/SECURITY_AND_PRIVACY.md`'s "Authentication and token handling" for
the token-lifecycle/storage details. An approved BI single-sign-on
integration remains an option to layer in later without changing this
downstream contract.

Authorization is based on permissions/policies rather than scattered role
string checks, and is enforced identically regardless of authentication
scheme — `PermissionClaimsTransformation` derives permission claims from the
authenticated user's roles on every request, whether that user authenticated
via a JWT or (for any remaining cookie-based host) a cookie.

Example permissions:

```text
Membership.ViewSelf
Membership.Manage
Membership.Verify
Beneficiary.ManageSelf
Beneficiary.Approve
Contribution.ViewSelf
Contribution.Manage
Loan.Apply
Loan.ViewSelf
Loan.Review
Loan.Approve
Loan.Release
Election.Vote
Election.Manage
Election.Finalize
Report.ViewMembership
Report.ViewFinance
Audit.View
System.ManageUsers
```

## Audit strategy

Use business audit records for important actions such as:

- member verification
- sensitive profile changes
- beneficiary approval/change
- contribution import/posting/adjustment
- loan submission/review/approval/rejection/release/payment adjustment
- election configuration/open/close/finalize/publish
- sensitive report export
- permission changes

Application logs and audit logs serve different purposes. Do not rely only on ordinary text logs for business auditability.
