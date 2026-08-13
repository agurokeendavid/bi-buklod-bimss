# BIMSS Architecture

## Architecture style

**Modular Monolith**

The system is one business platform and should initially remain one deployable solution/domain boundary set rather than microservices.

## Suggested solution

```text
src/
  Bimss.Web/
    MVC controllers, Razor views, static web assets, UI composition

  Bimss.Api/
    REST API controllers and integration endpoints

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
Web/API
  -> Application
      -> Domain

Infrastructure
  -> Application/Domain abstractions

Domain must not depend on Infrastructure, MVC, EF Core, DevExtreme, or JavaScript.
```

## Database strategy

Use one SQL Server database initially. Organize EF mappings and optionally SQL schemas by module if helpful.

Do not create a separate database for each module unless future operational requirements demand it.

EF Core is used Code First: the database schema is derived from `Bimss.Domain` entities and
`Bimss.Infrastructure` `IEntityTypeConfiguration<T>` classes, with migrations generated from that
model (`dotnet ef migrations add`) tracking every schema change alongside its code change. The
schema is never reverse-engineered from an existing database, and never hand-edited outside a
migration.

## API and MVC strategy

MVC is the primary member/admin web UI.

The Web API supports:
- DevExtreme data operations where appropriate
- future integrations
- asynchronous UI operations
- approved external consumers

Do not duplicate business logic between MVC and API. Both call the same Application use cases.

## Authentication direction

Use ASP.NET Core Identity or an approved BI single-sign-on integration.

Authorization should be based on permissions/policies rather than scattered role string checks.

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
