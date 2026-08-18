# Handoff: BIMSS Membership & Services UI

## Integration status (2026-08-18)

Tokens, the navy sidebar/header shell, centralized status badges, the real brand
assets (BI seal + login background photo), self-hosted Inter, and a reusable
wizard shell (`components/forms/wizard.tsx`) are integrated into `frontend/`.
Login, dashboard, the members register, member record, and the new/edit member
forms (now an actual navigable 2-step wizard: Personal information → Employment
information, not a single page) are re-skinned to match. The header also carries
a search box and notification bell matching this spec — both visually present
but intentionally inert (no cross-entity search or notifications backend yet);
"New member" is a real, functional link. See `docs/PHASE1_BACKLOG.md` for the
task-level record of exactly what shipped.

**Typography now matches this document's type scale exactly at the primitive
level, not just per-screen.** Every shared shadcn/ui component under
`components/ui/` (`input`, `select`, `textarea`, `table`, `badge`, `card`,
`label`, `button`, `tabs`, `alert`, `avatar`, `dropdown-menu`) has its default
text size set to this spec's Typography table below (13px form fields/body,
12.5px helper/meta, 12px table header/badge, 14.5px/600 card titles) — they
previously defaulted to Tailwind's stock sizes (16px inputs and labels, 14px
body/badges), off-spec everywhere until this pass. **Build future screens on
these primitives directly and they inherit the correct scale automatically —
don't re-derive sizes from the prototype or hardcode `text-[…]` values that
duplicate what the primitive already sets.**

**Everything else in this document — the expanded six-step wizard's later steps
(Family, Beneficiaries, Documents, Review+Submit), Beneficiaries, Loans,
Elections, Notifications/Announcements, the Audit log browser, and the expanded
Reports grid — exists only in this design handoff and the live Claude Design
project. None of it has been implemented in `frontend/` yet.** These are
Phase 2–6 (and Phase 7 once scoped) work, tracked in `docs/PHASE2_BACKLOG.md`
through `docs/PHASE6_BACKLOG.md` — read the relevant one before implementing
any of these screens, since the actual task breakdown (and any Buklod-confirmed
business rules, especially the UNCONFIRMED loan placeholders below) lives
there, not here.

## Overview

UI design for the **Buklod Integrated Membership and Services System (BIMSS)** — the Bureau of Immigration Buklod ng Kawani employee association system. The design covers the full screen set: authentication, an admin dashboard, the membership register, a member record view, a membership application wizard, contributions/remittance posting, an approvals queue, the beneficiaries module (self-service + officer review), the loans module (apply, my loan, loan account, payment import), the elections module (setup, monitoring, ballot, results), notifications and announcements, the audit-log browser, reports, and settings/roles.

The purpose of the system is to replace fragmented and manual Buklod recordkeeping with one secured system for membership and member services.

## About the design files

The files in this bundle are **design references created in HTML**. `BIMSS.dc.html` is an interactive prototype showing intended look, layout, and navigation behavior. **It is not production code and should not be copied into the app.**

The target codebase already exists: `bi-buklod-bimss/frontend` — Next.js (App Router) + React + TypeScript, Tailwind CSS, shadcn/ui, talking to an ASP.NET Core 10 Web API with JWT bearer auth over SQL Server / EF Core 10. **Recreate these designs using that project's established patterns, components, and tokens.** Several screens are already partially built (see "Current state of the codebase" below) — extend them rather than replacing them.

Open `BIMSS.dc.html` in a browser to click through every screen (sidebar Administration group carries the member-view entries). The login screen's "Sign in as" select switches roles; it is a prototype affordance only — in production, JWT claims determine the role.

## Fidelity

**High-fidelity.** Final layout, spacing, typography scale, component anatomy, copy, and color relationships. Recreate faithfully, but **express every value through the codebase's existing Tailwind/shadcn tokens** (`bg-primary`, `text-muted-foreground`, `border-border`), never as hardcoded hex. The hex values in this document exist so you can read the prototype, not so you can paste them.

