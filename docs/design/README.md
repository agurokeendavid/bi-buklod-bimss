# Handoff: BIMSS Membership & Services UI

## Integration status (2026-08-16)

Tokens, the navy sidebar/header shell, and centralized status badges from this handoff are
integrated into `frontend/`. Login, dashboard, the members register, member record, and the
new-member form are re-skinned to match. Deliberate deviations from the literal mockup, all
because the backing API/data doesn't exist yet:

- No live stat row on the login screen (unauthenticated, no public endpoint) and no "Sign in
  as" role selector (production correctly reads the role from JWT claims, as this doc directs).
- Dashboard ships only the four metric tiles — the collections chart, pending-approvals queue,
  recent-activity feed, and membership-by-office panel wait for Phase 4/7 data.
- Members register omits Contributions YTD / Last posted / Office columns per this doc's own
  "wait for Phase 4" note. The bulk bar ships a real "Verify selected" action (reuses the
  existing per-member verify endpoint) and a real client-side "Export CSV"; "Assign officer"
  and "Print IDs" are not implemented, since no such feature exists yet.
- Member record has Personal / Documents / Audit trail tabs only (the panels this doc already
  says to wire in). Contributions / Loans / Benefit claims tabs wait for their own data.
- The new-member form stays a single-page Personal+Employment form, not a 5-step wizard —
  Beneficiaries and a documents-upload step aren't real capabilities of the create-member API
  yet. The single page is split into two `FormSection`s named "Personal information" and
  "Employment information", mirroring the wizard's first two step names. This section/footer
  shell (`frontend/src/components/forms/record-form.tsx`) is now the established pattern for
  future create-record forms across modules — see the "Design system" bullet in
  `.github/instructions/frontend.instructions.md`.

See `docs/PHASE1_BACKLOG.md` for the task-level record of what shipped and what's still open.

## Overview

UI design for the **Buklod Integrated Membership and Services System (BIMSS)** — the Bureau of Immigration Buklod ng Kawani employee association system. The design covers nine screens spanning authentication, an admin dashboard, the membership register, a member record view, a membership application wizard, contributions/remittance posting, an approvals queue, reports, and settings/roles.

The purpose of the system is to replace fragmented and manual Buklod recordkeeping with one secured system for membership and member services.

## About the design files

The files in this bundle are **design references created in HTML**. `BIMSS.dc.html` is an interactive prototype showing intended look, layout, and navigation behavior. **It is not production code and should not be copied into the app.**

The target codebase already exists: `bi-buklod-bimss/frontend` — Next.js (App Router) + React + TypeScript, Tailwind CSS, shadcn/ui, talking to an ASP.NET Core 10 Web API with JWT bearer auth over SQL Server / EF Core 10. **Recreate these designs using that project's established patterns, components, and tokens.** Several screens are already partially built (see "Current state of the codebase" below) — extend them rather than replacing them.

Open `BIMSS.dc.html` in a browser to click through all nine screens. The login screen's "Sign in as" select switches roles; it is a prototype affordance only — in production, JWT claims determine the role.

## Fidelity

**High-fidelity.** Final layout, spacing, typography scale, component anatomy, copy, and color relationships. Recreate faithfully, but **express every value through the codebase's existing Tailwind/shadcn tokens** (`bg-primary`, `text-muted-foreground`, `border-border`), never as hardcoded hex. The hex values in this document exist so you can read the prototype, not so you can paste them.

One deliberate deviation: the prototype renders in Inter because it runs in a browser preview. **The app keeps its existing system font stack** — do not add `next/font/google`. The reasoning in the project's `globals.css` (offline/intranet-friendly for a government deployment) is correct.

---

## Current state of the codebase

Already present in `frontend/src`:

```
app/
  login/page.tsx
  dashboard/page.tsx
  dashboard/layout.tsx
  dashboard/members/page.tsx
  dashboard/members/[id]/
  dashboard/members/new/
  globals.css
components/
  app-sidebar.tsx  app-header.tsx  breadcrumbs.tsx
  members-table.tsx
  member-documents-panel.tsx  member-status-history-panel.tsx
  ui/  (avatar badge button card dropdown-menu input label
        select separator sonner table textarea tooltip)
lib/
  nav-items.ts  member-status.ts  auth-context.tsx  jwt.ts
  api-errors.ts  config.ts  utils.ts
  types/member.ts
```

**shadcn components still to add:** `checkbox`, `radio-group`, `tabs`, `dialog`, `sheet`, `progress`, `alert`, `form`, `pagination`, `skeleton`, `switch`.

