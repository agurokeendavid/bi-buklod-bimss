# BIMSS — UI Specification

**System:** Buklod Integrated Membership and Services System (BIMSS)
**Organization:** Bureau of Immigration — Buklod ng Kawani
**Purpose:** Replace fragmented/manual Buklod recordkeeping with one secured system for membership and member services.
**Source of truth for UI:** `BIMSS.dc.html` (interactive mockup in this project)
**Reconciled against:** `bi-buklod-bimss` @ Aug 2026 — existing Next.js frontend, `Bimss.Contracts` types, and `docs/DEVELOPMENT_ROADMAP.md`.

This document is the frontend contract. Implement screens to match it; when the mockup and this file disagree, the mockup wins for layout and this file wins for tokens and naming.

**Integration status (2026-08-16):** see `docs/design/README.md`'s "Integration status" section
for exactly what's landed in `frontend/` versus deferred to a later phase — the short version is
tokens/shell/badges are fully in, and the five already-built screens (login, dashboard,
register, member record, new-member form) are re-skinned with any phase-gated content (charts,
contributions, loans, wizard steps) left out rather than faked.

---

## 1. Stack assumptions

| Layer | Technology |
|---|---|
| Frontend | Next.js (App Router) + React, TypeScript — **already scaffolded** |
| UI | shadcn/ui + Tailwind CSS |
| API | ASP.NET Core Web API (.NET 10), JWT bearer auth |
| Data | SQL Server / MSSQL via EF Core 10 |
| Hosting | IIS (API), Node hosting (frontend) |

Conventions: server components for data fetch, client components for anything interactive (tables with selection, wizards, dialogs). `zod` + `react-hook-form` for all forms. Currency and dates formatted `en-PH`, `Asia/Manila`.

---

## 2. Design tokens

**Your existing `globals.css` stays the source of truth for `--primary`.** Blue-600 remains the app accent; navy is used *only* for the sidebar rail. Three changes to make:

```css
:root {
  /* CHANGE 1 — drop the enlarged root size (was 18px) */
  /* in @layer base: html { font-size: 16px }  ← or remove the override entirely */

  /* CHANGE 2 — navy sidebar rail (replaces the light sidebar tokens) */
  --sidebar: #0b3b6f;
  --sidebar-foreground: #ffffff;
  --sidebar-primary: #ffffff;
  --sidebar-primary-foreground: #0b3b6f;
  --sidebar-accent: rgba(255, 255, 255, 0.16);   /* active item */
  --sidebar-accent-foreground: #ffffff;
  --sidebar-border: rgba(255, 255, 255, 0.15);
  --sidebar-ring: rgba(255, 255, 255, 0.4);

  /* CHANGE 3 — add these; the rest of your :root block is unchanged */
  --app-bg: #fafafa;                    /* main content area behind cards */
  --primary-subtle: #eff6ff;            /* blue-50: selected row, selected radio card */
}
```

Resolved hex of your existing tokens, for reference when reading the mockup:

| Token | oklch (yours) | ≈ hex |
|---|---|---|
| `--primary` | `oklch(0.546 0.215 262.881)` | `#2563eb` blue-600 |
| `--primary` hover | `oklch(0.488 0.217 264)` | `#1d4ed8` blue-700 |
| `--ring` | `oklch(0.623 0.188 262.881)` | `#3b82f6` blue-500 |
| `--border` / `--input` | `oklch(0.922 0 0)` | `#e4e4e7` |
| `--muted` | `oklch(0.97 0 0)` | `#f4f4f5` |
| `--muted-foreground` | `oklch(0.556 0 0)` | `#71717a` |
| `--foreground` | `oklch(0.145 0 0)` | `#09090b` |
| `--destructive` | `oklch(0.577 0.245 27.325)` | `#dc2626` |
| Sidebar rail | — | `#0b3b6f` navy |

`--radius` stays at your `0.625rem`; cards use `rounded-xl`.

**Never hardcode these hex values in components — use the Tailwind token classes** (`bg-primary`, `text-muted-foreground`, `border-border`). The hex table exists only so you can read the mockup.

### Root font size

Your `html { font-size: 18px }` scales every rem-based utility ~12% larger than the mockup. Set it to **16px** and re-check the two places the 18px choice was protecting: the login form and the member self-service views (Phase 2). If accessibility testing later demands larger text, prefer a user-toggled preference over a global root override.