The prototype renders in Inter, and the app now matches: Inter is **self-hosted** via `@fontsource-variable/inter` (the font files ship with the build), not loaded via `next/font/google`. Never add `next/font/google` or a CDN `<link>` — either fetches over the network at runtime, which breaks the offline/intranet government deployment this app targets. The system-font stack (`ui-sans-serif, system-ui, ...`) is kept only as the `--font-sans` fallback chain in `globals.css`, not the primary face.

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
  dashboard/members/new/page.tsx      (2-step wizard)
  dashboard/members/[id]/edit/page.tsx (mirrors new/, 2-step wizard)
  globals.css
components/
  app-sidebar.tsx  app-header.tsx  breadcrumbs.tsx
  members-table.tsx
  member-documents-panel.tsx  member-status-history-panel.tsx
  forms/
    wizard.tsx        — reusable stepper shell (WizardHeader, WizardStepBody);
                         build future multi-step flows (Loans apply, Elections
                         setup, Benefits once scoped) on this, not a bespoke one
    record-form.tsx    — FormSection, FormFooter, RequiredMark, FieldError
  ui/  (alert avatar badge button card dropdown-menu input label
        select separator sonner table tabs textarea tooltip)
lib/
  nav-items.ts  member-status.ts  auth-context.tsx  jwt.ts
  api-errors.ts  config.ts  utils.ts
  types/member.ts
public/
  bi-seal.png  immigration-bg.jpg   — real brand assets, first image
  assets in the app; reuse these rather than re-exporting from Claude Design
```

**shadcn components still to add:** `checkbox`, `radio-group`, `dialog`, `sheet`, `progress`, `form`, `pagination`, `skeleton`, `switch`.

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
│   │      · Contributions · Loan accounts · Loan payment import · Approvals (badge)
│   │      · Beneficiary requests (badge) · Elections · Announcements · Reports
│   ├── ADMINISTRATION: Settings and roles · Audit log
│   └── Footer: 32px avatar · name · active role · EXIT
└── Main — --app-bg
    ├── Topbar 56px white sticky: screen title · FY pill · 258px search
    │      · notification bell + unread count badge · "New application" primary
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
- **Contributions tab**, `1.35fr / 1fr`: left = **Contribution ledger** table (Period · Reference tabular muted · Amount right-aligned · Status badge). Right column = **Personal details** (key/value rows, muted label left, 500-weight value right-aligned) over **Beneficiaries** (bordered `rounded-lg` rows: name 13px/500 + relationship 11.5px muted — a beneficiary is name + relationship only; no share or percentage field exists).

**Beneficiaries panel:** header "Add beneficiary" outline button (30px, always available — no count cap); per-row "Edit" / "Remove" text actions; inline request form (primary border, `--primary-subtle` fill) with Full name + Relationship select from the relationship reference list. Remove uses the same panel as a confirmation, no fields.

**Behavior:** add / update / remove are **change requests**, not direct writes. On submit the row goes amber (`#fffbeb` on `#fde68a`) with a "Pending review" badge, drops its actions, and shows "{kind} requested · BEN-YYYY-NNNNN · awaiting officer decision"; a confirmation toast names the reference. Officer decision clears it.

### 5. Membership application — `/dashboard/members/new`

Six-step wizard, content max-width 980px. The stepper segments are clickable.

Steps: **Personal information** → **Employment, education, and eligibility** → **Family information** → **Beneficiaries** → **Documents** → **Review and submit**.

- **Header card:** title "Membership application" · "Draft saved 2 minutes ago · Ref. APP-2026-00412" · right-aligned "Step n of 6". Below, six equal-width segments: a 4px `rounded-full` bar (primary if done/current, `--border` if upcoming) over an 11.5px label (current = 600 weight, foreground; upcoming = muted).
- **Body card:** section title = step name + a helper line per step. Two-column grid, `gap: 16px 18px`, fields spanning 1 or 2 columns. Subsections inside a step are separated by a `#f4f4f5` top rule with an 18px pad, a 13.5px/600 heading, and an 11.5px muted line.
- **Reference-data selects, never free text**, wherever a field is a reference lookup: Suffix · Civil status · Sex · Office/division · Highest educational attainment · Eligibility · Relationship. All read the lists in Settings and roles · Reference data.