### Four decisions already made — apply these

1. **Primary color stays blue-600** (the existing `--primary`). Navy `#0b3b6f` is used **only** for the sidebar rail. Do not repaint the app navy.
2. **Sidebar becomes a dark navy rail** (the current light sidebar tokens are replaced).
3. **Root font size drops from 18px to 16px.** The existing `html { font-size: 18px }` override scales every rem-based utility ~12% larger than these designs. Re-check the login form and any member-facing view after the change; if larger text is needed for accessibility, prefer a user-toggled preference over a global root override.
4. **Member statuses stay at three** — `PendingVerification | Active | Inactive`, exactly as `lib/types/member.ts` and `Bimss.Contracts` define. The prototype's earlier "In arrears" and "Separated" states are dropped. Contribution standing (arrears) is a **separate field**, never a member status.

---

## Design tokens

### Colors

Existing tokens in `globals.css` are unchanged except where noted. Resolved hex is given for reading the prototype:

| Token | Value | ≈ hex |
|---|---|---|
| `--primary` | `oklch(0.546 0.215 262.881)` | `#2563eb` blue-600 |
| primary hover | `oklch(0.488 0.217 264)` | `#1d4ed8` blue-700 |
| `--ring` | `oklch(0.623 0.188 262.881)` | `#3b82f6` blue-500 |
| `--foreground` | `oklch(0.145 0 0)` | `#09090b` |
| `--muted` | `oklch(0.97 0 0)` | `#f4f4f5` |
| `--muted-foreground` | `oklch(0.556 0 0)` | `#71717a` |
| `--border` / `--input` | `oklch(0.922 0 0)` | `#e4e4e7` |
| `--destructive` | `oklch(0.577 0.245 27.325)` | `#dc2626` |
| Sidebar rail | new | `#0b3b6f` navy |

**Add to `:root`:**

```css
--app-bg: #fafafa;          /* main content area behind cards */
--primary-subtle: #eff6ff;  /* blue-50: selected row, selected radio card */
```

**Replace the sidebar token block:**

```css
--sidebar: #0b3b6f;
--sidebar-foreground: #ffffff;
--sidebar-primary: #ffffff;
--sidebar-primary-foreground: #0b3b6f;
--sidebar-accent: rgba(255, 255, 255, 0.16);   /* active item background */
--sidebar-accent-foreground: #ffffff;
--sidebar-border: rgba(255, 255, 255, 0.15);
--sidebar-ring: rgba(255, 255, 255, 0.4);
```

**Change in `@layer base`:** `html { font-size: 16px }` (or remove the override).

### Status badge colors

Centralize in `lib/member-status.ts` — one function returning a badge variant per status. No component picks colors itself.

| State | Background | Foreground |
|---|---|---|
| `Active`, Posted, passed check | `#dcfce7` | `#166534` |
| `PendingVerification`, For review, Adjusted | `#fef9c3` | `#854d0e` |
| `Inactive`, Draft | `#f4f4f5` | `#52525b` |
| Attention (contribution standing only) | `#ffedd5` | `#9a3412` |
| Alert count badge, overdue stripe | `#c2410c` | `#ffffff` |
| Info callout | bg `#fffbeb`, border `#fde68a`, text `#78350f` |  |

### Typography (16px root)

| Role | Size / weight / tracking |
|---|---|
| Login hero H1 | 41px / 700 / `-0.03em` |
| Screen title (topbar) | 15px / 600 |
| Card / section title | 14.5px / 600 |
| Metric value | 27px / 600 / `-0.03em`, tabular |
| Body, table cell | 13px / 400 |
| Form label | 13px / 500 |
| Helper, meta | 12.5px / 400, muted-foreground |
| Table header, badge | 12px / 500 |
| Legal notice, micro | 11.5px / 400 |

**Every numeric column, ID, amount, and date uses `tabular-nums`.**

### Spacing, shape, elevation

- Page padding `20px 22px 48px`; gap between cards `14px`.
- Card: `bg-card border border-border rounded-xl`, inner padding `18px 20px`.
- Control heights: input/select/button `36–38px`; small button `28–30px`.
- Sidebar `242px` fixed; topbar `56px`; both sticky.
- `--radius` stays `0.625rem`.
- **No drop shadows** except shadcn's own popover/dialog defaults.
- Focus: 3px ring at ~18% alpha of `--ring`. Never remove outlines.

---

## Application shell

