# BIMSS Development Roadmap

## Phase 0 — Discovery and policy confirmation

Confirm:
- official member statuses
- required membership fields
- update approval rules
- beneficiary rules
- contribution rules
- loan products/calculations/workflow
- election constitution/rules
- retention/privacy rules
- officer permissions
- SSO/authentication direction

Output:
- approved requirements
- data dictionary
- role/permission matrix
- workflow diagrams

## Phase 1 — Repository and platform foundation

- Create .NET 10 solution/projects
- SQL Server + EF Core 10
- authentication
- permission policies
- audit foundation
- global error handling
- validation conventions
- logging baseline
- CI build/test
- synthetic seed data
- initial layout/navigation

## Phase 2 — Membership and migration

- member master data
- employment/contact/address/education/eligibility
- member search/admin grid
- import batches/staging/errors
- Google Forms Excel migration
- member update requests
- member self-service profile
- document proof handling

## Phase 3 — Beneficiaries

- beneficiary list
- add/update/remove request
- approval/history
- member self-service view

## Phase 4 — Contributions

- contribution batch import/posting
- transaction ledger
- member monthly contribution history
- missing/unposted validation
- finance reports
- adjustments

## Phase 5 — Loans

- loan type configuration
- eligibility rules
- loan application
- review/approval/rejection
- release
- payment schedule
- payments
- balances
- member history
- reports

## Phase 6 — Elections

- election configuration
- positions/candidates
- eligible voter freeze/list
- ballot rules
- secure voting
- one-vote concurrency protection
- close/finalize
- official results
- election audit/report

## Phase 7 — Notifications and reporting

- announcements
- in-app notifications
- email integration if approved
- dashboards
- authorized exports

## Phase 8 — Hardening / UAT / production

- role matrix review
- vulnerability review
- accessibility review
- performance tests
- backup/restore test
- UAT
- deployment runbook
- production readiness checklist

## Suggested first sprint

1. Create solution/repository structure.
2. Configure CI.
3. Implement Identity + permission model.
4. Implement Member aggregate and core reference tables.
5. Build import staging tables based on `docs/DATA_DICTIONARY.md`.
6. Build membership admin list/detail.
7. Add unit/integration test foundation.