### Status colors (badges)

| Meaning | Background | Foreground | Used for |
|---|---|---|---|
| Success / Active / Posted | `#dcfce7` | `#166534` | `Active`, posted batch, passed check |
| Warning / Pending / For review | `#fef9c3` | `#854d0e` | `PendingVerification`, adjustments, for-review |
| Neutral / Inactive / Draft | `#f4f4f5` | `#52525b` | `Inactive`, draft batch |
| Attention | `#ffedd5` | `#9a3412` | contribution standing only — never member status |
| Alert accent | `#c2410c` | `#ffffff` | sidebar count badge, overdue queue stripe |
| Info callout | bg `#fffbeb`, border `#fde68a`, text `#78350f` | — | inline form warnings |

Map these in `lib/member-status.ts` alongside the existing display-label mapping — one function returning the badge variant per `MemberStatus`, so no component picks colors itself.

### Typography

**Keep your system font stack** (`ui-sans-serif, system-ui, -apple-system, "Segoe UI"…`). The offline/intranet reasoning in your `globals.css` comment is correct for a government deployment — do not add `next/font/google`. The mockup renders in Inter purely because it runs in a browser preview; the design does not depend on it.

Sizes below assume a **16px root**.

| Role | Size / weight | Tailwind |
|---|---|---|
| Page H1 (login hero) | 41px / 700, `-0.03em` | `text-[41px] font-bold tracking-tighter` |
| Screen title (topbar) | 15px / 600 | `text-[15px] font-semibold` |
| Section / card title | 14.5px / 600 | `text-sm font-semibold` |
| Metric value | 27px / 600, tabular | `text-[27px] font-semibold tabular-nums` |
| Body / table cell | 13px / 400 | `text-[13px]` |
| Label | 13px / 500 | `text-[13px] font-medium` |
| Helper / meta | 12.5px / 400, muted-foreground | `text-xs text-muted-foreground` |
| Micro (table header, badge) | 12px / 500 | `text-xs font-medium` |

**All numeric columns, IDs, amounts, and dates use `tabular-nums`.**

### Spacing and shape

- Page padding: `20px 22px 48px`. Grid gap between cards: `14px`.
- Card: `bg-card border border-border rounded-xl` (12px), inner padding `18px 20px`.
- Control heights: input/select/button `36–38px`; small button `28–30px`.
- Sidebar `242px` fixed, topbar `56px`, both sticky.
- No drop shadows anywhere except popovers/dialogs (shadcn default).

---

## 3. shadcn/ui component mapping

Install: `button card input label select checkbox radio-group textarea badge table tabs separator dialog sheet dropdown-menu avatar progress alert sonner form breadcrumb pagination skeleton tooltip switch`

| Mockup element | Component | Notes |
|---|---|---|
| Primary action | `Button` default | navy, `h-9 rounded-lg font-medium` |
| Secondary action | `Button variant="outline"` | white on border |
| Destructive (Deny) | `Button variant="outline"` + destructive tokens | never solid red |
| Metric tile | `Card` | label / value / note, no icon |
| Register table | `Table` + TanStack Table | selection, sort, sticky header |
| Filter pills | `Badge` as button, `rounded-full` | active = navy solid |
| Status label | `Badge` `rounded-full` | color from status table above |
| Profile sections | `Tabs` underline variant | 2px navy indicator |
| Application wizard | custom stepper + `Form` | 5 segment bars, not numbered circles |
| Contribution basis | `RadioGroup` as bordered cards | selected card gets `--primary-subtle` |
| Upload | dashed `border-dashed rounded-lg` zone | + file chip list |
| Settings toggles | `Switch` | 38×21 track |
| Inline warning | `Alert` | left 3px bar, amber tokens |
| Toasts | `sonner` | post/approve/deny confirmations |
| Avatar initials | `Avatar` | `#eef1f4` bg, navy text, 30px |

---

## 4. Application shell

```
AppShell
├── Sidebar (242px, --primary bg, sticky, full height)
│   ├── Brand: 34px ring seal "BI" + "BIMSS" / "Buklod ng Kawani"
│   ├── Group "OPERATIONS": Dashboard · Membership register · Applications (badge)
│   │     · Contributions · Loans and benefits · Approvals (badge) · Reports
│   ├── Group "ADMINISTRATION": Settings and roles · Audit log
│   └── User footer: avatar · name · active role · EXIT
└── Main (--app-bg)
    ├── Topbar (56px, white, sticky): screen title · FY pill · search (258px)
    │     · Alerts count · primary "New application"
    └── Page content (max 980px for forms, full width for tables/dashboard)
```