```
AppShell
├── Sidebar — 242px, navy, sticky, full height
│   ├── Brand: 34px ringed "BI" seal + "BIMSS" / "Buklod ng Kawani"
│   ├── OPERATIONS: Dashboard · Membership register · Applications (badge)
│   │      · Contributions · Loans and benefits · Approvals (badge) · Reports
│   ├── ADMINISTRATION: Settings and roles · Audit log
│   └── Footer: 32px avatar · name · active role · EXIT
└── Main — --app-bg
    ├── Topbar 56px white sticky: screen title · FY pill · 258px search
    │      · "Alerts · n" outline button · "New application" primary
    └── Content — max 980px for forms, full width for tables/dashboard
```

Sidebar item: 8px 10px padding, `rounded-lg`, 13.5px. Default transparent → hover `rgba(255,255,255,.10)` → active `--sidebar-accent` + weight 600. Group headings 10px, uppercase, `.14em` tracking, 50% opacity. Count badges are `rounded-full`, 10.5px, `#c2410c` on white text.

**`lib/nav-items.ts` needs a `group` discriminator** (`"operations" | "administration"`) to render the headings. Keep `isNavItemActive` — its exact/prefix pathname logic is correct. (The prototype used flat screen state, which had a bug where two items sharing a target both highlighted; the pathname approach avoids this.)

Nav is filtered by role — see "Roles and access".

---

## Screens

### 1. Login — `/login`

Two columns, `1.05fr / 1fr`, full viewport.

**Left panel** — navy `#0b3b6f`, white text, padding `52px 56px`, `justify-content: space-between`, `overflow: hidden`:
- Top: 46px circle with 2px `rgba(255,255,255,.5)` border containing "BI"; beside it "REPUBLIC OF THE PHILIPPINES" (10.5px, 600, `.16em`, uppercase, 72% opacity) over "Bureau of Immigration · Buklod ng Kawani" (14.5px / 600).
- Middle: "BIMSS · RELEASE 1.0" eyebrow (10.5px, 600, `.2em`, uppercase, 70%); H1 "Buklod Integrated Membership and Services System" (41px / 700 / `-0.03em` / line-height 1.09, max-width 450px); paragraph at 14.5px / line-height 1.7 / 82% opacity: *"One secured record per member, replacing fragmented and manual recordkeeping. Membership, contributions, and member services are handled in a single verified workflow."*
- Bottom: 1px `rgba(255,255,255,.2)` top border, 22px padding-top, three stats 40px apart — 3,412 Active members · ₱48.2M Fund balance · 27 Offices covered (values 23px / 600 tabular, labels 11.5px / 70%).
- Two decorative outline circles bottom-right: 440px at `rgba(255,255,255,.13)`, 290px at `.10`, both offset off-canvas.

**Right panel** — white, centered, form max-width 376px:
- H2 "Sign in" (24px / 600 / `-0.02em`); helper "Use your BI employee number or Buklod membership ID."
- Fields (38px, `rounded-lg`, 1px `--border`): **Employee / Membership ID** (tabular) · **Password** with "Forgot password" link inline on the label row · **Sign in as** select · "Remember this device for 30 days" checkbox.
- Primary submit "Sign in" (40px, full width) → OR divider → outline "Continue with BI single sign-on".
- Footer notice, 11.5px muted, line-height 1.65: *"All access is logged. Personal data is processed under the Data Privacy Act of 2012 (RA 10173). Unauthorized use is subject to administrative and criminal liability."*

**Behavior:** field errors inline via `Form`; failed authentication renders an `Alert` above the form, never a toast. The role select is prototype-only — production reads the role from JWT claims.

### 2. Dashboard — `/dashboard`

Three rows.

**Row 1** — four equal metric cards: *Active members* 3,412 / "+38 this month, 27 offices" · *Collections this month* ₱4.12M / "96.4% of payroll deductions posted" · *Outstanding loans* ₱11.8M / "412 accounts, 6 in arrears" · *Fund balance* ₱48.2M / "As of 15 Aug 2026". Anatomy: 12.5px muted label → 27px tabular value → 12px muted note.

