# BIMSS Phase 3 Backlog

This is the authoritative, in-repo tracker for Phase 3 ("Contributions")
work — task status and what's next. Same convention as
`docs/PHASE1_BACKLOG.md`/`docs/PHASE2_BACKLOG.md`; read `PHASE1_BACKLOG.md`'s
"Secrets convention" and "Environment notes" for still-current project-wide
conventions not repeated here.

Read `AGENTS.md` and the rest of `docs/` first for the standing rules this
backlog was built against. This file tracks task-level progress only; it
doesn't restate architecture or security rules that live elsewhere.

## Status legend

- `Done` — merged to `main`.
- `In branch` — implemented and committed, not yet pushed/merged. Branch name given.
- `Not started`

## How to pick this up cold

1. Run `git log --oneline -10 main` and `gh pr list --state all --limit 10` to
   confirm this file still matches reality (it can drift if someone forgets to
   update it — trust git over this doc if they disagree).
2. Find the first `Not started` (or unfinished `In branch`) task below, in
   order — later tasks generally depend on earlier ones.
3. Create a feature branch, implement, verify (build + test + `dotnet format
   --verify-no-changes`), commit, then confirm with whoever's driving before
   pushing/opening a PR.
4. Update this file's status/PR link in the same PR that completes the task.

**Current state (2026-08-17): Phase 3 has not started implementation.**
Phase 1 and Phase 2 are Done/scoped — see `docs/PHASE1_BACKLOG.md` and
`docs/PHASE2_BACKLOG.md`. Buklod confirmed the blocking Contributions
question on 2026-08-17 (see "Confirmed decisions (Buklod, 2026-08-17,
Contributions)" in `docs/DATA_DICTIONARY.md`): a fixed flat contribution
rate (effective-dated, same for every member), one contribution type only,
and a pure-ledger model with no expected-vs-actual tracking.

Next task to pick up: **BIMSS-057**.

## Phase 3A — Contributions Module

| ID | Title | Status | Dependencies |
|---|---|---|---|
| BIMSS-057 | `ContributionRate` schema (effective-dated flat rate) | Not started | BIMSS-004 |
| BIMSS-058 | `ContributionBatch`/`ContributionStaging`/`ContributionValidationError` schema | Not started | BIMSS-004, BIMSS-015 |
| BIMSS-059 | `Contribution`/`ContributionAdjustment` schema (immutable ledger) | Not started | BIMSS-015, BIMSS-057 |
| BIMSS-060 | Contribution batch ingestion service | Not started | BIMSS-058 |
| BIMSS-061 | Contribution staging validation + posting service | Not started | BIMSS-058, BIMSS-059 |
| BIMSS-062 | Admin UI: contribution batch import + review/post | Not started | BIMSS-060, BIMSS-061, BIMSS-047 |
| BIMSS-063 | Member contribution ledger view (admin + self-service) | Not started | BIMSS-059, BIMSS-047, BIMSS-039 |
| BIMSS-064 | Adjustment/reversal workflow + UI | Not started | BIMSS-059, BIMSS-062 |
| BIMSS-065 | Synthetic contribution seed data | Not started | BIMSS-059 |

### BIMSS-057 — `ContributionRate` schema (Not started)

