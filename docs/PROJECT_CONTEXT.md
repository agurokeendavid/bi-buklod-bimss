# BIMSS Project Context

## Organization

Bureau of Immigration Buklod ng Kawani.

## Purpose

Replace fragmented/manual Buklod recordkeeping with one secured system for membership and member services.

## Primary user groups

### Member
Can view permitted personal records and use self-service functions.

### Membership Officer
Maintains and verifies official membership information.

### Treasurer / Finance Officer
Posts and reviews contributions, loan releases/payments, and finance reports.

### Loan Officer / Loan Committee
Reviews and processes loan applications according to Buklod rules.

### Election Committee
Configures elections, positions, candidates, voter eligibility, voting periods, finalization, and results.

### Auditor / Read-only Reviewer
Can access approved reports/audit information without unrestricted editing rights.

### System Administrator
Technical configuration and user/permission administration. System administrator access must not automatically grant the ability to see how an individual voted.

## Core modules

1. Identity and Access
2. Membership
3. Beneficiaries
4. Contributions
5. Loans
6. Elections
7. Notifications and Announcements
8. Reports
9. Documents
10. Audit
11. Reference Data
12. Migration/Imports
13. Benefits (benefit claims — confirmed real 2026-08-18, surfaced from the
    Claude Design mockup's Reports/Approvals/dashboard references before
    it had ever been scoped; see `docs/DEVELOPMENT_ROADMAP.md`)

## Important non-functional goals

- Data privacy
- Security
- Traceability
- Auditability
- Election secrecy
- Financial correctness
- Concurrency safety
- Maintainability
- IIS/on-premise deployability
- Simple support and operations

## Migration source

An existing Google Forms Excel export contains membership registration/update fields.

The spreadsheet should be imported through a staging process rather than inserted directly into production domain tables.

Recommended process:

```text
Excel
  -> Import Batch
  -> Import Staging
  -> Validation
  -> Duplicate Detection
  -> Manual Review where needed
  -> Member/Employment/Family/Beneficiary records
  -> Member confirmation/verification
```

Never commit the source workbook to Git.