**Row 2** — `1.55fr / 1fr`:
- **Collections and disbursements.** Subtitle "Monthly, millions of pesos". Legend top-right: 9px blue square "Collections", 9px `#c7cdd4` square "Disbursements". 176px plot; eight month groups; each group is a pair of bars, 3px gap, `border-radius: 3px 3px 0 0`; 1px baseline rule; month labels 11px muted. Implement with Recharts — keep it flat, no gridlines, no axis lines beyond the baseline.
- **Pending your action.** Subtitle "Queued for review or approval". Clickable bordered rows: 3px full-height status stripe · title (13px / 500) + meta (11.5px muted) · right-aligned count (18px / 600). Rows: Membership applications 12 · Salary loan requests 18 (orange stripe) · Benefit claims 5 · Record corrections 3. Footer row above a divider: "Oldest item in queue" / "9 days".

**Row 3** — two equal cards:
- **Recent activity** — header with "Full audit log" link; rows are a 50px tabular time gutter + action text with the actor appended in muted after a `·`. Cap at 5. Sourced from the audit log.
- **Membership by office** — subtitle "Top divisions, active members"; per row a label/count line then a 6px `rounded-full` track (`#f4f4f5`) with a primary-colored fill.

**States:** empty = one muted sentence in place of the card body, no illustration. Loading = `Skeleton` blocks matching each card's height.

### 3. Membership register — `/dashboard/members`

- **Filter pills** (`rounded-full`, 6px 13px, 12.5px, each with a count): All members 3,412 · Active 3,208 · Pending verification 112 · Inactive 76. Active pill = primary solid; others white on `--border`, hover primary border. Right-aligned: "Filters · 2" and "Export CSV" outline buttons.
- **Bulk bar** inside the card header, above the table, `--app-bg` background: select-all checkbox · divider · "3 selected" · Verify / Assign officer / Print IDs small outline buttons · right-aligned "Showing 1–8 of 3,412".
- **Columns:** checkbox · **Member** (30px avatar with initials, `#eff6ff` bg + primary text; name 13px/500 linking to the record; position 11.5px muted) · Membership ID (tabular) · Office / Division · Status badge · Contributions YTD (right, tabular) · Last posted (tabular) · `···` actions.
- Row hover `#fafafa`; 1px `#f4f4f5` dividers; header cells 12px / 500 muted.
- **Footer:** "Page 1 of 427" left; Previous / 1 2 3 / Next right; current page = primary solid 30px square.

**Phasing:** the existing `members-table.tsx` has name, employee number, and status. Add the avatar cell, status badges, filter pills, and bulk bar now. **Contributions YTD and Last posted depend on Phase 4 contributions data** — hide behind a flag until then. Pagination, sorting, and filtering must be server-side; the register is 3,400+ rows.

### 4. Member record — `/dashboard/members/[id]`

- Breadcrumb "Members / {name}" (existing `breadcrumbs.tsx`).
- **Identity header card:** 76px ID-photo placeholder (diagonal stripe pattern `repeating-linear-gradient(135deg, #f4f4f5 0 6px, #e9eaec 6px 12px)`, `rounded-lg`, centered 9.5px "ID photo" label) · name (21px / 600 / `-0.025em`) + status badge · "Position · Division · Member since {date}" (13px muted) · four fact blocks 26px apart (label 11.5px muted over value 14px / 600 tabular): Membership ID, Employee no., Contributions YTD, Outstanding loan · right actions "Print ID" (outline) and "Edit record" (primary).
- **Tabs** (underline style, 2px primary indicator, 13px): Contributions · Personal · Loans · Benefit claims · Documents · Audit trail. Wire the existing `member-documents-panel.tsx` into **Documents** and `member-status-history-panel.tsx` into **Audit trail**.
- **Contributions tab**, `1.35fr / 1fr`: left = **Contribution ledger** table (Period · Reference tabular muted · Amount right-aligned · Status badge). Right column = **Personal details** (key/value rows, muted label left, 500-weight value right-aligned) over **Beneficiaries** (bordered `rounded-lg` rows: name 13px/500 + relationship 11.5px muted, right-aligned share %).

**Validation:** beneficiary shares must total 100%.

### 5. Membership application — `/dashboard/members/new`

Five-step wizard, content max-width 980px.

Steps: Personal information → **Employment and eligibility** (shown) → Beneficiaries → Documents → Review and submit.

