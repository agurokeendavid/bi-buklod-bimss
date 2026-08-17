# BIMSS Development Roadmap

**Numbering note (2026-08-17):** this roadmap was written before Phase 1
started and originally numbered phases 0–8. Actual implementation merged
the original Phase 0 (Discovery), Phase 1 (platform foundation), and Phase
2 (Membership/migration) into a single `docs/PHASE1_BACKLOG.md` — discovery
happened continuously as "Confirmed decisions" sections in
`docs/DATA_DICTIONARY.md` rather than as an up-front separate phase, and
platform foundation + Membership shipped as one connected effort (see that
file's Phase 1A–1E sub-phases). Everything from the original Phase 3
onward is renumbered down by one to match the `docs/PHASEn_BACKLOG.md`
file names actually in use:

| This roadmap (original) | Actual backlog file | Status |
|---|---|---|
| Phase 0 — Discovery | folded into Phase 1 | Done |
| Phase 1 — Platform foundation | `PHASE1_BACKLOG.md` (Phase 1A) | Done |
| Phase 2 — Membership and migration | `PHASE1_BACKLOG.md` (Phase 1B–1E) | Done |
| Phase 3 — Beneficiaries | `PHASE2_BACKLOG.md` | Not started (drafted) |
| Phase 4 — Contributions | `PHASE3_BACKLOG.md` (not yet written) | Not started |
| Phase 5 — Loans | `PHASE4_BACKLOG.md` (not yet written) | Not started |
| Phase 6 — Elections | `PHASE5_BACKLOG.md` (not yet written) | Not started |
| Phase 7 — Notifications and reporting | `PHASE6_BACKLOG.md` (not yet written) | Not started |
| Phase 8 — Hardening / UAT / production | `PHASE7_BACKLOG.md` (not yet written) | Not started |

The section headers below use the **renumbered (actual)** scheme. Each
`Not started` phase needs its own round of Buklod business-question
confirmation — see `docs/DATA_DICTIONARY.md`'s "Questions to confirm with
Buklod before final schema" — before its detailed `PHASEn_BACKLOG.md` gets
written, the same way Phase 2's did.

## Module coverage

`docs/PROJECT_CONTEXT.md`'s 12 core modules, mapped to phase and status:

| # | Module | Phase | Status |
|---|---|---|---|
| 1 | Identity and Access | 1 | Done |
| 2 | Membership | 1 | Done |
| 11 | Reference Data | 1 (extended per-module as needed) | Done for Phase 1's needs; each later phase adds its own reference tables |
| 12 | Migration/Imports | 1 (existing members) | Done for members; beneficiary/child backfill is Phase 2 (BIMSS-054/055) |
| 9 | Documents | 1 (metadata + storage + verification gate) | Foundation done (BIMSS-021); no dedicated upload workflow beyond that yet — expect incremental additions inside Loans (supporting docs) and Elections rather than a standalone phase |
| 10 | Audit | 1 (logging foundation) | Foundation done (BIMSS-007); no admin-facing log viewer UI yet — planned for Phase 6 alongside Reports |
| 3 | Beneficiaries | 2 | Not started — scoped in `PHASE2_BACKLOG.md` |
| 4 | Contributions | 3 | Not started |
| 5 | Loans | 4 | Not started |
| 6 | Elections | 5 | Not started |
| 7 | Notifications and Announcements | 6 | Not started |
| 8 | Reports | 6 | Not started |

Notifications and Reports are grouped into one phase (6) because neither
is a standalone data domain — DOMAIN_WORKFLOWS.md's workflow #10
("Sensitive report/export") reads across whatever data exists by that
point, and announcements/notifications are plumbing that mostly gets
triggered by events in the other modules (update-request decided, loan
status changed, election opened). Both could be pulled earlier and threaded
through each phase incrementally instead, if Buklod wants member-facing
notifications sooner than Phase 6 — worth a explicit decision when Phase 3
starts, not assumed here.

## Phase 1 — Platform foundation + Membership (Done)

See `docs/PHASE1_BACKLOG.md` for full detail (BIMSS-001 through BIMSS-047,
all Done as of 2026-08-17). Covered: .NET 10 solution, SQL Server + EF Core
10, JWT authentication, permission policies, audit foundation, global error
handling, validation conventions, CI, synthetic seed data, the Next.js
frontend, and the full Membership domain — master data, employment/
contact/address/education/eligibility, admin grid, Excel import
(batches/staging/errors), update requests, and member self-service.

## Phase 2 — Beneficiaries (Not started, scoped)

See `docs/PHASE2_BACKLOG.md` (BIMSS-048 through BIMSS-056). Buklod's
beneficiary questions are confirmed (`docs/DATA_DICTIONARY.md`,
2026-08-17): no shares, no cap, officer approval required. Covered:
`MemberBeneficiary` schema, a dedicated change-request/approval pipeline,
admin + self-service UI, and backfill of already-imported beneficiary/
child data that had nowhere to land until this phase's schema exists.

## Phase 3 — Contributions (Not started)

Blocked on one open question: *"What contribution amount/rules vary by
member or year?"* (`docs/DATA_DICTIONARY.md`, question 2). Get that answer
before writing `PHASE3_BACKLOG.md`, same process as Phase 2.

Proposed scope, from `docs/DATA_DICTIONARY.md`'s "Proposed core database
tables" (`ContributionBatches`/`Contributions`/`ContributionAdjustments`)
and `docs/DOMAIN_WORKFLOWS.md` #4:

- Contribution batch import/posting (mirrors the BIMSS-033–038 import
  pattern already built for Membership — staging, validation, error
  reporting, before anything touches the ledger).
- Immutable contribution transactions — **never** January–December columns
  per `AGENTS.md`'s explicit rule; corrections go through a traceable
  adjustment/reversal, never an overwrite of a posted row.
- Member contribution ledger/history view (admin + self-service).
- `Permission.Contribution.ViewSelf`/`Manage` already exist in the
  Permission catalog (reserved since BIMSS-006, unused so far) — reuse
  them rather than adding new ones unless the confirmed rules demand a
  finer split (e.g. a separate posting-vs-adjustment permission).
- Finance reports are Phase 6's concern, not this phase's — this phase
  is the ledger itself.

## Phase 4 — Loans (Not started)

Blocked on: *"What official loan products, interest rules, terms,
penalties, and eligibility rules exist?"* (question 3). This is the
largest remaining module — get the product/calculation rules nailed down
before scoping, not during.

Proposed scope, from `docs/DATA_DICTIONARY.md`'s proposed tables
(`LoanTypes`/`LoanApplications`/`LoanApplicationStatusHistory`/
`LoanApprovals`/`Loans`/`LoanPaymentSchedules`/`LoanPayments`/
`LoanAdjustments`) and `docs/DOMAIN_WORKFLOWS.md` #5–6:

- Loan type/product configuration (reference-data-like, but with
  calculation rules attached — interest, terms, penalties).
- Application lifecycle: `Draft -> Submitted -> For Review -> For
  Approval -> Approved/Disapproved -> For Release -> Released -> Active ->
  Fully Paid/Closed` (plus `Cancelled`/`Returned for Correction`), each
  transition server-validated with actor/timestamp/remarks recorded, per
  `AGENTS.md`'s Loan rules.
- Payment schedule generation and payment posting; balances computed
  **server-side only**, never in browser JavaScript, per `AGENTS.md`.
- `Permission.Loan.Apply`/`ViewSelf`/`Review`/`Approve`/`Release` already
  exist in the catalog (reserved since BIMSS-006) — the four-permission
  split already anticipates a review/approve/release pipeline distinct
  from Contributions' simpler manage/view split.
- Never delete a released loan or posted payment as a normal correction
  method — same adjustment/reversal discipline as Contributions.
- Extra concurrency/integrity tests required for financial posting, per
  `AGENTS.md`'s testing requirements — budget for this explicitly.

## Phase 5 — Elections (Not started)

Blocked on: *"What election positions and voting rules apply, including
abstention and number of selections per position?"* (question 4).
Self-contained relative to Contributions/Loans (doesn't depend on
financial data), so — pending Buklod's answer — this phase could run in
parallel with Phase 3/4 rather than strictly after them.

Proposed scope, from `docs/DATA_DICTIONARY.md`'s proposed tables
(`Elections`/`ElectionPositions`/`ElectionCandidates`/
`ElectionEligibleVoters`/`ElectionParticipation`/`ElectionBallots`/
`ElectionVotes`/`ElectionFinalizedResults`) and `docs/DOMAIN_WORKFLOWS.md`
#7–9:

- Election setup: positions, candidates, eligibility rules, voter-list
  freeze.
- Voting: server-enforced eligibility, one-ballot-per-voter with
  **database-level** race-condition protection (a unique constraint on
  `(ElectionId, MemberId)` in `ElectionParticipation`, not just an
  application check), and participation records kept structurally
  separate from ballot contents — **no direct `MemberId -> CandidateId`
  relationship anywhere**, per `AGENTS.md`'s election rules.
- No live candidate totals exposed while voting is open, unless Buklod
  explicitly adopts that policy.
- Closing/finalization as its own auditable action; published results
  come only from finalized persisted results, never computed live from
  ballots on each report request.
- `Permission.Election.Vote`/`Manage`/`Finalize` already exist in the
  catalog (reserved since BIMSS-006).
- Per `AGENTS.md`: "Election integrity code requires extra tests and
  review" — plan for a dedicated review pass on the ballot-secrecy and
  concurrency code, not just the standard test suite.

## Phase 6 — Notifications, Reports, and Audit viewer (Not started)

Not blocked on a specific Buklod business question the way 3–5 are, but
worth confirming scope preferences (email integration? which
reports/exports are actually wanted?) before drafting its backlog — see
"Module coverage" above for why Notifications and Reports are bundled
here rather than each getting a phase.

Proposed scope, from `docs/DOMAIN_WORKFLOWS.md` #10 and the original
roadmap's Phase 7:

- Announcements + in-app notifications; email integration only if/when
  Buklod approves it (no email-sending infrastructure exists yet — noted
  as a gap back in BIMSS-005).
- Dashboards and authorized exports, permission-gated
  (`Permission.Report.ViewMembership`/`ViewFinance`, reserved since
  BIMSS-006) and audited on every sensitive export, per
  `docs/DOMAIN_WORKFLOWS.md` #10.
- Admin-facing audit log viewer — `Permission.Audit.View` exists and is
  seeded to the `Auditor` role (BIMSS-013), but nothing renders
  `AuditEvent` rows yet; this is the natural home for that screen once
  Contributions/Loans/Elections give audit history something substantial
  to show.

## Phase 7 — Hardening / UAT / production (Not started)

Unchanged in substance from the original roadmap's Phase 8, renumbered:

- role/permission matrix review
- vulnerability review
- accessibility review
- performance tests
- backup/restore test
- UAT
- deployment runbook
- production readiness checklist
