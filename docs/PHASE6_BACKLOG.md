# BIMSS Phase 6 Backlog

This is the authoritative, in-repo tracker for Phase 6 ("Notifications,
Reports, and Audit viewer") work — task status and what's next. Same
convention as `docs/PHASE1_BACKLOG.md` through `docs/PHASE5_BACKLOG.md`;
read `PHASE1_BACKLOG.md`'s "Secrets convention" and "Environment notes"
for still-current project-wide conventions not repeated here — the
Secrets convention matters directly for this phase's SMTP credentials.

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

**Current state (2026-08-18): Phase 6 has not started implementation.**
Phases 1–5 are Done/scoped — see `docs/PHASE1_BACKLOG.md` through
`docs/PHASE5_BACKLOG.md`. Unlike those phases, Phase 6 wasn't blocked by
a numbered open question — its scope preferences were confirmed directly
(see "Confirmed decisions (Buklod, 2026-08-18, Notifications & Reports)"
in `docs/DATA_DICTIONARY.md`): email delivery needed, both announcements
and personal notifications in scope, Membership and Finance reports built
together.

**This phase can realistically only finish once Phases 3–5 have real
data to report on.** Membership reports can start early (Membership data
has existed since Phase 1), but Finance reports need Contributions/Loans
data to exist first, and event-triggered notifications (BIMSS-091) need
the events they're hooking — a loan status change, an election opening —
to exist in the modules that raise them. The schema/infrastructure tasks
below (BIMSS-088–090) don't have this constraint and can start any time.

Next task to pick up: **BIMSS-088**.

## Phase 6A — Notifications, Reports, Audit viewer

| ID | Title | Status | Dependencies |
|---|---|---|---|
| BIMSS-088 | `Announcement` schema | Not started | BIMSS-004 |
| BIMSS-089 | `Notification`/`EmailOutbox` schema | Not started | BIMSS-004 |
| BIMSS-090 | Email delivery service (SMTP + outbox processing) | Not started | BIMSS-089 |
| BIMSS-091 | Notification triggers (wire existing module events) | Not started | BIMSS-089, BIMSS-043, BIMSS-071 (Phase 4), BIMSS-083 (Phase 5) |
| BIMSS-092 | Admin UI: compose/manage announcements | Not started | BIMSS-088, BIMSS-047 |
| BIMSS-093 | Self-service UI: notification center + announcements | Not started | BIMSS-089, BIMSS-039 |
| BIMSS-094 | Audit log viewer (admin UI) | Not started | BIMSS-007, BIMSS-047 |
| BIMSS-095 | Membership reports | Not started | BIMSS-023, BIMSS-047 |
| BIMSS-096 | Finance reports (Contributions + Loans) | Not started | Phase 3, Phase 4, BIMSS-047 |
| BIMSS-097 | Synthetic notification/announcement seed data | Not started | BIMSS-088, BIMSS-089 |

### BIMSS-088 — `Announcement` schema (Not started)

- `Announcement` (`Bimss.Domain/Notifications/` — first content in a new
  Notifications bounded module) — `Title`, `Body`, `PublishedAtUtc`,
  `AuthorUserId` (no FK to `AspNetUsers`, same established reasoning as
  every other actor reference), optional audience scope (e.g. all
  members vs. a specific `OfficeUnit` — decide the exact scoping
  mechanism at implementation time; simplest starting point is
  all-members-only with scoping added later if Buklod asks for it).
- Migration: `AddAnnouncementSchema` — one table.
- Tests: three-layer pattern (unit/configuration/integration).

### BIMSS-089 — `Notification`/`EmailOutbox` schema (Not started)

- `Notification` (`Bimss.Domain/Notifications/`) — personal, per-member:
  `MemberId`, `Type` (e.g. `UpdateRequestDecided`, `LoanStatusChanged`,
  `ElectionOpen` — a small enum/string set, extend as new trigger types
  get added in BIMSS-091), `Message`, `RelatedEntityType`/
  `RelatedEntityId` (nullable — lets the UI deep-link, e.g. straight to
  the update request that was decided), `CreatedAtUtc`, `ReadAtUtc`
  (nullable — set when the member views it).
- `EmailOutbox` (`Bimss.Domain/Notifications/`) — `ToAddress`,
  `Subject`, `Body`, `Status` (`Pending`/`Sent`/`Failed`), `SentAtUtc`,
  `RetryCount`. A `Notification` may optionally queue a corresponding
  `EmailOutbox` row (not every in-app notification needs an email) —
  decide per trigger type in BIMSS-091, not globally here.
- Migration: `AddNotificationSchema` — two tables.
- Tests: three-layer pattern.

### BIMSS-090 — Email delivery service (Not started)

SMTP configuration follows the existing "Secrets convention"
(`docs/PHASE1_BACKLOG.md`) exactly — real SMTP host/credentials go in
the git-ignored real `appsettings.json`/`appsettings.Production.json`,
never committed; add the corresponding placeholder section to
`appsettings*.json.example`. A background/scheduled process (exact
mechanism TBD at implementation time — a hosted service polling
`EmailOutbox` for `Pending` rows is the simplest fit for this
architecture, no message broker per `AGENTS.md`) sends queued emails and
updates `Status`/`SentAtUtc`/`RetryCount`. Never log email body content
if it could contain sensitive member data — mask per
`docs/SECURITY_AND_PRIVACY.md`'s diagnostic-log rules.

### BIMSS-091 — Notification triggers (Not started)

Wires existing (and Phase 3/4/5) application services to call into a
shared `INotificationService` (`Bimss.Application/Notifications/`) at
the points `docs/DATA_DICTIONARY.md`'s confirmed decision 23 names:
update-request approved/rejected (BIMSS-043), loan application status
changes (Phase 4's BIMSS-071/072), election opening/closing (Phase 5's
BIMSS-081/083). Each call site creates a `Notification` (and optionally
queues an `EmailOutbox` row) — simple in-process calls from the
triggering service, not an event bus/message broker, consistent with
`AGENTS.md`'s architecture rules. This task necessarily touches code in
other phases' modules; keep each addition minimal (one notification-
service call at the existing decision point) rather than restructuring
those services.