- **Header card:** title "Membership application" · "Draft saved 2 minutes ago · Ref. APP-2026-00412" · right-aligned "Step 2 of 5". Below, five equal-width segments: a 4px `rounded-full` bar (primary if done/current, `--border` if upcoming) over an 11.5px label (current = 600 weight, foreground; upcoming = muted).
- **Body card:** section title "Employment and eligibility" + helper *"Verified against the BI personnel record. Fields marked with an asterisk are required."* Two-column grid, `gap: 16px 18px`, fields spanning 1 or 2 columns.
- **Field states:** normal white with `--border`; **read-only** `#fafafa` background, muted text, helper "Pulled from the BI personnel record and read-only."; **error** `#fca5a5` border + destructive helper; optional right-aligned 11px suffix such as "verified".
- **Contribution basis:** three bordered radio cards in a row — "Standard · ₱800 / month" (Default rate for rank-and-file) · "Supervisory · ₱1,200 / month" (Division chiefs and above) · "Voluntary top-up" (Member specifies the amount). Selected: primary border + `--primary-subtle` background + filled 7px dot.
- **Supporting documents:** dashed `#d4d4d8` `rounded-lg` drop zone on `#fafafa`, 20px padding, "Drop files or browse" + constraints line *"PDF, JPG, or PNG · maximum 5 MB per file · appointment paper and valid ID required"*. Below, uploaded-file chips: 26px type square, name 12.5px/500, size 11px muted, `✕` remove.
- **Cross-field warning** as an amber `Alert` with a 3px left bar: *"The employee number entered already has a closed Buklod record from 2016. Confirm whether this is a reinstatement before submitting; reinstatements retain the original membership ID."*
- **Footer** above a divider: "Back" outline left; "Save draft" outline + "Continue to beneficiaries" primary right.

**Behavior:** `react-hook-form` + `zod` per step; validate on step change; autosave draft on step change; block forward navigation on validation failure but always allow Back.

### 6. Contributions and remittances — `/contributions` *(Phase 4)*

- Four tiles: Posted this period ₱2.06M · Unposted / on hold ₱74.4K · Arrears ₱312K · Adjustments YTD 41.
- Card header "Post remittance batch" + "Payroll deduction file for the period 01–15 Aug 2026"; actions "Import payroll file" (outline) and "Post batch" (primary).
- Table: Batch · Period · Source (Payroll deduction / Over-the-counter / Adjustment) · Members (right) · Amount (right) · Status badge (Draft / For review / Posted) · Posted by.

**Behavior:** posting is irreversible — confirm in a `Dialog` naming the batch id, member count, and total. Posted batches are corrected only through an `ADJ-` adjustment entry.

### 7. Approvals — `/approvals` *(Phase 5)*

Master–detail, `1.25fr / 1fr`.

- **Queue** (header "Approval queue" + "18 items"): 30px avatar · name (13px/500) · "{kind} · {ref}" (11.5px muted) · right-aligned amount (13px/600 tabular) over age. **Age turns destructive past the 7-day SLA.** Selected row `--primary-subtle`.
- **Detail:** "Salary loan · LN-2026-00318" + state badge · "Filed 07 Aug 2026 by Dela Cruz, Marvin S. · BKD-2015-00233" · 2-column fact grid in bordered boxes (Amount applied ₱60,000 · Term 24 months · Monthly amortization ₱2,750 · Interest 6% p.a. · Net take-home after ₱21,340 · Existing loan balance ₱0) · **Eligibility checks** list (16px round mark, `✓` green or `!` amber, label + right-aligned value) · Remarks textarea, placeholder *"Recorded in the audit trail and visible to the member"* · actions Approve (primary) / Return for revision (outline) / Deny (destructive outline, `#fecaca` border on white).

**Behavior:** Return and Deny require remarks; Approve does not. Amounts above the two-person threshold show a second-approver notice.

### 8. Reports — `/reports` *(Phase 7)*

3-column card grid: name (14px/600) · description (12.5px muted, line-height 1.55) · cadence pill + "Last run {date}". Hover = primary border. Cards: Collection summary · Membership register · Loan portfolio · Benefit claims released · Delinquency report · Audit trail extract.

Below: **Membership growth** — single-series quarterly bars over three years, year switcher (active year primary solid), current/incomplete quarter rendered `#c7cdd4`.

Every report exports to CSV and PDF and records the run in the audit log.

### 9. Settings and roles — `/settings`

- **Roles and permissions** table: Role · Scope · Accounts (right, tabular) · Key permissions. Subtitle: *"Applies to every account assigned the role. Changes take effect on next sign-in."*
- **System preferences** switches (38×21 track): two-person approval above ₱50,000 · lock posted remittance batches · email members on status change · allow member-initiated record corrections (off by default).

Any change writes an audit entry with before/after values.

---

## Roles and access

