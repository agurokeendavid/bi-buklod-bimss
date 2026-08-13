---
name: bimss-feature
description: Implement or modify a BIMSS business feature across Domain, Application, Infrastructure, MVC/API, and tests while following repository architecture and security rules.
---

# BIMSS Feature Workflow

1. Read `AGENTS.md`.
2. Read the relevant domain workflow and data/security docs.
3. Inspect existing code for the affected module before creating new abstractions.
4. State the business rule and acceptance criteria in implementation terms.
5. Implement the smallest coherent change:
   - Domain invariant/status logic
   - Application use case
   - Infrastructure persistence
   - MVC/API contract and authorization
   - UI behavior where required
6. Add appropriate tests.
7. If schema changes, add/review an EF Core migration.
8. Check authorization and sensitive-data exposure.
9. Build and run relevant tests.
10. Summarize the change, validation, migration impact, and remaining risk.

Do not put authoritative business rules only in JavaScript.
Do not use real member data in fixtures.
