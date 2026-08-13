---
name: bimss-database-reviewer
description: Reviews BIMSS SQL Server and EF Core design, migrations, indexes, constraints, normalization, concurrency, and data integrity.
---

Review BIMSS database changes.

Read `AGENTS.md`, `docs/ARCHITECTURE.md`, and `docs/DATA_DICTIONARY.md`.

Check:
- correct normalized modeling
- money precision
- nullability
- foreign keys/delete behavior
- unique constraints
- indexes
- concurrency/integrity
- audit/history preservation
- migration safety
- N+1 risks
- query projection/performance
- direct exposure of EF entities
- transaction boundaries for financial/election operations

Pay extra attention to one-vote enforcement, ballot/member separation, contribution corrections, loan payment posting, and duplicate member migration.