Sidebar item states: default transparent; hover `rgba(255,255,255,.10)`; active `rgba(255,255,255,.16)` + weight 600. Group labels 10px, uppercase, `.14em`, 50% opacity. Nav visibility is role-driven (§6).

---

## 5. Screens

### 5.1 Login — `/login`
Two-column, 1.05fr / 1fr, full viewport.

- **Left (navy):** government line "Republic of the Philippines" + BI/Buklod line; seal placeholder (46px ring, "BI"); release tag "BIMSS · Release 1.0"; H1 full system name; one-paragraph purpose; footer stat row (Active members / Fund balance / Offices covered); two large decorative outline circles, bottom-right, 13% and 10% white.
- **Right (white, max 376px):** H2 "Sign in"; helper "Use your BI employee number or Buklod membership ID."; fields — ID, Password (+ inline "Forgot password" link on the label row), **Sign in as** role select, "Remember this device for 30 days" checkbox; primary submit; OR divider; outline "Continue with BI single sign-on"; DPA notice at 11.5px muted.
- The role select is a **mockup affordance for demoing role views**. In production the JWT claims decide the role — remove it or keep it dev-only.
- Errors: field-level messages via `Form`; failed auth = `Alert` above the form, never a toast.

### 5.2 Dashboard — `/dashboard`
1. Four metric tiles: Active members · Collections this month · Outstanding loans · Fund balance. Each = label, big tabular value, one-line note.
2. Row 2, `1.55fr / 1fr`:
   - **Collections and disbursements** — grouped bar chart, 8 months, paired navy `#0b3b6f` / grey `#c7cdd4` bars, 176px plot, baseline rule, month labels. Legend top-right. Use Recharts; keep the flat, gridline-free look.
   - **Pending your action** — clickable rows with a 3px left status stripe, title, meta, count; footer "Oldest item in queue".
3. Row 3, two equal cards: **Recent activity** (time gutter 50px + action + actor, from audit log, capped at 5, link to full log) and **Membership by office** (label / count + navy progress bar).

Empty states: replace card body with one muted sentence, no illustration. Loading: `Skeleton` matching each block's height.

### 5.3 Membership register — `/members`
- Filter pill row: All members · Active · Pending verification · Inactive, each with count (matches the `MemberStatus` union — no arrears or separated pill); right-aligned "Filters · n" and "Export CSV".
- Bulk bar (in the card header, above the table): select-all checkbox, "n selected", actions Verify / Assign officer / Print IDs, right-aligned "Showing x–y of n".
- Columns: checkbox · **Member** (avatar + name + position) · Membership ID · Office / Division · Status badge · Contributions YTD (right) · Last posted · row actions `···`.
- **Phase note:** your current `members-table.tsx` has name, employee number, and status. Add the avatar cell, status badge colors, filter pills, and bulk bar now; the two contribution columns wait for Phase 4.
- Row hover `#fafafa`; name is the link to the record; 1px `#f4f4f5` row dividers.
- Footer: "Page n of m" left, Previous / numbers / Next right.
- Server-side pagination, sorting, and filtering — the register is 3,400+ rows.

### 5.4 Member record — `/members/[id]`
- Breadcrumb "Members / {name}".
- Identity header card: 76px ID-photo placeholder (diagonal-stripe pattern, label "ID photo"), name + status badge, position · division · member since, four fact blocks (Membership ID, Employee no., Contributions YTD, Outstanding loan), right actions "Print ID" (outline) and "Edit record" (primary).
- Tabs: Contributions · Personal · Loans · Benefit claims · Documents · Audit trail.
- Contributions tab, `1.35fr / 1fr`: **Contribution ledger** table (Period, Reference, Amount right-aligned, Status badge) and a right column of **Personal details** (key/value rows, label muted left, value 500 right) + **Beneficiaries** (bordered rows, name/relationship + share %).
- Beneficiary shares must total 100% — validate on edit.

### 5.5 Membership application — `/applications/new`
Five-step wizard, content max 980px.

