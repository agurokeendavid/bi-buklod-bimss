# BIMSS Phase 4 Backlog

This is the authoritative, in-repo tracker for Phase 4 ("Loans") work —
task status and what's next. Same convention as `docs/PHASE1_BACKLOG.md`/
`docs/PHASE2_BACKLOG.md`/`docs/PHASE3_BACKLOG.md`; read `PHASE1_BACKLOG.md`'s
"Secrets convention" and "Environment notes" for still-current project-wide
conventions not repeated here.

Read `AGENTS.md` and the rest of `docs/` first for the standing rules this
backlog was built against — this module especially: `AGENTS.md`'s "Loan
rules" section is not optional flavor text. This file tracks task-level
progress only; it doesn't restate architecture or security rules that live
elsewhere.

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
5. This is the largest and most financially sensitive remaining module —
   per `CLAUDE.md`/`AGENTS.md`, prefer Claude Code's planning behavior
   before editing, and budget extra time for concurrency/integrity tests
   on the payment-posting and status-transition code specifically.

**Current state (2026-08-17): Phase 4 has not started implementation.**
Phases 1–3 are Done/scoped — see `docs/PHASE1_BACKLOG.md`,
`docs/PHASE2_BACKLOG.md`, `docs/PHASE3_BACKLOG.md`. Buklod confirmed the
blocking Loans question on 2026-08-17 (see "Confirmed decisions (Buklod,
2026-08-17, Loans)" in `docs/DATA_DICTIONARY.md`): one generic loan
product for now, flat-rate interest on original principal, co-maker
required, payroll-deduction repayment, flat penalty per missed payment,
fixed max amount per product (not tied to contributions), one active loan
per member at a time.

Next task to pick up: **BIMSS-066**.

## Phase 4A — Loans Module

| ID | Title | Status | Dependencies |
|---|---|---|---|
| BIMSS-066 | `LoanType` schema (product configuration) | Not started | BIMSS-004 |
| BIMSS-067 | `LoanApplication`/`LoanApplicationStatusHistory`/`LoanApproval` schema | Not started | BIMSS-015, BIMSS-066 |
| BIMSS-068 | `Loan`/`LoanPaymentSchedule` schema | Not started | BIMSS-067 |
| BIMSS-069 | `LoanPayment`/`LoanAdjustment` schema | Not started | BIMSS-068 |
| BIMSS-070 | Member submits loan application | Not started | BIMSS-067, BIMSS-039 |
| BIMSS-071 | Officer review/approve/disapprove loan application | Not started | BIMSS-067, BIMSS-030 |
| BIMSS-072 | Loan release service (application → active loan) | Not started | BIMSS-068, BIMSS-071 |
| BIMSS-073 | Loan payment batch ingestion + posting + penalty service | Not started | BIMSS-069, BIMSS-072, BIMSS-061 |
| BIMSS-074 | Admin UI: application review queue, loan detail, payment import | Not started | BIMSS-071, BIMSS-073, BIMSS-047 |
| BIMSS-075 | Self-service UI: apply, track status, view active loan | Not started | BIMSS-070, BIMSS-039 |
| BIMSS-076 | Synthetic loan seed data | Not started | BIMSS-069 |

### BIMSS-066 — `LoanType` schema (Not started)

- `LoanType` (`Bimss.Domain/Loans/` — first content in a new Loans
  bounded module, sibling to `Bimss.Domain/Membership/` and
  `Bimss.Domain/Contributions/`) — `Name`, `InterestRatePercent`
  (`decimal`, flat rate applied to original principal per the confirmed
  decision), `MaxAmount` (`decimal`, fixed cap), `TermMonths` (default/
  max repayment term), `PenaltyAmount` (`decimal`, flat fee per missed
  payment). All monetary/rate fields `decimal`, never `float`/`real`,
  per `AGENTS.md`. `IsActive` — deactivated, never deleted, once a
  `LoanApplication` references it, same pattern as every Phase 1
  reference-data entity.