**Step 1 — Personal information.** Surname* · First name* · Middle name · Suffix (select) · Civil status* (select) · Date of birth* · Sex* (select). Subsection **Contact:** Landline (optional) · Mobile number* · Email address*. Subsection **Address:** Present residential address* and Provincial or permanent address — two separate 2-row textareas, full width, placeholder naming the parts to write out.

**Step 2 — Employment, education, and eligibility.** The verified employment fields and the reinstatement warning, then subsection **Education:** Highest educational attainment* (select) + Degree or course (free text, blank for below-college). Then subsection **Civil service and professional eligibility:** an **add-another-row** list — "Add eligibility" outline button in the subsection header; each row is Eligibility* (select) + Details (free text: rating, licence number, or date taken) + a 34px `✕` remove button on a `1fr 1fr 34px` grid; dashed "No eligibility recorded." empty state. A member may hold several, and zero is valid.

**Step 3 — Family information.** Spouse's full name (its helper reflects the civil status picked in step 1) · Father's full name · Mother's full maiden name · Parents' present address (2-row textarea). All optional. Subsection **Children:** repeatable `1fr 190px 34px` rows of Full name* + Date of birth* + `✕`, "Add child" button, dashed "No children recorded." empty state. Zero children is valid; a row that exists needs both fields, so empty ones carry `#fca5a5` borders.

**Step 4 — Beneficiaries.** Repeatable `1fr 200px 34px` rows of Full name* + Relationship* (select) + `✕`. **No share, percentage, or ordering field** — proceeds settle under the by-laws. Footer note that changes after activation are filed as change requests instead.

**Step 5 — Documents.** The dashed drop zone and uploaded-file chips described below.

**Step 6 — Review and submit.** Label/value summary of every step (muted label left, 500-weight value right, `#f4f4f5` row rule), then the **Data Privacy Act consent** block: required 17px checkbox + "Data Privacy Act consent *" + the notice text (collection, storage, and processing for membership, contribution, loan, benefit, and election purposes, family and beneficiary data included) + a muted line that consent is recorded with date, time, and account and written to the audit trail. Unchecked the block sits `#fafafa` on `--border` and the submit button renders `#93b4f0`; clicking it shows the inline error "Consent is required before the application can be submitted." Checked, the block turns primary-bordered on `--primary-subtle`. **The form cannot submit without consent.**
- **Field states:** normal white with `--border`; **read-only** `#fafafa` background, muted text, helper "Pulled from the BI personnel record and read-only."; **error** `#fca5a5` border + destructive helper; optional right-aligned 11px suffix such as "verified".
- **No contribution-basis selector.** Every member contributes the same fixed flat rate, so the application carries no rate choice. Earlier drafts showed three basis radio cards; they were removed and must not be rebuilt.
- **Supporting documents:** dashed `#d4d4d8` `rounded-lg` drop zone on `#fafafa`, 20px padding, "Drop files or browse" + constraints line *"PDF, JPG, or PNG · maximum 5 MB per file · appointment paper and valid ID required"*. Below, uploaded-file chips: 26px type square, name 12.5px/500, size 11px muted, `✕` remove.
- **Cross-field warning** as an amber `Alert` with a 3px left bar: *"The employee number entered already has a closed Buklod record from 2016. Confirm whether this is a reinstatement before submitting; reinstatements retain the original membership ID."*
- **Footer** above a divider: "Back" outline left (muted label on step 1); "Save draft" outline + "Continue to {next step}" primary right, becoming "Submit application" on the last step.

**Behavior:** `react-hook-form` + `zod` per step; validate on step change; autosave draft on step change; block forward navigation on validation failure but always allow Back. Repeatable groups (eligibility, children, beneficiaries) are `useFieldArray`.