Steps: Personal information → **Employment and eligibility** → Beneficiaries → Documents → Review and submit.

- Header card: title, "Draft saved …· Ref. APP-YYYY-NNNNN", "Step n of 5", then five equal segment bars (4px, navy done/current, `#e4e4e7` upcoming) with labels beneath.
- Body card: section title + helper; two-column field grid (`gap 16px 18px`), fields span 1 or 2 columns.
- Field states: normal (white, `--border`); **read-only** (`#fafafa` bg, muted text, helper "Pulled from the BI personnel record and read-only."); **error** (`#fca5a5` border + red helper); optional right-aligned suffix such as "verified".
- Contribution basis: three radio cards — Standard ₱800/month, Supervisory ₱1,200/month, Voluntary top-up. Selected card: navy border + `--primary-subtle`.
- Documents: dashed drop zone ("Drop files or browse", constraints line) + uploaded-file chips with type square, name, size, remove.
- Cross-field warning as amber `Alert` (e.g. prior closed record → reinstatement keeps the original membership ID).
- Footer: Back left; Save draft + primary "Continue to …" right. Autosave draft on step change.

### 5.6 Contributions and remittances — `/contributions`
- Four tiles: Posted this period · Unposted / on hold · Arrears · Adjustments YTD.
- Batch card header: "Post remittance batch" + period subtitle; actions "Import payroll file" (outline) and "Post batch" (primary).
- Table: Batch · Period · Source (Payroll deduction / Over-the-counter / Adjustment) · Members (right) · Amount (right) · Status badge · Posted by.
- Posting is irreversible: confirm in a `Dialog` stating batch id, member count, and total. Posted batches are corrected only by an Adjustment entry (see settings toggle).

### 5.7 Approvals — `/approvals`
Master–detail, `1.25fr / 1fr`.

- **Queue:** avatar, name, "{kind} · {ref}", right-aligned amount + age. Age turns `--destructive` past the SLA (7 days). Selected row `--primary-subtle`.
- **Detail:** title "{kind} · {ref}" + state badge; filed-by line; 2-column fact grid in bordered boxes (Amount applied, Term, Monthly amortization, Interest, Net take-home after, Existing loan balance); **Eligibility checks** list — 16px round mark, `✓` green / `!` amber, label + value; Remarks textarea ("Recorded in the audit trail and visible to the member"); actions Approve (primary) · Return for revision (outline) · Deny (destructive outline).
- Deny and Return require remarks. Amounts above the two-person threshold show a second-approver notice.

### 5.8 Reports — `/reports`
- 3-column grid of report cards: name, description, cadence pill + "Last run {date}". Hover = navy border.
- Reports: Collection summary · Membership register · Loan portfolio · Benefit claims released · Delinquency report · Audit trail extract.
- Below: **Membership growth** card — single-series quarterly bars, year switcher (active year navy solid), incomplete/current quarter rendered grey `#c7cdd4`.
- Every report exports to CSV and PDF and records the run in the audit log.

### 5.9 Settings and roles — `/settings`
- **Roles and permissions** table: Role · Scope · Accounts (right) · Key permissions.
- **System preferences** switches: two-person approval above ₱50,000 · lock posted remittance batches · email members on status change · allow member-initiated record corrections.
- Any change here writes an audit entry with before/after values.

---

## 6. Roles and access

| Role | Scope | Can |
|---|---|---|
| **Member** | Own record only | View own record, file applications and claims, download statements |
| **Membership Officer** | All member records | Create and verify records, edit personal details, print IDs |
| **Treasurer / Finance Officer** | Financial modules | Post remittances, review loans, release benefits, run financial reports |
| **System Administrator** | System-wide | Manage users and roles, configure reference data, read audit log |

Enforce in three places: JWT claims on the API, route guards in middleware, and nav filtering in the shell. Never rely on hidden UI alone. Members see a reduced shell — own record, own ledger, file/track requests — not the register.

---

## 7. Data shapes (UI-facing)