### BIMSS-092 — Admin UI: compose/manage announcements (Not started)

Gated on an appropriate permission (likely `Permission.Membership.Manage`
or a new `Permission.Announcement.Manage` if scope grows — decide at
implementation time whether announcements need their own permission or
can reuse an existing one). Compose, publish, and view past
announcements.

### BIMSS-093 — Self-service UI: notification center + announcements (Not started)

Extends the member dashboard: an in-app notification list (read/unread
state, deep links via `RelatedEntityType`/`RelatedEntityId`) and a
surfaced view of active announcements.

### BIMSS-094 — Audit log viewer (Not started)

Gated on `Permission.Audit.View` (reserved since BIMSS-006, seeded to
the `Auditor` role since BIMSS-013 — this is the first screen that
actually uses it). A query service + admin UI over the existing
`AuditEvent` table (BIMSS-007) — filterable by actor/action/object
type/date range, following the same projection-for-lists discipline
(`AGENTS.md`) as every other admin grid in the codebase. No new schema —
`AuditEvent` has existed since Phase 1.

### BIMSS-095 — Membership reports (Not started)

Gated on `Permission.Report.ViewMembership` (reserved since BIMSS-006).
Roster/summary reports over existing Membership data — exact report list
TBD with Buklod at implementation time (e.g. active-member roster by
office unit, verification-status summary). Every export audited via
`IAuditLogger`, per `docs/DOMAIN_WORKFLOWS.md` #10 ("Sensitive
report/export" — check permission, apply scope filters, generate,
audit).

### BIMSS-096 — Finance reports (Not started)

Gated on `Permission.Report.ViewFinance`. Reports over the Contributions
(Phase 3) and Loans (Phase 4) ledgers — exact report list TBD with
Buklod (e.g. contribution collection summary by period, loan portfolio/
aging summary). Same audit-on-export discipline as BIMSS-095. Blocked in
practice on Phases 3–4 existing with real data, even though nothing
prevents starting the schema-adjacent groundwork earlier.

### BIMSS-097 — Synthetic notification/announcement seed data (Not started)

Optional/dev-convenience, same role as every prior phase's seed task —
a few seeded `Announcement`s and per-member `Notification`s (mixed
read/unread) so BIMSS-092/093's screens have something to render.

## Looking beyond Phase 6

Phase 7 (Hardening / UAT / production) is the last phase in
`docs/DEVELOPMENT_ROADMAP.md` and is intentionally left without a
detailed `PHASE7_BACKLOG.md` for now — its actual task list depends on
what's been built by the time it starts (which modules need a security
review, what the accessibility/performance gaps turn out to be), so
drafting concrete BIMSS-IDs for it now would mostly be guessing. Revisit
once Phases 3–6 are closer to done; the checklist in
`docs/DEVELOPMENT_ROADMAP.md`'s Phase 7 section is the current best
scope statement.

Two open questions remain in `docs/DATA_DICTIONARY.md`'s "Questions to
confirm with Buklod before final schema" list (retirees/honorary members;
record-retention rules after membership ends) — neither currently blocks
a specific phase's schema, but get them answered before they become
load-bearing for something.
