# BIMSS Phase 5 Backlog

This is the authoritative, in-repo tracker for Phase 5 ("Elections") work
— task status and what's next. Same convention as
`docs/PHASE1_BACKLOG.md`/`docs/PHASE2_BACKLOG.md`/`docs/PHASE3_BACKLOG.md`/
`docs/PHASE4_BACKLOG.md`; read `PHASE1_BACKLOG.md`'s "Secrets convention"
and "Environment notes" for still-current project-wide conventions not
repeated here.

Read `AGENTS.md` and the rest of `docs/` first for the standing rules this
backlog was built against — this module especially: `AGENTS.md`'s
"Election rules" section, and its explicit line "Election integrity code
requires extra tests and review," are not optional flavor text. This file
tracks task-level progress only; it doesn't restate architecture or
security rules that live elsewhere.

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
5. Per `CLAUDE.md`/`AGENTS.md`, prefer Claude Code's planning behavior
   before editing election/voting code, and get a dedicated review pass
   on BIMSS-079/082 specifically (ballot secrecy + one-vote concurrency)
   beyond the standard test suite.

**Current state (2026-08-18): Phase 5 has not started implementation.**
Phases 1–4 are Done/scoped — see `docs/PHASE1_BACKLOG.md` through
`docs/PHASE4_BACKLOG.md`. Buklod confirmed the blocking Elections
question on 2026-08-18 (see "Confirmed decisions (Buklod, 2026-08-18,
Elections)" in `docs/DATA_DICTIONARY.md`): positions configured per
election, multiple seats per position allowed (top-N win), abstention
allowed, eligibility is Active status only as of the voter-list freeze.

This resolves the last of the module-blocking business questions —
Elections was the final `Not started` module with an open question in
`docs/DATA_DICTIONARY.md`'s list (only "retirees/honorary members" and
"record-retention rules" remain, neither of which blocks a specific
phase's schema the way the others did).

Next task to pick up: **BIMSS-077**.

## Phase 5A — Elections Module

| ID | Title | Status | Dependencies |
|---|---|---|---|
| BIMSS-077 | `Election`/`ElectionPosition`/`ElectionCandidate` schema | Not started | BIMSS-004, BIMSS-015 |
| BIMSS-078 | `ElectionEligibleVoter` schema (voter list freeze) | Not started | BIMSS-077 |
| BIMSS-079 | `ElectionParticipation`/`ElectionBallot`/`ElectionVote` schema | Not started | BIMSS-077, BIMSS-078 |
| BIMSS-080 | `ElectionFinalizedResult` schema | Not started | BIMSS-079 |
| BIMSS-081 | Election setup service (positions/seats/candidates/voter freeze) | Not started | BIMSS-077, BIMSS-078 |
| BIMSS-082 | Voting service (atomic participation + ballot recording) | Not started | BIMSS-079 |
| BIMSS-083 | Election closing/finalization + tally service | Not started | BIMSS-080, BIMSS-082 |
| BIMSS-084 | Admin UI: election setup + monitoring | Not started | BIMSS-081, BIMSS-047 |
| BIMSS-085 | Self-service UI: voting screen | Not started | BIMSS-082, BIMSS-039 |
| BIMSS-086 | Results UI (finalize action + published results) | Not started | BIMSS-083, BIMSS-047 |
| BIMSS-087 | Synthetic election seed data | Not started | BIMSS-080 |

### BIMSS-077 — `Election`/`ElectionPosition`/`ElectionCandidate` schema (Not started)

Schema/domain-rule only — the setup half of `docs/DOMAIN_WORKFLOWS.md`
#7. First content in a new Elections bounded module
(`Bimss.Domain/Elections/`, sibling to `Bimss.Domain/Membership/`/
`Contributions/`/`Loans/`).

- `Election` — `Name`, voting window (`VotingOpensAtUtc`/
  `VotingClosesAtUtc`). Lifecycle: `Draft -> Scheduled/Open -> Closed ->
  Finalized`, guarded the same way as every other state machine in the
  codebase.
- `ElectionPosition` — belongs to one `Election` (confirmed decision 18:
  positions are per-election, not a shared constitutional set), `Name`,
  `SeatCount` (required, positive — confirmed decision 19: a position
  can have more than one seat; top-`SeatCount` candidates by vote count
  win).