**API status:** only Personal and Employment are wired to the create-member endpoint today. Education, Eligibility, Contact, Address, Family, Children, Beneficiaries, and Consent are all confirmed in the backend Member domain model — the remaining wiring is implementation work, not an open business question, so none of these carry UNCONFIRMED banners.

### 6. Contributions and remittances — `/contributions` *(Phase 3)*

- Four tiles: Posted this period ₱2.06M · Unposted / on hold ₱74.4K · Arrears ₱312K · Adjustments YTD 41.
- Card header "Post remittance batch" + "Payroll deduction file for the period 01–15 Aug 2026"; actions "Import payroll file" (outline) and "Post batch" (primary).
- Table: Batch · Period · Source (Payroll deduction / Over-the-counter / Adjustment) · Members (right) · Amount (right) · Status badge (Draft / For review / Posted) · Posted by.

**Behavior:** posting is irreversible — confirm in a `Dialog` naming the batch id, member count, and total. Posted batches are corrected only through an `ADJ-` adjustment entry.

### 7. Approvals — `/approvals` *(Phase 4)*

Master–detail, `1.25fr / 1fr`.

- **Queue** (header "Approval queue" + "18 items"): 30px avatar · name (13px/500) · "{kind} · {ref}" (11.5px muted) · right-aligned amount (13px/600 tabular) over age. **Age turns destructive past the 7-day SLA.** Selected row `--primary-subtle`.
- **Detail:** "Salary loan · LN-2026-00318" + state badge · "Filed 07 Aug 2026 by Dela Cruz, Marvin S. · BKD-2015-00233" · 2-column fact grid in bordered boxes (Amount applied ₱60,000 · Term 24 months · Monthly amortization ₱2,750 · Interest 6% p.a. · Net take-home after ₱21,340 · Existing loan balance ₱0) · **Eligibility checks** list (16px round mark, `✓` green or `!` amber, label + right-aligned value) · Remarks textarea, placeholder *"Recorded in the audit trail and visible to the member"* · actions Approve (primary) / Return for revision (outline) / Deny (destructive outline, `#fecaca` border on white).

**Behavior:** Return and Deny require remarks; Approve does not. Amounts above the two-person threshold show a second-approver notice **for non-loan items** — whether the threshold applies to loans is unconfirmed (see the loans placeholder block below).

**Loans use three explicit steps.** A 3-segment stage strip above the fact grid tracks Review (Officer) → Approve (Approver) → Release (Treasurer → Chair); the state badge follows it (For review / For approval / For release) and an amber note explains the current step. Per-step actions: Review = "Mark reviewed · endorse to approval" / Return for correction / Deny · Approve = "Approve loan" / Return for correction / Deny · Release = "Prepare release · Treasurer" / "Confirm release · Chair" / Hold. Review, Approve, and Release are separate permissions — a single account holding all three must still perform them as separate audited actions. **Release generates the payment schedule and opens the loan account**; it is never a side effect of approval.

### 7b. Beneficiary change requests — `/approvals/beneficiaries` *(Phase 2)*

Same master–detail shell as Approvals — reuse the components, do not rebuild.

- **Queue:** avatar · member name · "{kind} · BEN-ref" (tabular) · right: "For review" badge over age (`#c2410c` past SLA). Kinds: Add / Update / Remove beneficiary.
- **Detail:** header "{kind} · {ref}" + badge · "Filed {date} by {member} · {membership id}" · fact grid (Member, Membership ID, Request type, Filed through) · **Requested change** box with Current / Requested rows ("No entry" for additions, "Removed from the record" for removals) · Remarks textarea · Approve / Return for revision / Deny.

**Behavior:** Return and Deny require remarks. Approving writes or deletes the beneficiary row server-side; the member is notified and the pending badge clears.

### 7c. My beneficiaries — `/me/beneficiaries` *(Phase 2)*

Member self-service, max-width 980px. Identity strip, then `1fr / 1fr`: **My beneficiaries** (read-only list + the same request flow scoped to self) and **My change requests** (Reference · Change · Filed · Status; Pending review yellow / Approved green / Returned yellow / Denied gray, with the officer remark under Returned and Denied badges).