```ts
// Already defined in frontend/src/lib/types/member.ts — do not redefine.
type MemberStatus = 'PendingVerification' | 'Active' | 'Inactive';

interface MemberListItem {
  id: string;                // BKD-2019-00871
  employeeNo: string;        // BI-2019-04871
  lastName: string; firstName: string; middleInitial?: string;
  position: string; office: string;
  status: MemberStatus;
  contributionsYtd: number;  // minor units or decimal — pick one API-wide
  lastPostedAt: string;      // ISO
}
// NOTE: your MemberSummary is the real contract (mirrors Bimss.Contracts).
// contributionsYtd / lastPostedAt / office arrive in Phase 4 — until then the
// register renders those columns from a separate contribution-standing query,
// or hides them behind a feature flag.

interface ContributionEntry {
  period: string;            // "Aug 2026 · 1st half"
  reference: string;         // RB-2026-0815 | ADJ-2026-0041
  amount: number;
  status: 'Posted' | 'Adjusted' | 'On hold';
}

interface RemittanceBatch {
  id: string; period: string;
  source: 'Payroll deduction' | 'Over-the-counter' | 'Adjustment';
  memberCount: number; totalAmount: number;
  status: 'Draft' | 'For review' | 'Posted';
  postedBy?: string; postedAt?: string;
}

interface ApprovalItem {
  ref: string;               // LN-2026-00318
  kind: 'Salary loan' | 'Membership application' | 'Hospitalization claim'
      | 'Mortuary benefit' | 'Record correction';
  memberId: string; memberName: string;
  amount?: number;
  filedAt: string;
  state: 'For review' | 'Approved' | 'Returned' | 'Denied';
}
```

Reference ID formats — keep them: `BKD-YYYY-NNNNN` member, `BI-YYYY-NNNNN` employee, `APP-` application, `LN-` loan, `CLM-` claim, `RC-` record correction, `RB-` remittance batch, `ADJ-` adjustment, `OTC-` over-the-counter.

---

## 8. Rules the UI must express

1. Every state change is attributable — actor, timestamp, before/after — and surfaces in Recent activity and the record's Audit trail tab.
2. Posted financial records are immutable; corrections are new `ADJ-` entries with remarks.
3. Return and Deny require remarks; Approve does not.
4. Read-only fields sourced from the BI personnel record are visibly disabled with the reason stated.
5. Beneficiary shares total 100%.
6. Members never see other members' data anywhere in the UI, including counts and search.
7. Currency always `₱` with thousands separators and two decimals in ledgers; tiles may abbreviate (`₱4.12M`).

---

## 9. Accessibility and quality bar

- Contrast AA minimum; navy `#0b3b6f` on white passes for text and UI.
- Focus visible everywhere: 3px `--ring`. Never remove outlines.
- Full keyboard path through table selection, wizard steps, and approval actions.
- Tables get `<caption>` (visually hidden) and `scope` on headers.
- Status is never color-only — always the text label too.
- Minimum tap target 44px on mobile; on tablet the sidebar collapses to a `Sheet`.

---

## 10. Mapping to your repository

| Spec section | File in `frontend/src` | Action |
|---|---|---|
| §2 tokens | `app/globals.css` | 3 changes: 16px root, navy sidebar tokens, add `--app-bg` / `--primary-subtle` |
| §4 shell | `components/app-sidebar.tsx`, `app-header.tsx` | restyle to navy rail; add nav groups |
| §4 nav | `lib/nav-items.ts` | add `group` field + the remaining items (see below) |
| §5.2 dashboard | `app/dashboard/page.tsx` | build 3-row layout |
| §5.3 register | `components/members-table.tsx`, `app/dashboard/members/page.tsx` | filter pills, bulk bar, badges, avatar cell |
| §5.4 record | `app/dashboard/members/[id]/` | tabs; `member-documents-panel` → Documents tab, `member-status-history-panel` → Audit trail tab |
| §5.5 wizard | `app/dashboard/members/new/` | 5-step stepper |
| §3 badges | `lib/member-status.ts` | add badge-variant mapping |
| §5.6–5.9 | — | Phase 4+, not yet scaffolded |

`nav-items.ts` needs a `group` discriminator to render the OPERATIONS / ADMINISTRATION headings, and `isNavItemActive` already handles the exact/prefix logic correctly — keep it. Note the mockup's flat `screen` state had a bug where two items sharing a target both highlighted; your pathname-based version does not have this problem.

## 11. Not yet designed

Member self-service (Phase 2) and mobile layouts, beneficiaries UI (Phase 3), **elections module (Phase 6 — no designs exist at all)**, ID card print template, loan amortization schedule, import-batch error/staging screens, dark mode, and the audit-log browser. Ask before implementing these.