- `ElectionCandidate` — belongs to one `ElectionPosition`, references a
  `Member` (`Restrict` delete — a candidate record must survive if the
  member's status changes), display name/bio fields as needed.
- Migration: `AddElectionSetupSchema` — three tables, `Cascade` from
  `Election` to `ElectionPosition` to `ElectionCandidate`, `Restrict` FK
  to `Member`.
- Tests: three-layer pattern (unit/configuration/integration), including
  a multi-seat position test (several candidates under one position).

### BIMSS-078 — `ElectionEligibleVoter` schema (Not started)

- `ElectionEligibleVoter` — one row per `(ElectionId, MemberId)`, created
  by "freezing" the eligible voter list at a point in time (confirmed
  decision 21: Active status only, no further criteria, evaluated as of
  the freeze). Once frozen, this table — not a live query against
  `Member.Status` — is the authoritative eligibility source for voting,
  so a member's status changing mid-election doesn't retroactively
  add/remove their eligibility.
- Migration: `AddElectionEligibleVoterSchema` — one table, unique index
  on `(ElectionId, MemberId)`, `Restrict` FKs.
- Tests: three-layer pattern, plus a freeze-snapshot test (member
  deactivated after freeze stays eligible; member activated after freeze
  is not retroactively added).

### BIMSS-079 — `ElectionParticipation`/`ElectionBallot`/`ElectionVote` schema (Not started)

**This is the ballot-secrecy core — read `AGENTS.md`'s Election rules
again before starting.** No direct `MemberId -> CandidateId` relationship
anywhere in this schema, structurally, not just by convention.

- `ElectionParticipation` — proves a member voted: `(ElectionId,
  MemberId)`, `VotedAtUtc`. **Unique index on `(ElectionId, MemberId)` at
  the database level** — this is what makes one-ballot-per-voter a real
  constraint under concurrent requests, not just an application-level
  check race-prone under load. No reference to the ballot content.
- `ElectionBallot` — the anonymous ballot itself: `ElectionId`,
  `CastAtUtc`. **Deliberately no `MemberId` column at all** — the
  application service that casts a vote writes both
  `ElectionParticipation` and `ElectionBallot` in the same atomic
  transaction, but nothing persisted afterward links a specific ballot
  back to the member who cast it.
- `ElectionVote` — one row per `(BallotId, PositionId, CandidateId)`
  selected on that ballot. A position left blank (confirmed decision 20:
  abstention allowed) simply has no rows for that position on that
  ballot — no explicit "abstain" marker needed.
- Migration: `AddElectionVotingSchema` — three tables. `ElectionBallot`/
  `ElectionVote` cascade together; `ElectionParticipation` is otherwise
  unrelated to them at the FK level, by design.
- Tests: three-layer pattern, **plus** a dedicated test asserting the
  schema itself provides no queryable path from `Member` to the
  candidates they selected (the actual concurrency/race test for the
  unique constraint belongs with BIMSS-082's application-service tests,
  since InMemory doesn't reliably simulate real concurrent-insert races —
  same reasoning as `docs/PHASE1_BACKLOG.md`'s "Environment notes" on
  real-SQL-Server-only constraint tests).

### BIMSS-080 — `ElectionFinalizedResult` schema (Not started)

- `ElectionFinalizedResult` — one row per `(PositionId, CandidateId)`
  with the final vote count, written **once**, only by the finalization
  action (BIMSS-083). Immutable. This is what "published election
  results must come from finalized persisted results" (`AGENTS.md`)
  means concretely: a report/results screen reads this table, never
  computes a live tally from `ElectionVote` on each request.
- Migration: `AddElectionFinalizedResultSchema` — one table.
- Tests: three-layer pattern.

### BIMSS-081 — Election setup service (Not started)

Gated on `Permission.Election.Manage`. Create election, define positions
and seat counts, add candidates, and freeze the eligible voter list
(materializing `ElectionEligibleVoter` from current `Member.Status ==
Active`, per confirmed decision 21). Validates configuration before
allowing the election to move to `Scheduled`/`Open` (e.g. every position
has at least one candidate, `SeatCount` doesn't exceed candidate count is
NOT required — fewer candidates than seats is a legitimate configuration,
just don't allow zero candidates for a position with seats to fill).

### BIMSS-082 — Voting service (Not started)

**Extra review target, per `AGENTS.md`.** Gated on `Permission.
Election.Vote`. Implements `docs/DOMAIN_WORKFLOWS.md` #8 exactly: check
election is open, check `ElectionEligibleVoter` membership, check no
existing `ElectionParticipation` row, accept ballot selections (validate
each selected candidate actually belongs to the position voted for, and
a multi-seat position's selection count doesn't exceed its `SeatCount`),
then atomically insert `ElectionParticipation` + `ElectionBallot` +
`ElectionVote` rows in one transaction — relying on the unique
`(ElectionId, MemberId)` database constraint (BIMSS-079) to make a
double-submit race safe, not just an application-level pre-check. Returns
a non-secret receipt/reference proving participation, never the ballot
content. Needs a **real concurrency test** (two near-simultaneous vote
attempts from the same member, expecting exactly one success) — this is
exactly the kind of test `docs/PHASE1_BACKLOG.md`'s "Environment notes"
section describes running against the CI SQL Server service container,
not the InMemory provider.

### BIMSS-083 — Election closing/finalization + tally service (Not started)

Gated on `Permission.Election.Finalize` — a distinct, explicitly audited
action from `Manage`, per `docs/DOMAIN_WORKFLOWS.md` #9 ("authorized
finalization" as its own step). Prevents new ballots once closed
(`Election.Status -> Closed`), tallies `ElectionVote` rows per
`(PositionId, CandidateId)`, determines winners (top-`SeatCount` by
count, per confirmed decision 19), and writes `ElectionFinalizedResult`
rows once. `Election.Status -> Finalized` is terminal.

### BIMSS-084 — Admin UI: election setup + monitoring (Not started)

Election configuration screens (positions/seats/candidates, voter-list
freeze action) and an in-progress monitoring view. Per `AGENTS.md`: **do
not** show live candidate totals while voting is open unless Buklod
explicitly adopts that policy — this phase's confirmed decisions didn't
address that question, so default to participation-count-only monitoring
(how many eligible voters have voted, not who they voted for) until
that's explicitly asked and answered.

### BIMSS-085 — Self-service UI: voting screen (Not started)

Implements the member-facing half of `docs/DOMAIN_WORKFLOWS.md` #8:
present open elections the member is eligible for, ballot entry
(respecting each position's `SeatCount` and allowing abstention per
confirmed decision 20), a review step before submission, then a
confirmation/receipt screen. Never allow a resubmission attempt to look
like it "worked" silently — a second attempt must surface the
already-voted state clearly (server-enforced regardless of what the UI
shows, per every other module's "server-side authorization" rule).

### BIMSS-086 — Results UI (Not started)

Admin finalize action (triggers BIMSS-083) and a published results view
reading only `ElectionFinalizedResult` (BIMSS-080) — never a live tally.
Gated appropriately: finalize on `Permission.Election.Finalize`, viewing
published results on whatever general viewing permission is appropriate
once results exist (likely no special permission needed for finalized,
published results — decide at implementation time based on whether
Buklod wants results member-visible or officer-only).

### BIMSS-087 — Synthetic election seed data (Not started)

Optional/dev-convenience, same role as prior phases' seed tasks — a
seeded `Election` in `Open` status with positions (including at least one
multi-seat position), candidates, a frozen eligible voter list, and a few
synthetic ballots cast (respecting real ballot-secrecy mechanics — no
shortcuts that link a seeded ballot back to a member outside the same
atomic-insert pattern BIMSS-082 uses), so BIMSS-084/085/086's screens
have something real to render without waiting for a live election.

## Looking beyond Phase 5

Elections was the last module with a specific blocking Buklod question.
Phase 6 (Notifications, Reports, Audit viewer) isn't blocked the same
way, but still benefits from confirming scope preferences (email
integration? which reports/exports are actually wanted?) before drafting
`docs/PHASE6_BACKLOG.md` — see `docs/DEVELOPMENT_ROADMAP.md` for the
current phase-to-module mapping.
