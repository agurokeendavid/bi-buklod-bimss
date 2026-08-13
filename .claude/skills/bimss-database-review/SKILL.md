---
name: bimss-database-review
description: Design or review BIMSS SQL Server and EF Core schema, entities, migrations, constraints, indexes, transactions, and data integrity.
---

# BIMSS Database Review

Read `docs/DATA_DICTIONARY.md` and `docs/ARCHITECTURE.md`.

Check:

- normalization matches the domain
- beneficiary records are rows, not fixed columns
- contribution records are transactions
- money uses decimal with explicit precision
- foreign keys and delete behaviors are safe
- business uniqueness has database enforcement
- indexes support expected lookups
- nullability reflects business rules
- migrations are deterministic/reviewable
- audit/history records are preserved
- optimistic concurrency is used where needed
- high-integrity operations use proper transaction boundaries
- EF queries avoid N+1 and unnecessary tracking
- list/report queries project only needed columns

For election changes, verify duplicate voting is prevented under concurrent requests and ballot data is not directly linked to MemberId.
