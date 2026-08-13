---
name: bimss-test-verify
description: Verify a BIMSS change with the correct build, unit, integration, authorization, database migration, and browser tests based on the risk of the change.
---

# BIMSS Verification Workflow

1. Identify changed modules and risk.
2. Build the solution.
3. Run affected unit tests.
4. Run affected integration tests.
5. If authorization changed, run/add authorization tests.
6. If schema changed:
   - review migration
   - apply it to a disposable/local database
   - verify constraints/indexes relevant to the feature
7. If contribution/loan posting changed, test duplicate/retry/concurrency behavior.
8. If election voting changed, test duplicate/concurrent vote attempts and voter/ballot separation.
9. If member-facing/admin UI changed, run the relevant Playwright/E2E flow when available.
10. Report:
   - commands/tests run
   - pass/fail
   - uncovered risk
   - whether a manual UAT step remains

Do not claim verification if the relevant command/test was not actually run.