Scope every query to the signed-in member's id server-side; never filter client-side.

> ## ⚠ UNCONFIRMED PLACEHOLDER VALUES — LOANS
>
> The following loan parameters appear in the spec and the prototype **only as placeholders so the screens could be drawn**. None of them has been confirmed with Buklod. Do not treat them as decisions, do not build validation rules or permissions around them, and confirm each before UAT:
>
> 1. **₱100,000 maximum loan amount** — unconfirmed.
> 2. **12 / 24 / 36-month terms** — unconfirmed.
> 3. **Flat interest rate used in the prototype's computation (12% per annum equivalent)** — unconfirmed; only the *flat, computed-once-at-approval* behavior is known.
> 4. **Release performed as "Treasurer prepares, Chair confirms"** — unconfirmed; the *existence* of Release as its own auditable step is confirmed, the roles are not.
> 5. **Co-maker must accept the request in-system** — unconfirmed; the *requirement* of a co-maker is confirmed, the acceptance flow is not.
> 6. **Whether the ₱50,000 two-person approval rule in Settings applies to loans** — still open with Buklod. The prototype shows a single approver on the Approve step; this is a placeholder, not a decision.
>
> Confirmed for loans: one product currently offered · co-maker is required (a guarantor, not optional) · interest is flat and computed once at approval so every installment is equal · Review, Approve, and Release are three distinct, separately-permissioned steps · penalty charges are flat fees per missed installment, never a percentage · schedule, balances, penalties, and interest are computed server-side.

### 7d. Apply for a loan — `/me/loans/new` *(Phase 4)*

Four-step wizard reusing the membership-application wizard components: header card (ref `LN-YYYY-NNNNN`, "Step n of 4", four segment bars), body card, footer Back / Save draft / Continue. Steps: **Loan details** → Co-maker → Documents → Review and submit.

- **Loan details:** single preselected loan-type radio card; amount field with `₱` prefix validated against the product maximum (over cap = `#fca5a5` border + red helper); term as three segmented cards; right-hand **Indicative computation** panel (Principal, Flat interest, Total payable, Monthly installment × term) on `#fafafa`. Flat interest is computed once at approval — equal installments, no amortization recalculation.
- **Co-maker:** required. Search by name or membership ID; candidate rows carry an eligibility badge, Inactive members are non-selectable; selected co-maker card shows "Acceptance pending".
- **Documents:** reuse the drop zone + file chips.
- **Review:** key/value summary + submission consequences paragraph.

**Behavior:** `react-hook-form` + `zod` per step; the amount rule and the co-maker rules come from the server-side product configuration, not hardcoded constants.

### 7e. My loan — `/me/loans` *(Phase 4)*

