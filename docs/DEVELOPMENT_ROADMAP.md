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
| Phase 4 — Contributions | `PHASE3_BACKLOG.md` | Not started (drafted) |
| Phase 5 — Loans | `PHASE4_BACKLOG.md` | Not started (drafted) |
| Phase 6 — Elections | `PHASE5_BACKLOG.md` | Not started (drafted) |
| Phase 7 — Notifications and reporting | `PHASE6_BACKLOG.md` | Not started (drafted) |
| Phase 8 — Hardening / UAT / production | `PHASE7_BACKLOG.md` (not yet written) | Not started |

The section headers below use the **renumbered (actual)** scheme.
Phases 2–6 are now all drafted (`PHASE2_BACKLOG.md` through
`PHASE6_BACKLOG.md`) — each got its own round of Buklod business-question
confirmation first, recorded in `docs/DATA_DICTIONARY.md`'s dated
"Confirmed decisions" sections, before its detailed backlog was written.
Only Phase 7 (Hardening/UAT/production) has no detailed backlog yet — see
its section below for why.

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
| 4 | Contributions | 3 | Not started — scoped in `PHASE3_BACKLOG.md` |
| 5 | Loans | 4 | Not started — scoped in `PHASE4_BACKLOG.md` |
| 6 | Elections | 5 | Not started — scoped in `PHASE5_BACKLOG.md` |
| 7 | Notifications and Announcements | 6 | Not started — scoped in `PHASE6_BACKLOG.md` |
| 8 | Reports | 6 | Not started — scoped in `PHASE6_BACKLOG.md` |
| 13 | Benefits (benefit claims) | 7 (new) | Not started — no backlog yet, blocked on Buklod questions (see below) |

Notifications and Reports are grouped into one phase (6) because neither
is a standalone data domain — DOMAIN_WORKFLOWS.md's workflow #10
("Sensitive report/export") reads across whatever data exists by that
point, and announcements/notifications are plumbing that mostly gets
triggered by events in the other modules (update-request decided, loan
status changed, election opened). Buklod confirmed keeping them together
in Phase 6 rather than pulling them earlier (`docs/DATA_DICTIONARY.md`,
2026-08-18) — see `docs/PHASE6_BACKLOG.md`.

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

## Phase 3 — Contributions (Not started, scoped)

See `docs/PHASE3_BACKLOG.md` (BIMSS-057 through BIMSS-065). Buklod's
Contributions question is confirmed (`docs/DATA_DICTIONARY.md`,
2026-08-17): fixed flat rate (effective-dated, same for every member),
one contribution type only, pure ledger with no expected-vs-actual
tracking. Covered: `ContributionRate`, batch import/staging/posting
(mirroring BIMSS-033–038's Membership import pattern), the immutable
`Contribution` ledger with a traceable `ContributionAdjustment`
correction path, and admin + self-service ledger views.

## Phase 4 — Loans (Not started, scoped)

See `docs/PHASE4_BACKLOG.md` (BIMSS-066 through BIMSS-076) — the largest
remaining module. Buklod's Loans question is confirmed
(`docs/DATA_DICTIONARY.md`, 2026-08-17): one generic loan product for
now, flat-rate interest on original principal, required co-maker,
payroll-deduction repayment, flat penalty per missed payment, fixed max
amount per product, one active loan per member at a time. Covered:
`LoanType` product configuration, the full application lifecycle
(`Draft -> ... -> Approved/Disapproved -> Released -> Active -> Fully
Paid/Closed`) with separate review/approval/release steps matching the
`Permission.Loan.Review`/`Approve`/`Release` split already reserved since
BIMSS-006, payment schedule generation, payroll-deduction payment
posting (reusing the Contributions batch-ingestion shape), and
server-side-only balance/penalty computation. Extra concurrency/integrity
tests required for the payment-posting and status-transition code, per
`AGENTS.md`'s testing requirements.

## Phase 5 — Elections (Not started, scoped)

See `docs/PHASE5_BACKLOG.md` (BIMSS-077 through BIMSS-087) — the last
module with a specific blocking Buklod question, now confirmed
(`docs/DATA_DICTIONARY.md`, 2026-08-18): positions configured per
election, multiple seats per position allowed (top-N by votes win),
abstention allowed, eligibility is Active status only as of the
voter-list freeze. Covered: election setup (positions/seats/candidates/
voter-list freeze), voting with database-level one-ballot-per-voter
protection and participation records kept structurally separate from
ballot contents (no direct `MemberId -> CandidateId` relationship
anywhere), closing/finalization as its own auditable action, and
published results sourced only from finalized persisted results — never
computed live from ballots on each request. Per `AGENTS.md`: "Election
integrity code requires extra tests and review" — the backlog flags
BIMSS-079/082 specifically for a dedicated review pass beyond the
standard test suite.

## Phase 6 — Notifications, Reports, and Audit viewer (Not started, scoped)

See `docs/PHASE6_BACKLOG.md` (BIMSS-088 through BIMSS-097). Not blocked
by a numbered open question the way 3–5 were, but scope preferences are
confirmed (`docs/DATA_DICTIONARY.md`, 2026-08-18): email delivery is
needed (adds SMTP infrastructure — a real gap since BIMSS-005), both
officer-broadcast announcements and personal event-triggered
notifications are in scope, and Membership + Finance reports are built
together in one phase (both `Permission.Report.ViewMembership`/
`ViewFinance`, reserved since BIMSS-006). Also covers the admin-facing
audit log viewer — `Permission.Audit.View` has existed since BIMSS-006/
BIMSS-013 with nothing rendering `AuditEvent` rows yet. Finance reports
in particular are practically gated on Phases 3–4 existing with real
data, even though the notification/schema groundwork (BIMSS-088–090)
doesn't depend on that and can start any time — see the backlog's
"Current state" note.

## Phase 7 — Benefits (benefit claims) (Not started, not yet scoped)

**New module, not in the original roadmap** — surfaced 2026-08-18 while
building out the Claude Design mockup: "Benefit claims" was referenced
across the Reports card grid, the Approvals queue, and the original
prototype's dashboard/member-record tabs, without ever having been
scoped as a real feature anywhere in this repo's docs. Confirmed with
the user 2026-08-18 that this is a real Buklod feature, not decorative
leftover content — a member benefit payout (e.g. death/calamity/
education benefit) with its own claims workflow, likely structurally
similar to Loans (application → review/approval → release → paid) but
against a benefit rather than a loan.

Blocked on business questions that don't have answers yet — same
pattern as Contributions/Loans/Elections before their backlogs got
written. Get these from Buklod before drafting `PHASE7_BACKLOG.md`:

- What benefit types exist (death, calamity, education, retirement,
  other)? One generic type or several with different rules, mirroring
  the Loans "one product for now" question?
- Eligibility rules — membership tenure, contribution standing, event
  documentation (e.g. death certificate) required to file a claim?
- Payout amount rules — fixed per benefit type, tied to
  contribution/membership history, or something else?
- Approval workflow — same review/approve/release shape as Loans, or
  different (e.g. committee vote, single officer)?
- Funding source — paid from the same fund as loans/contributions, or a
  separate benefit fund needing its own ledger?

Until these are answered, don't invent numbers the way the Claude
Design tool's stray "You answered" block did for Loans (see that
phase's mockup session notes) — leave it genuinely open.

## Phase 8 — Hardening / UAT / production (Not started)

Unchanged in substance from the original roadmap's Phase 8, renumbered:

- role/permission matrix review
- vulnerability review
- accessibility review
- performance tests
- backup/restore test
- UAT
- deployment runbook
- production readiness checklist
