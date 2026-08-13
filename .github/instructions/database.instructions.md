---
applyTo: "**/*DbContext*.cs,**/Migrations/**/*.cs,**/*.sql,**/Infrastructure/**/*.cs"
---

# SQL Server / EF Core Instructions

- EF Core 10 + SQL Server is the default persistence stack.
- Code First: entities and `IEntityTypeConfiguration<T>` classes in C# are the source of truth.
  Never scaffold entities from an existing database and never hand-edit the schema outside migrations.
- Use migrations, generated from the model with `dotnet ef migrations add`.
- Use SQL `decimal` for money and define precision explicitly.
- Add unique constraints/indexes for business uniqueness and concurrent safety.
- Use foreign keys and appropriate delete behavior; avoid cascade deletion of financial/audit records.
- Prefer projections for read models.
- Watch for N+1 queries.
- Do not use unparameterized raw SQL.
- Do not store beneficiaries in Beneficiary1/Beneficiary2 columns.
- Do not store contribution months as columns.
- Do not hard-delete posted contributions, payments, finalized election artifacts, or audit records.
- Schema changes affecting financial or election integrity require integration tests.