Tiles (Current balance · Monthly installment · Paid to date · Next due) → active-loan strip → `1.25fr / 1fr` **Payment schedule** (# · Due date · Amount · Status: Paid green / Due yellow / Scheduled gray) and **Payment history** (Date · Reference · Amount · Balance) → **My loan applications** table (Reference · Filed · Amount · Term · Co-maker · Status) using the same history pattern as My change requests.

Status set and badge colors: green Approved / Released / Active / Fully paid · yellow Submitted / For review / For approval / For release / Returned for correction · gray Draft / Cancelled / Disapproved. Reason text prints under the badge for Returned, Disapproved, and Cancelled.

### 7f. Loan account — `/loans/[ref]` *(Phase 4)*

Header card (status badge, member + co-maker line, five fact blocks: Principal, Flat interest, Installment, Balance, Penalties charged; "Post adjustment" and "Statement of account" actions) → server-side computation notice → `1fr / 1fr` **Schedule** (adds an orange "Missed" badge) and right column **Payments** (running balance) over **Adjustments** (Reference · Type + note · Amount · Posted by).

**Rules:** everything financial is computed server-side; the screen is read-only except posting an adjustment, which writes an `ADJ-` entry. Penalties are **flat fees per missed installment, never a percentage**.

### 7g. Loan payment import — `/loans/payments` *(Phase 4)*

Identical anatomy to the Contributions batch screen — same payroll-deduction file shape, same card/table, same irreversible-post Dialog. Tiles: Posted this period · Unmatched lines · Missed installments · Portfolio balance. Table: Batch `LRB-YYYY-MMDD` · Period · Source · Accounts · Amount · Status (+ optional note line) · Posted by.

### 7h. Elections — `/elections` *(Phase 5 — new module, nothing existed before)*

Sidebar item "Elections" under Operations. IDs: `ELC-YYYY-NNN` election · `BAL-YYYY-NNNNN` ballot receipt.

**Election list:** table Election (name over ref) · Voting window · Positions · Eligible voters · Status (Draft gray / Voting open green / Voting closed yellow / Finalized green) · state-dependent action link (Continue setup / Monitor / View results). "Create election" primary in the card header.

**Election setup** — four-step wizard reusing the membership-application wizard: Election details → Positions and seats → Candidates → Voter list.

- Positions belong to **this election only** — there is no shared master position list. Each carries its own seat count, which may exceed 1 (e.g. Board Member, 5 seats).
- Candidates are added per position from the membership register.
- **Freeze voter list** is an explicit action behind a confirmation `Dialog` (title, irreversibility statement, captured/excluded fact rows, confirm "Freeze voter list") — it snapshots every member who is Active at that instant. After freezing the button is disabled ("Voter list frozen") and the panel shows Eligible voters / Frozen at / Frozen by. **Opening voting is a separate confirmed action.**

**Monitoring (while open)** — header card with election, window, freeze timestamp, and "Close voting" + "Finalize results" actions. Participation card only: count of voters against eligible, percentage, progress bar, turnout by office, ballots by hour. **Never render candidate totals or partial tallies while voting is open** — do not even query them.

**Ballot** — `/me/vote`, three steps: Select → Review → Receipt. One card per position with "Select up to n" and checkbox candidate rows capped at the seat count; every position may be left blank ("Leave blank (abstain)") and Continue is never blocked on empty positions. Review lists picks or "No selection — abstained". Submit is behind a confirmation `Dialog`. The receipt shows reference `BAL-YYYY-NNNNN`, election ref, and timestamp — **it never shows what was selected**.

**Results** — visible only after finalization; read from a static final-results set. Per position: ranked candidates, vote counts, "Elected" badge on the top-N by count, bars scaled to the leader, abstention count. Header records finalized timestamp and finalizing officer.

**Two distinct terminal actions:** *closing* stops new ballots; *finalizing* computes and locks the tally and publishes results. Both are separately permissioned, separately confirmed, and separately audited. Finalizing is not a side effect of closing.

### 8. Reports — `/reports` *(Phase 6)*

3-column card grid. Card: name (14px/600) + group pill (Finance / Membership / Audit) · description (12.5px muted, line-height 1.55) · "Outputs {formats}" · footer rule with cadence pill, "Last run {date}", and a primary "Run" affordance. Hover = primary border. Clicking opens a run `Dialog` (period + output formats) and confirming toasts that the report is queued.

Ten cards. **Finance:** Collection summary · Contribution collection by office · Remittance reconciliation · Loan portfolio · Loan releases and collections · Loan arrears ageing · Benefit claims released. **Membership:** Membership register · Delinquency report. **Audit:** Audit trail extract. The finance cards are the contribution-collection and loan-portfolio reports the earlier drafts anticipated — real cards now, with their parameters living in the run Dialog and executed server-side.

Below: **Membership growth** — single-series quarterly bars over three years, year switcher (active year primary solid), current/incomplete quarter rendered `#c7cdd4`.

Every report exports to CSV and PDF and records the run in the audit log.

### 8b. Notifications, announcements, and audit log — *(Phase 6)*

**Notification centre (all roles).** Topbar bell (38×36 outline button, `lucide-react` `Bell`) with an unread count pill in `#c2410c`, 1.5px white ring, tabular; no pill when nothing is unread. Opens a 396px `DropdownMenu`/`Popover` panel: header "Notifications" + "{n} unread · personal notices only" and a "Mark all as read" link; items are a 7px dot (primary unread, `#e4e4e7` read) + message (500 weight unread) + meta row of record reference (primary, tabular) and timestamp; unread rows on `#f8fbff`, hover `#fafafa`; list scrolls at 392px; footer links to Settings. Clicking an item marks it read, closes the panel, and routes to the record.

Contents are **personal only** — own application/request status changes, approvals and rejections, loan release, posted contributions, election open. Never another member's events. Email delivery for the same events is governed by the existing "Email members on status change" switch in Settings; no additional email screen exists.

**Announcements — `/announcements`.** `1.15fr / 1fr`: compose card (Title · Body textarea · Audience select — All members / Active members / Officers only / Single office · Placement select — Dashboard banner and list / List only · muted note about email delivery · "Publish announcement" primary + "Save as draft" outline) and a published list (title + "On dashboard" pill on the pinned item, body, date · author · audience, Edit / Unpublish text actions). Publishing with an empty title toasts a correction. One pinned announcement at a time — publishing a new banner unpins the previous.

Display surface: a `#eff6ff` on `#bfdbfe` banner at the top of the dashboard with an "ANNOUNCEMENT" pill, title, body, "Posted {date} by {author}", and an "All announcements" link. Rendered only when a pinned announcement exists. Reference `ANN-YYYY-NNNN`; publish/edit/unpublish are audited.

**Audit log browser — `/audit`** (System Administrator only). Filter card: Actor text · Action select · Object type select · Result select (All / Success / Denied / Failed) · Date range · Apply / Reset, with a footer note that filtering and paging run server-side and entries are append-only. Table card uses the register's conventions: filter-summary strip + "Export CSV" + "Showing x–y of n"; columns Timestamp · Actor (name over role) · Action · Object type · Object (link) · Result badge (Success green / Denied yellow / Failed gray) · Source IP; footer "Page n of m" with Previous / Next.

**Server-side pagination and filtering are mandatory** — this is the largest table in the system (18k+ rows in year one). Never load the trail into the client. The dashboard's "Full audit log" link and the "Audit trail extract" report both point here.

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
- **Toasts:** `sonner` for post/approve/deny/publish confirmations. Never for validation errors — those are inline.
- **Notifications:** the bell panel is read-only routing, not an inbox — no reply, no archive, no per-item delete. Marking read is per item (on click) or all at once.
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
5. A beneficiary is **name + relationship only** — no share, percentage, or ordering field exists anywhere in the UI or the API contract. Beneficiary changes are change requests, not direct edits.
6. Members never see other members' data — including counts and search results.
7. Currency always `₱` with thousands separators and two decimals in ledgers; summary tiles may abbreviate (`₱4.12M`).
8. Reference ID formats: `BKD-YYYY-NNNNN` member · `BI-YYYY-NNNNN` employee · `APP-` application · `LN-` loan · `CLM-` claim · `RC-` record correction · `RB-` remittance batch · `BEN-` beneficiary change request · `LRB-` loan payment batch · `ELC-` election · `BAL-` ballot receipt · `ANN-` announcement · `ADJ-` adjustment · `OTC-` over-the-counter.

## Ballot secrecy (non-negotiable)

1. No screen, for any role including System Administrator, shows how a specific member voted — no member-to-selection view, export, report, or audit entry, ever.
2. The audit trail records **that** a member voted and when, never **what** was selected.
3. While voting is open, per-candidate counts are not displayed or computed for display.
4. Results exist only after finalization and are read as a locked static tally.
5. The receipt proves participation only and never echoes selections back, not even to the voter.

Enforce this in the API contract, not just the UI: the ballot write endpoint must not persist voter identity alongside selections.

## Accessibility

- AA contrast minimum; white on navy `#0b3b6f` and white on blue-600 both pass.
- Visible focus everywhere; never remove outlines.
- Full keyboard path through table selection, wizard steps, and approval actions.
- Tables carry a visually-hidden `<caption>` and `scope` on headers.
- **Status is never color-only** — always the text label too.

## Assets

Two supplied assets, in `assets/`: `bi-seal.png` — the official Bureau of Immigration seal, used at 46px on the login panel and 34px in the sidebar brand block (`object-fit: contain`, no ring or frame around it); `immigration-bg.jpg` — the BI building photo used as the login left panel background, layered under a navy overlay (`linear-gradient(180deg, rgba(11,59,111,.92), rgba(11,59,111,.86) 45%, rgba(11,59,111,.95))`, `background-size: cover`) so the white headline, seal, and stat row keep AA contrast. The decorative outline circles stay on top of the photo. The member photo is still a CSS stripe placeholder — **replace with real ID photos** before UAT. Icons come from `lucide-react`, already in the project.

## Suggested implementation order

1. **Tokens** — `globals.css`: 16px root, navy sidebar block, `--app-bg`, `--primary-subtle`.
2. **Shell** — `nav-items.ts` groups, navy `app-sidebar.tsx`, `app-header.tsx` search + actions. Everything else hangs off this.
3. **Badges** — status variant mapping in `lib/member-status.ts`.
4. **Register** — `members-table.tsx`: avatar cell, badges, filter pills, bulk bar, pagination footer.
5. **Dashboard** — three-row layout; Recharts for the paired bar chart.
6. **Member record** — tabs; rewire the two existing panels.
7. **Wizard** — `/members/new` six-step form (Personal+Contact+Address → Employment/Education/Eligibility → Family information → Beneficiaries → Documents → Review+Consent). Only Personal and Employment are API-wired today; the rest is forward-looking per screen 5's "API status" note.
8. **Phase 2 — Beneficiaries** — panel on the member record, the change-request review queue, and My beneficiaries self-service (screens 4, 7b, 7c).
9. **Phase 3 — Contributions** (screen 6).
10. **Phase 4 — Loans** — apply, my loan, loan account, payment import, and the Approvals stage strip (screens 7, 7d–7g).
11. **Phase 5 — Elections** (screen 7h) — budget extra review time for ballot-secrecy code per `AGENTS.md`.
12. **Phase 6 — Notifications, Reports, Audit log** (screens 8, 8b).
13. **Phase 7 — Benefits**: not yet designed, blocked on a Buklod business-question round — see "Not designed yet" below.

## Not designed yet

Member self-service beyond beneficiaries, loans, and voting; import-batch error/staging screens; ID card print template; mobile layouts; dark mode. **Benefit-claim screens** are a real future module (Phase 7 — Benefits, see `docs/DEVELOPMENT_ROADMAP.md`), not yet scoped or designed — the "Benefit claims" text already sprinkled through Reports/dashboard/Approvals is intentional forward reference to that future phase, not accidental placeholder content, but no schema or screens exist for it yet. It needs its own Buklod business-question round (benefit types, eligibility, payout rules, funding source — see `docs/DATA_DICTIONARY.md`) before a backlog gets drafted, same as every other phase. Ask before implementing any of the above.

## Files in this bundle

| File | What it is |
|---|---|
| `prototype/BIMSS.dc.html` | Interactive prototype — every screen in the spec. Open in a browser and click through (loads `prototype/support.js` relatively, so the pair must stay together). |
| `prototype/support.js` | Runtime the prototype needs to render — generated, not hand-edited. |
| `prototype/assets/bi-seal.png` | Official Bureau of Immigration seal — see "Assets" above for where it's used. |
| `prototype/assets/immigration-bg.jpg` | BI building photo used as the login left-panel background. |
| `BIMSS-UI-SPEC.md` | Full UI specification with the repo mapping table (which spec section touches which file). |
| `README.md` | This document. |
| `CLAUDE_DESIGN_BRIEF.md` | Paste-in briefs used to build/update the live Claude Design project — kept for the next round of changes (e.g. once Phase 7 Benefits is scoped). |