- `ContributionRate` (`Bimss.Domain/Contributions/` — first content in a
  new Contributions bounded module, sibling to `Bimss.Domain/Membership/`)
  — `EffectiveFrom` (date, required) and `Amount` (`decimal`, required,
  positive — never `float`/`real`, per `AGENTS.md`'s money rule). No
  `EffectiveTo`: the rate effective for a given date is the row with the
  latest `EffectiveFrom <= that date`, same open-ended-effective-dating
  pattern used elsewhere rather than tracking redundant end dates.
  Immutable once created — a rate change is a new row, never an edit to
  an existing one (same reasoning as every other Phase 1 append-only
  entity).
- Informational/reference use only, **not** a posting gate: per the
  confirmed "pure ledger" decision (BIMSS-058/059), a posted contribution
  amount does not have to match the current rate. This table exists so
  dashboards/forms can show "current rate: ₱X" and so `BIMSS-060`'s
  ingestion can flag (not block) rows whose amount looks off, not so the
  domain enforces it.
- Migration: `AddContributionRate` — one table, unique index on
  `EffectiveFrom` (two rates can't both take effect the same day).
- Tests: unit (constructor guards), configuration (metadata inspection),
  integration (round-trip, "rate as of date X" lookup helper).
- Admin UI for managing rates is out of this task's scope — a small
  addition to BIMSS-062 or its own minor follow-up once the rest of the
  module's UI exists.

### BIMSS-058 — `ContributionBatch`/`ContributionStaging`/`ContributionValidationError` schema (Not started)

Mirrors BIMSS-033's `ImportBatch`/`MemberImportStaging`/
`ImportValidationError` pattern closely — Contributions gets imported in
batches the same way Membership records did, per
`docs/DOMAIN_WORKFLOWS.md` #4 ("Finance creates/imports Contribution
Batch -> Validate rows and Member references -> Resolve errors -> Post
batch -> ..."). Schema/domain-rule only — no ingestion/validation logic
here (BIMSS-060/061).

- `ContributionBatch` (`Bimss.Domain/Contributions/`) — one row per
  import/posting run. `Created -> Staged -> Validated -> Posted`
  lifecycle (`Posted` replacing `ImportBatch`'s `Promoted` — same shape,
  different terminal-state name to match this domain's vocabulary), plus
  `Cancel` from any non-terminal state. Each transition guarded with
  `ConflictException` on an invalid move, same pattern as `ImportBatch`.
  No FK from `UploadedByUserId` to `AspNetUsers` (indexed only), matching
  every prior actor-reference entity.
- `ContributionStaging` (`Bimss.Domain/Contributions/`) — one row per
  source file row: `MemberId` reference (resolved by employee number or
  similar business key — exact matching rule is BIMSS-060's concern),
  `Amount`, `ContributionDate` (the period/date the contribution applies
  to), raw/unvalidated until `RecordValidation` marks it. Does not carry
  an in-memory collection on `ContributionBatch` — queried by
  `ContributionBatchId`, same N+1-avoidance reasoning as
  `MemberImportStaging`.
- `ContributionValidationError` (`Bimss.Domain/Contributions/`) —
  immutable row/field-level or batch-level finding, same shape as
  `ImportValidationError`.
- Migration: `AddContributionImportSchema` — three tables, FKs
  cascading batch -> staging -> validation errors, `Restrict` FK from
  staging to `Member`.
- Tests: same three-layer pattern (unit/configuration/integration) as
  BIMSS-033, plus a real-SQL-Server constraint test alongside
  whatever `MembershipSchemaConstraintTests`-equivalent test class this
  module gets (likely `ContributionSchemaConstraintTests`, following the
  `docs/PHASE1_BACKLOG.md` "Environment notes" pattern for real-DB-only
  checks via the CI SQL Server service container).

### BIMSS-059 — `Contribution`/`ContributionAdjustment` schema (Not started)

- `Contribution` (`Bimss.Domain/Contributions/`) — the immutable ledger
  transaction itself: `MemberId`, `Amount` (`decimal`), `ContributionDate`,
  `PostedAtUtc`, `SourceBatchId` (nullable FK to `ContributionBatch` —
  nullable because a future single-transaction manual-posting path,
  outside this phase's scope, might not come from a batch). **Never**
  January–December columns, per `AGENTS.md`'s explicit rule — this is the
  transaction/ledger row the rule requires. No update method — a posted
  contribution is never edited or deleted; corrections go through
  `ContributionAdjustment`.
- `ContributionAdjustment` (`Bimss.Domain/Contributions/`) — a traceable
  correction: references the original `Contribution` (`Restrict` delete —
  an adjustment must not silently vanish if someone tries to remove the
  original row), `AdjustmentAmount` (signed `decimal` — positive to add,
  negative to reverse/reduce), `Reason` (required), actor/timestamp.
  Never overwrites the original row, per `AGENTS.md`'s "never overwrite
  historical contribution records" rule and `docs/DOMAIN_WORKFLOWS.md`
  #4's "traceable adjustment/reversal workflow."
- A member's effective total for any period is computed by summing
  `Contribution` + its `ContributionAdjustment`s — reports must be
  reproducible from persisted transactions, per `AGENTS.md`.
- Migration: `AddContributionLedger` — two tables, FKs as above.
- Tests: unit/configuration/integration, same three-layer pattern; a
  specific test asserting the summed-total calculation is correct across
  a contribution + multiple adjustments.

### BIMSS-060 — Contribution batch ingestion service (Not started)

Mirrors BIMSS-034 (Excel ingestion service). Parses a finance-provided
file (format TBD — likely CSV or Excel; confirm the actual source format
Buklod's finance office uses before building the parser, don't assume
it's the same Google Forms Excel shape Membership used) into
`ContributionStaging` rows under a new `ContributionBatch`. Resolves
each row's member reference (by BI Employee Number, the confirmed unique
business identifier from Phase 1B) — unresolvable rows become
`ContributionValidationError`s, not silent skips or thrown exceptions,
same "reader failures translate to a domain validation error with a
field error" pattern as BIMSS-034.

### BIMSS-061 — Contribution staging validation + posting service (Not started)

Mirrors BIMSS-035 (staging validation) and BIMSS-037/038's posting half.
Validates staged rows (member resolves to an `Active` member; amount is
positive; date is sane) and, on posting, creates the immutable
`Contribution` rows from validated staging rows in one transaction per
batch, advancing `ContributionBatch` to `Posted`. Per the confirmed pure-
ledger decision, an amount that doesn't match the current
`ContributionRate` is a **warning**, not a blocking validation error —
finance may legitimately post a different amount (partial payment,
correction already known at import time, etc.).

### BIMSS-062 — Admin UI: contribution batch import + review/post (Not started)

Extends the Next.js admin shell, following the import-batches screen
pattern from BIMSS-038: upload a file, see staging rows and validation
errors, resolve/skip, post the batch. Gated on `Permission.Contribution.
Manage` (reserved since BIMSS-006, unused so far).

### BIMSS-063 — Member contribution ledger view (Not started)

- Admin: a Contributions panel on the member-detail screen (running
  ledger + adjustments), following `docs/design/BIMSS-UI-SPEC.md`'s
  established conventions — the membership register's "Contributions
  YTD/Last posted" columns already anticipate this (see
  `docs/PHASE1_BACKLOG.md`'s design-pass notes).
- Self-service: a "My Contributions" view on the member dashboard, gated
  on `Permission.Contribution.ViewSelf` (reserved since BIMSS-006).

### BIMSS-064 — Adjustment/reversal workflow + UI (Not started)

Officer-only action (`Permission.Contribution.Manage`) to create a
`ContributionAdjustment` against a posted contribution, with a required
reason. Every adjustment is itself audited via `IAuditLogger`
(BIMSS-007), same as every other financially-sensitive action in this
codebase.

### BIMSS-065 — Synthetic contribution seed data (Not started)

Optional/dev-convenience, same role as BIMSS-025 for Membership and
BIMSS-056 for Beneficiaries — a synthetic `ContributionRate` history plus
posted `Contribution` rows (and a couple of `ContributionAdjustment`s)
for the seeded dev members, so BIMSS-062/063's screens have something to
render against without a real import.

## Looking beyond Phase 3

Loans and Elections each still have their own blocking Buklod question
in `docs/DATA_DICTIONARY.md`'s "Questions to confirm with Buklod before
final schema" list. Get those answered and write `docs/PHASE4_BACKLOG.md`
(Loans) / `docs/PHASE5_BACKLOG.md` (Elections) the same way this file and
`docs/PHASE2_BACKLOG.md` were written — see `docs/DEVELOPMENT_ROADMAP.md`
for the current phase-to-module mapping.