| Role | Scope | Permissions |
|---|---|---|
| **Member** | Own record only | View own record, file applications and claims, download statements |
| **Membership Officer** | All member records | Create and verify records, edit personal details, print IDs |
| **Treasurer / Finance Officer** | Financial modules | Post remittances, review loans, release benefits, run financial reports |
| **System Administrator** | System-wide | Manage users and roles, configure reference data, read audit log |

Enforce in three places: **JWT claims on the API**, **route guards in middleware**, and **nav filtering in the shell**. Never rely on hidden UI alone. Members get a reduced shell — own record, own ledger, file and track requests — and never see the register.

---

## Interactions & behavior

- **Navigation:** pathname-driven active state via the existing `isNavItemActive`. One item highlights at a time.
- **Transitions:** hover color changes only, ~150ms. No page transitions, no entrance animations, no parallax. This is an operational system used daily.
- **Toasts:** `sonner` for post/approve/deny confirmations. Never for validation errors — those are inline.
- **Destructive/irreversible actions** (Post batch, Deny, Delete beneficiary): confirm in a `Dialog` that restates the specific consequence and identifiers.
- **Loading:** `Skeleton` matching final block dimensions; never spinners in cards.
- **Empty:** one muted sentence plus, where relevant, the primary action. No illustrations.
- **Responsive:** desktop-first. Below `lg`, the sidebar collapses into a `Sheet`; tables scroll horizontally within their card rather than reflowing to cards. Minimum tap target 44px on touch.

## State management

Per-screen client state needed: table selection set, active filter pill, current wizard step + per-step form values + draft-saved timestamp, selected approval id, active profile tab, sidebar sheet open. Server state — members, ledgers, batches, approvals, reports — via the existing API layer (`lib/api-errors.ts`, `lib/config.ts`) with server components for initial fetch; auth from `lib/auth-context.tsx`.

## Business rules the UI must express

1. Every state change is attributable — actor, timestamp, before/after — and appears in Recent activity and the record's Audit trail tab.
2. Posted financial records are immutable; corrections are new `ADJ-` entries with remarks.
3. Return and Deny require remarks.
4. Read-only fields sourced from the BI personnel record are visibly disabled with the reason stated.
5. Beneficiary shares total 100%.
6. Members never see other members' data — including counts and search results.
7. Currency always `₱` with thousands separators and two decimals in ledgers; summary tiles may abbreviate (`₱4.12M`).
8. Reference ID formats: `BKD-YYYY-NNNNN` member · `BI-YYYY-NNNNN` employee · `APP-` application · `LN-` loan · `CLM-` claim · `RC-` record correction · `RB-` remittance batch · `ADJ-` adjustment · `OTC-` over-the-counter.

## Accessibility

- AA contrast minimum; white on navy `#0b3b6f` and white on blue-600 both pass.
- Visible focus everywhere; never remove outlines.
- Full keyboard path through table selection, wizard steps, and approval actions.
- Tables carry a visually-hidden `<caption>` and `scope` on headers.
- **Status is never color-only** — always the text label too.

## Assets

No image assets. The "BI" seal is a text placeholder inside a ringed circle, and the member photo is a CSS stripe pattern. **Replace both with the official Buklod/BI seal and real ID photos** before UAT. Icons come from `lucide-react`, already in the project.

## Suggested implementation order

1. **Tokens** — `globals.css`: 16px root, navy sidebar block, `--app-bg`, `--primary-subtle`.
2. **Shell** — `nav-items.ts` groups, navy `app-sidebar.tsx`, `app-header.tsx` search + actions. Everything else hangs off this.
3. **Badges** — status variant mapping in `lib/member-status.ts`.
4. **Register** — `members-table.tsx`: avatar cell, badges, filter pills, bulk bar, pagination footer.
5. **Dashboard** — three-row layout; Recharts for the paired bar chart.
6. **Member record** — tabs; rewire the two existing panels.
7. **Wizard** — `/members/new` five-step form.
8. Phase 4+ — Contributions, Approvals, Reports, Settings.

## Not designed yet

Member self-service (Phase 2), beneficiaries UI (Phase 3), **elections module (Phase 6 — no designs exist)**, import-batch error/staging screens, ID card print template, loan amortization schedule, mobile layouts, dark mode, and the audit-log browser. Ask before implementing these.

## Files in this bundle

| File | What it is |
|---|---|
| `BIMSS.dc.html` | Interactive prototype — all nine screens. Open in a browser and click through. |
| `BIMSS-UI-SPEC.md` | Full UI specification with the repo mapping table (which spec section touches which file). |
| `README.md` | This document. |
