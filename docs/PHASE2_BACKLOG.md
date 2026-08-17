# BIMSS Phase 2 Backlog

This is the authoritative, in-repo tracker for Phase 2 ("Beneficiaries") work —
task status and what's next. It follows the same convention as
`docs/PHASE1_BACKLOG.md`, which is now a complete historical record — read
that file's "Secrets convention" and "Environment notes" sections for
still-current project-wide conventions; they aren't repeated here.

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

**Current state (2026-08-17): Phase 2 has not started implementation.**
Phase 1 (BIMSS-001 through BIMSS-045, plus BIMSS-046/047) is fully Done — see
`docs/PHASE1_BACKLOG.md`. Buklod confirmed the three blocking beneficiary
questions on 2026-08-17 (see "Confirmed decisions (Buklod, 2026-08-17)" in
`docs/DATA_DICTIONARY.md`): no shares/percentages, no cap on beneficiary
count, and beneficiary changes require officer review/approval (no
direct-edit path, unlike Membership's contact-info exception). One related
question stays genuinely open — see BIMSS-054's note below — and does not
block starting BIMSS-048.

Next task to pick up: **BIMSS-048**.

## Phase 2A — Beneficiaries Module

| ID | Title | Status | Dependencies |
|---|---|---|---|
| BIMSS-048 | `MemberBeneficiary` + `MemberBeneficiaryHistory` schema | Not started | BIMSS-014, BIMSS-015 |
| BIMSS-049 | `MemberBeneficiaryChangeRequest` schema | Not started | BIMSS-048 |
| BIMSS-050 | Member submits beneficiary change request | Not started | BIMSS-049, BIMSS-039 |
| BIMSS-051 | Officer review/approve/reject beneficiary change request | Not started | BIMSS-049, BIMSS-030 |
| BIMSS-052 | Admin UI: member's beneficiaries + review queue | Not started | BIMSS-048, BIMSS-051, BIMSS-047 |
| BIMSS-053 | Self-service UI: view beneficiaries + submit/track requests | Not started | BIMSS-050, BIMSS-039 |
| BIMSS-054 | Backfill: promote imported `BeneficiariesRaw` into `MemberBeneficiary` rows | Not started | BIMSS-048, BIMSS-033–038 |
| BIMSS-055 | Backfill: promote imported `ChildrenRaw` into `MemberChild` rows | Not started | BIMSS-019, BIMSS-033–038 |
| BIMSS-056 | Synthetic beneficiary seed data | Not started | BIMSS-048 |

### BIMSS-048 — `MemberBeneficiary` + `MemberBeneficiaryHistory` schema (Not started)

Schema/domain-rule only, same scope discipline as every Phase 1B Membership
child-entity task — no Application use case, controller, or UI here.

- `MemberBeneficiary` (`Bimss.Domain/Membership/`) — `Name` (required
  free text) and `RelationshipTypeId` (required FK to the existing
  `RelationshipType` reference table from BIMSS-014, `Restrict` delete —
  it's been sitting unused since Phase 1B specifically for this). No
  share/percentage field (confirmed 2026-08-17). A member can have any
  number of beneficiaries — `MemberId` indexed but **not** unique, same
  shape as `MemberEligibility`/`MemberChild`; no application-level or
  database-level cap.
- `MemberBeneficiaryHistory` — one row per effective change (add/update/
  remove), same pattern as `MemberStatusHistory`: `internal` constructor,
  only `MemberBeneficiary`'s owning aggregate operations create one, so
  history stays consistent with actual mutations. This is what
  `docs/DOMAIN_WORKFLOWS.md` #3 means by "Previous record retained in
  history."
- `Cascade` delete to `Member` (meaningless without its member, same
  reasoning as every other Membership child entity).
- Migration: `AddMemberBeneficiary` — two tables, FK to `Members`
  (cascade) and `RelationshipTypes` (restrict).
- Tests: unit (constructor guards, history-row creation on each mutation
  path), configuration (unique-vs-not-unique `MemberId` assertion, same
  style as `MemberEligibilityConfigurationTests`), integration
  (round-trip, multiple beneficiaries per member, history accumulates
  correctly across reloads).

### BIMSS-049 — `MemberBeneficiaryChangeRequest` schema (Not started)

**Do not reuse `MemberUpdateRequest` (BIMSS-041) for this.** That schema
is a field-level diff (`FieldName`/`OldValue`/`NewValue`) designed for
scalar `Member`/`MemberEmployment` field changes — it doesn't naturally
express "add a new beneficiary" (there's no existing field to diff) or
"remove one" (a deletion, not a value change). `Permission.Beneficiary.
ManageSelf`/`Permission.Beneficiary.Approve` already exist in the
Permission catalog as their own module, separate from `Permission.
Membership.ManageSelf`/`Manage` — seeded to roles since BIMSS-013 but
never referenced by any controller — which is a strong signal the
original design intended a dedicated beneficiary request/approval
pipeline, not reuse of the Membership one. Build a new entity instead.

- `MemberBeneficiaryChangeRequest` (`Bimss.Domain/Membership/`) —
  `ActionType` (`Add`/`Update`/`Remove`), `TargetMemberBeneficiaryId`
  (nullable FK — null for `Add`, required for `Update`/`Remove`),
  `ProposedName`/`ProposedRelationshipTypeId` (required for `Add`/
  `Update`, ignored for `Remove`). `Pending -> Approved/Rejected`
  terminal state machine, same guard pattern as `MemberUpdateRequest`:
  `Reject` requires non-blank remarks, `Approve`'s remarks stay optional.
- No FK from `SubmittedByUserId`/`ReviewedByUserId` to `AspNetUsers`,
  same established reasoning as `MemberStatusHistory.ActorUserId`.
- Migration: `AddMemberBeneficiaryChangeRequest` — `Cascade` FK to
  `Members`; the `TargetMemberBeneficiaryId` FK should probably be
  `Restrict` or nullable-on-delete rather than cascade, since a request
  referencing a beneficiary shouldn't silently vanish if that beneficiary
  row changes shape mid-review — decide at implementation time by
  checking what `MemberUpdateRequest`'s closest analogue does.
- Tests: same three-layer pattern as BIMSS-041 (unit, configuration,
  integration), including remarks-required-on-reject and each
  `ActionType`'s field-requiredness.

### BIMSS-050 — Member submits beneficiary change request (Not started)

Mirrors BIMSS-042's split (schema lands first, then the submit path).

- New endpoint(s) on a `MemberBeneficiaryChangeRequestsController` (or
  extend the existing beneficiary-adjacent controller if one exists by
  then), gated on `Permission.Beneficiary.ManageSelf` — the permission
  that's been reserved for exactly this since BIMSS-013.
- Scoped to the authenticated member only, same "member-only endpoints
  stay scoped to the authenticated member" rule as every other
  self-service endpoint.
- Self-service UI wiring is BIMSS-053, not this task — this task is the
  application service + API only, same "backend lands before its screen"
  ordering Phase 1E used throughout.

### BIMSS-051 — Officer review/approve/reject beneficiary change request (Not started)

Mirrors BIMSS-043.

- Gated on `Permission.Beneficiary.Approve`.
- Approving actually applies the effective change to the member's
  `MemberBeneficiary` set (add the row / update the row / soft-remove or
  delete the row — decide which at implementation time, consistent with
  `AGENTS.md`'s "avoid hard deletes for auditable ... records" rule,
  likely meaning removal is itself a tracked history event rather than a
  hard row delete) and appends the corresponding `MemberBeneficiaryHistory`
  row. Rejecting just closes the request with remarks.
- Every approval/rejection records actor, timestamp, status, and remarks
  where required, per `AGENTS.md`'s Loan-rules-style requirement (applies
  equally here even though this is the Beneficiaries module).

### BIMSS-052 — Admin UI: member's beneficiaries + review queue (Not started)

- Extends the member-detail screen from Phase 1C with a Beneficiaries
  panel (current effective set + history), following
  `docs/design/BIMSS-UI-SPEC.md`'s established screen conventions.
- A review queue for pending `MemberBeneficiaryChangeRequest`s, likely
  alongside or reusing the Approvals screen pattern from Phase 1E's
  `MemberUpdateRequest` review UI (BIMSS-043's screen).

### BIMSS-053 — Self-service UI: view beneficiaries + submit/track requests (Not started)

- Extends the My Profile screen from Phase 1E: view current
  beneficiaries, submit add/update/remove requests, and see
  status/history of previously submitted ones (folds in what would
  otherwise be a separate BIMSS-045-style status/history task — the
  scope here is small enough not to warrant a standalone task).

### BIMSS-054 — Backfill: promote imported `BeneficiariesRaw` into `MemberBeneficiary` rows (Not started)

Real data-shape gap, not new feature work: Phase 1D's import batches
(BIMSS-033–038) captured Beneficiary 1–4 as structured JSON in
`BeneficiariesRaw` on `MemberImportStaging`/the promoted member record,
specifically because `MemberBeneficiary` didn't exist as a schema yet
(see `docs/PHASE1_BACKLOG.md`'s BIMSS-034/037 notes). Once BIMSS-048
exists, that already-imported JSON should be parsed into real
`MemberBeneficiary` rows for every previously-imported member.

**Still open and NOT resolved by the 2026-08-17 confirmation**: the
Excel export's free-text "Additional Beneficiaries (Beneficiary 5 and
above)" column has no agreed delimiter/format, per
`docs/DATA_DICTIONARY.md`'s original "do not auto-parse until delimiter/
format is agreed" note. Beneficiaries 1–4 (the structured JSON) can be
backfilled without further confirmation; the free-text overflow column
cannot until that format question gets its own answer from Buklod. Scope
this task to the 1–4 backfill and leave the overflow column staged/raw,
flagged for a follow-up once that question is answered — don't guess a
delimiter.

### BIMSS-055 — Backfill: promote imported `ChildrenRaw` into `MemberChild` rows (Not started)

Same shape of gap as BIMSS-054, called out in the same Phase 1D notes
("`MemberChild`/`MemberBeneficiary` stay unpromoted for the same reason
`ChildrenRaw`/`BeneficiariesRaw` stayed unparsed"). Unlike beneficiaries,
`MemberChild`'s business rules are already fully confirmed (BIMSS-019:
name + birth date both mandatory) — no open question blocks this one.
Bundled into Phase 2 because it's the same import-promotion pattern as
BIMSS-054 and touches the same staging data, not because it's a
Beneficiaries-module task per se.

### BIMSS-056 — Synthetic beneficiary seed data (Not started)

Optional/dev-convenience, same role as BIMSS-025 for Membership — add
synthetic `MemberBeneficiary` rows (and a few `MemberBeneficiaryChangeRequest`s
in various states) to the development seed data so the admin/self-service
UIs (BIMSS-052/053) have something to render against without needing a
live import first.

## Looking beyond Phase 2

`PROJECT_CONTEXT.md`'s remaining core modules — Contributions, Loans,
Elections, Notifications and Announcements, Reports — each have their own
blocking business questions in `docs/DATA_DICTIONARY.md`'s "Questions to
confirm with Buklod before final schema" list. Don't start scoping Phase 3
until Buklod has answered the relevant subset for whichever module comes
next; get those answers and write `docs/PHASE3_BACKLOG.md` the same way
this file was written.