- One seeded row to start (Buklod's actual current product), but the
  table itself is what makes adding a second product later a data change,
  not a schema change.
- Migration: `AddLoanType` — one table.
- Tests: unit (constructor guards), configuration (metadata inspection),
  integration (round-trip, `SetActive`).

### BIMSS-067 — `LoanApplication`/`LoanApplicationStatusHistory`/`LoanApproval` schema (Not started)

Implements the pre-release half of `docs/DOMAIN_WORKFLOWS.md` #5's
lifecycle: `Draft -> Submitted -> For Review -> For Approval -> Approved
/ Disapproved`, plus `Cancelled`/`Returned for Correction` from a
non-terminal state. Schema/domain-rule only.

- `LoanApplication` (`Bimss.Domain/Loans/`) — `MemberId`, `LoanTypeId`,
  `RequestedAmount` (must not exceed `LoanType.MaxAmount` — a domain
  invariant enforced in the constructor/submit method, throwing
  `DomainValidationException` on violation), `CoMakerMemberId` (required
  FK to another `Member` — the confirmed co-maker rule), `TermMonths`.
  State machine guarded the same way as `Member`/`ImportBatch`/
  `MemberUpdateRequest`: `ConflictException` on an invalid transition.
  Every transition requires actor/timestamp/remarks-where-required, per
  `AGENTS.md`'s Loan rules — this is not optional the way some other
  modules' remarks are.
- `LoanApplicationStatusHistory` — one row per transition (mirrors
  `MemberStatusHistory`'s `internal`-constructor, owning-aggregate-only
  creation pattern).
- `LoanApproval` — one row per reviewer/approver decision, distinct from
  the status timeline because `Permission.Loan.Review` and
  `Permission.Loan.Approve` are two separate permissions (reserved since
  BIMSS-006) representing two separate steps, possibly two different
  people: a review decision and a later approval decision. Captures
  actor, decision, remarks, timestamp per step.
- The one-active-loan-at-a-time rule (confirmed decision 17) is an
  **application-service** concern (BIMSS-070), not enforced inside this
  schema task — same "schema/domain-rule only, no use case" scope
  discipline as every prior schema task.
- Migration: `AddLoanApplicationSchema` — three tables, `Restrict` FKs to
  `Member`/`LoanType`/co-maker `Member`.
- Tests: three-layer pattern (unit/configuration/integration), plus a
  specific test for the max-amount-exceeded rejection and each
  `LoanApproval` step recording correctly.

### BIMSS-068 — `Loan`/`LoanPaymentSchedule` schema (Not started)

- `Loan` (`Bimss.Domain/Loans/`) — the active loan account, created once
  from an approved `LoanApplication` (BIMSS-072's job, not this task's —
  schema only here). `PrincipalAmount`, `InterestAmount` (computed once
  at creation per the flat-rate rule — `Principal × Rate × Term`),
  `TotalAmount` (`Principal + Interest`), `TermMonths`,
  `SourceApplicationId`. Status: `Active -> Fully Paid/Closed`, matching
  the back half of `docs/DOMAIN_WORKFLOWS.md` #5.
- `LoanPaymentSchedule` — one row per installment (`InstallmentNumber`,
  `DueDate`, `AmountDue`, `Status`: `Pending`/`Paid`/`Missed`). Generated
  once at `Loan` creation by dividing `TotalAmount` across `TermMonths`
  equal installments (flat-rate loans have equal installments by
  definition — no recomputation needed later, unlike diminishing-balance
  amortization).
- Migration: `AddLoanSchema` — two tables, `Restrict` FK to
  `LoanApplication`.
- Tests: three-layer pattern, plus a schedule-generation correctness
  test (installments sum exactly to `TotalAmount`, no rounding drift
  left unaccounted for — decide and document the rounding rule, e.g.
  "last installment absorbs the remainder").

### BIMSS-069 — `LoanPayment`/`LoanAdjustment` schema (Not started)

- `LoanPayment` (`Bimss.Domain/Loans/`) — immutable ledger transaction,
  same "never delete a released loan or posted payment as a normal
  correction method" rule as `AGENTS.md`'s Loan rules state explicitly.
  `LoanId`, `ScheduleInstallmentId` (nullable — a payment might not
  cleanly match one installment), `Amount`, `PostedAtUtc`,
  `SourceBatchId` (nullable FK, mirroring `Contribution.SourceBatchId`).
- `LoanAdjustment` — traceable correction/penalty-application record:
  references the affected `Loan`/`LoanPayment`, `AdjustmentAmount`
  (signed), `Reason` (required — a missed-payment penalty charge is
  itself one of these, with `Reason` set accordingly, applying the
  confirmed flat `LoanType.PenaltyAmount`).
- Balances are computed **server-side only** from these persisted
  transactions — never authoritative in browser JavaScript, per
  `AGENTS.md`. This task defines the data the balance calculation reads;
  the calculation itself is BIMSS-073/074's concern.
- Migration: `AddLoanLedger` — two tables.
- Tests: three-layer pattern, plus a balance-calculation correctness
  test across a payment + penalty adjustment.

### BIMSS-070 — Member submits loan application (Not started)

Mirrors BIMSS-042's split (schema lands first, then submit). Gated on
`Permission.Loan.Apply`. Enforces the confirmed one-active-loan-at-a-time
rule here (application-service level, querying for an existing `Active`
`Loan` before allowing a new `LoanApplication`) and the co-maker must be
a distinct, `Active` member (not the applicant themselves).

### BIMSS-071 — Officer review/approve/disapprove loan application (Not started)

Mirrors BIMSS-043. Two gates matching the two reserved permissions:
`Permission.Loan.Review` for the review step, `Permission.Loan.Approve`
for the approval step — implement as two distinct actions/endpoints, not
one combined "approve" action, since the schema (BIMSS-067) already
models them as separate `LoanApproval` steps potentially taken by
different people.

### BIMSS-072 — Loan release service (Not started)

Gated on `Permission.Loan.Release` (reserved since BIMSS-006, distinct
from `Approve` — releasing funds is its own auditable action per
`docs/DOMAIN_WORKFLOWS.md` #5's "For Release -> Released" step, separate
from the approval decision). Creates the `Loan` + generates its
`LoanPaymentSchedule` from an `Approved` `LoanApplication`. This is the
transition from "approved on paper" to "an active loan account exists" —
keep it a distinct, explicitly audited step rather than folding it into
BIMSS-071's approval action.

### BIMSS-073 — Loan payment batch ingestion + posting + penalty service (Not started)

Mirrors BIMSS-060/061's Contributions batch pattern closely, per the
confirmed payroll-deduction repayment channel — reuse that pattern's
shape (batch → staging → validate → post) rather than inventing a new
one. Posting a payment matches it against the next `Pending`
`LoanPaymentSchedule` installment; marks `Loan` `Fully Paid`/`Closed`
once every installment is settled. A separate scheduled/manual process
(exact trigger TBD at implementation time) applies the confirmed flat
penalty fee (`LoanAdjustment`) to installments that pass their `DueDate`
still `Pending`, marking them `Missed`.

### BIMSS-074 — Admin UI: application review queue, loan detail, payment import (Not started)

Extends the Next.js admin shell: a review queue mirroring the
Update-Requests/Approvals screen pattern from Phase 1E, a loan detail
view (schedule, payments, adjustments, balance), and a payment batch
import screen following BIMSS-062's Contributions import UI pattern.

### BIMSS-075 — Self-service UI: apply, track status, view active loan (Not started)

Extends the member dashboard: an apply-for-a-loan form (gated on
`Permission.Loan.Apply`, blocked client-side *and* server-side if the
member already has an active loan — client-side is UX only, per
`AGENTS.md`'s "never trust client-side validation as authorization"), a
status/history view for submitted applications (mirrors BIMSS-045), and
a "My Loan" view of the active loan's schedule/payments/balance (gated
on `Permission.Loan.ViewSelf`).

### BIMSS-076 — Synthetic loan seed data (Not started)

Optional/dev-convenience, same role as BIMSS-025/056/065 — the seeded
`LoanType`, a few `LoanApplication`s in various states (including one
`Approved`-but-not-yet-released, one full `Active` `Loan` with a partial
payment history and at least one `Missed` installment with its penalty
applied), so BIMSS-074/075's screens have something real to render.

## Looking beyond Phase 4

Elections still has its own blocking Buklod question in
`docs/DATA_DICTIONARY.md`'s "Questions to confirm with Buklod before
final schema" list. Get that answered and write `docs/PHASE5_BACKLOG.md`
the same way this file was written — see `docs/DEVELOPMENT_ROADMAP.md`
for the current phase-to-module mapping.
