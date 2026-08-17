# BIMSS — UI Specification

**System:** Buklod Integrated Membership and Services System (BIMSS)
**Organization:** Bureau of Immigration — Buklod ng Kawani
**Purpose:** Replace fragmented/manual Buklod recordkeeping with one secured system for membership and member services.
**Source of truth for UI:** `BIMSS.dc.html` (interactive mockup in this project)
**Reconciled against:** `bi-buklod-bimss` @ Aug 2026 — existing Next.js frontend, `Bimss.Contracts` types, and `docs/DEVELOPMENT_ROADMAP.md`.

This document is the frontend contract. Implement screens to match it; when the mockup and this file disagree, the mockup wins for layout and this file wins for tokens and naming.

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

Your `html { font-size: 18px }` scales every rem-based utility ~12% larger than the mockup. Set it to **16px** and re-check the two places the 18px choice was protecting: the login form and the member self-service views (beneficiaries — Phase 2; loans — Phase 4; elections — Phase 5). If accessibility testing later demands larger text, prefer a user-toggled preference over a global root override.

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
| Upload | dashed `border-dashed rounded-lg` zone | + file chip list |
| Settings toggles | `Switch` | 38×21 track |
| Inline warning | `Alert` | left 3px bar, amber tokens |
| Toasts | `sonner` | post/approve/deny confirmations |
| Notification panel | `DropdownMenu` or `Popover` | 396px, bell trigger, unread dot list |
| Announcement banner | `Alert` on `--primary-subtle` | dashboard, pinned announcement only |
| Avatar initials | `Avatar` | `#eef1f4` bg, navy text, 30px |

---

## 4. Application shell

```
AppShell
├── Sidebar (242px, --primary bg, sticky, full height)
│   ├── Brand: 34px BI seal image + "BIMSS" / "Buklod ng Kawani"
│   ├── Group "OPERATIONS": Dashboard · Membership register · Applications (badge)
│   │     · Contributions · Loan accounts · Loan payment import · Approvals (badge)
│   │     · Beneficiary requests (badge) · Elections · Announcements · Reports
│   ├── Group "ADMINISTRATION": Settings and roles · Audit log
│   └── User footer: avatar · name · active role · EXIT
└── Main (--app-bg)
    ├── Topbar (56px, white, sticky): screen title · FY pill · search (258px)
    │     · notification bell with unread count · primary "New application"
    └── Page content (max 980px for forms, full width for tables/dashboard)
```

Sidebar item states: default transparent; hover `rgba(255,255,255,.10)`; active `rgba(255,255,255,.16)` + weight 600. Group labels 10px, uppercase, `.14em`, 50% opacity. Nav visibility is role-driven (§6).

---

## 5. Screens

### 5.1 Login — `/login`
Two-column, 1.05fr / 1fr, full viewport.

- **Left (navy, `assets/immigration-bg.jpg` under a navy overlay — `linear-gradient(180deg, rgba(11,59,111,.92), rgba(11,59,111,.86) 45%, rgba(11,59,111,.95))`, `cover`, decorative circles on top):** government line "Republic of the Philippines" + BI/Buklod line; official BI seal, `assets/bi-seal.png`, 46px, no ring or frame; release tag "BIMSS · Release 1.0"; H1 full system name; one-paragraph purpose; footer stat row (Active members / Fund balance / Offices covered); two large decorative outline circles, bottom-right, 13% and 10% white.
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
- **Phase note:** your current `members-table.tsx` has name, employee number, and status. Add the avatar cell, status badge colors, filter pills, and bulk bar now; the two contribution columns wait for Phase 3.
- Row hover `#fafafa`; name is the link to the record; 1px `#f4f4f5` row dividers.
- Footer: "Page n of m" left, Previous / numbers / Next right.
- Server-side pagination, sorting, and filtering — the register is 3,400+ rows.

### 5.4 Member record — `/members/[id]`
- Breadcrumb "Members / {name}".
- Identity header card: 76px ID-photo placeholder (diagonal-stripe pattern, label "ID photo"), name + status badge, position · division · member since, four fact blocks (Membership ID, Employee no., Contributions YTD, Outstanding loan), right actions "Print ID" (outline) and "Edit record" (primary).
- Tabs: Contributions · Personal · Loans · Benefit claims · Documents · Audit trail.
- Contributions tab, `1.35fr / 1fr`: **Contribution ledger** table (Period, Reference, Amount right-aligned, Status badge) and a right column of **Personal details** (key/value rows, label muted left, value 500 right) + **Beneficiaries** (bordered rows, name over relationship — no share or percentage concept exists).
- Beneficiaries panel header carries an "Add beneficiary" outline button (30px), always enabled — no maximum count. Each row shows "Edit" (primary text link) and "Remove" (muted, red on hover).
- Every add, update, and removal is a **change request**, never a direct edit. On submit the row turns `#fffbeb` on `#fde68a`, gains a yellow "Pending review" badge, loses its row actions, and shows a footer line "{kind} requested · BEN-YYYY-NNNNN · awaiting officer decision". A confirmation toast names the reference.
- Request form opens inline in the panel (primary border on `--primary-subtle`): Full name text field + Relationship select drawn from the relationship reference list (same reference-data pattern as Civil status / Office; helper "Reference list maintained in Settings and roles · Reference data"). Remove opens the same panel with no fields — title, confirmation sentence, Submit change request / Cancel.

### 5.5 Membership application — `/applications/new`
**Steps (6, clickable in the stepper):** 1 Personal information · 2 Employment, education, and eligibility · 3 Family information · 4 Beneficiaries · 5 Documents · 6 Review and submit. Header shows "Step n of 6"; bars fill up to the current step.

- **Step 1 Personal information** — Surname* · First name* · Middle name · Suffix (reference select) · Civil status* (reference select) · Date of birth* · Sex*. Subsection **Contact**: Landline (optional) · Mobile number* · Email address*. Subsection **Address**: Present residential address* and Provincial or permanent address, two separate textareas (2 rows). Required-but-empty fields carry `#fca5a5` borders and red help text, per the existing pattern.
- **Step 2 Employment, education, and eligibility** — the existing read-only/verified employment fields and the reinstatement warning, then subsection **Education**: Highest educational attainment* (reference select) + Degree or course (free text). Then subsection **Civil service and professional eligibility**: an **add-another-row** list — "Add eligibility" button in the subsection header, each row Eligibility* (reference select) + Details (free text) + a 34px ✕ remove button, dashed "No eligibility recorded." empty state. A member may hold several; zero is valid.
- **Step 3 Family information** — Spouse's full name (helper text reflects the civil status chosen in step 1) · Father's full name · Mother's full maiden name · Parents' present address. All optional. Subsection **Children**: repeatable rows of Full name* + Date of birth* + ✕, "Add child" button, dashed "No children recorded." empty state. Zero children is valid; a row that exists needs both fields.
- **Step 4 Beneficiaries** — repeatable rows of Full name* + Relationship* (reference select) + ✕ only. **No share, percentage, or ordering field** (§8 rule 5). Note that changes after activation are filed as change requests.
- **Step 5 Documents** — the existing dashed dropzone and uploaded-file chips.
- **Step 6 Review and submit** — a label/value summary of every step, then the **Data Privacy Act consent** block: required checkbox, notice text, and a line stating that consent is recorded with date, time, and account, and written to the audit trail. Unchecked, the submit button renders in `#93b4f0` and clicking it shows the inline error "Consent is required before the application can be submitted." — **the form cannot submit without consent**. Checked, the block turns primary-bordered on `--primary-subtle`.
- Reference selects (Suffix, Civil status, Office, Highest educational attainment, Eligibility, Relationship) all read from the reference lists in Settings and roles · Reference data — never free text.
- **API status:** only Personal and Employment are wired to the create-member endpoint today. Education, Eligibility, Contact, Address, Family, Children, Beneficiaries, and Consent exist in the backend Member domain model and are confirmed; the remaining wiring is implementation work, not an open question.

Five-step wizard, content max 980px.

Steps: Personal information → **Employment and eligibility** → Beneficiaries → Documents → Review and submit.

- Header card: title, "Draft saved …· Ref. APP-YYYY-NNNNN", "Step n of 5", then five equal segment bars (4px, navy done/current, `#e4e4e7` upcoming) with labels beneath.
- Body card: section title + helper; two-column field grid (`gap 16px 18px`), fields span 1 or 2 columns.
- Field states: normal (white, `--border`); **read-only** (`#fafafa` bg, muted text, helper "Pulled from the BI personnel record and read-only."); **error** (`#fca5a5` border + red helper); optional right-aligned suffix such as "verified".
- Documents: dashed drop zone ("Drop files or browse", constraints line) + uploaded-file chips with type square, name, size, remove.
- Cross-field warning as amber `Alert` (e.g. prior closed record → reinstatement keeps the original membership ID).
- Footer: Back left; Save draft + primary "Continue to …" right. Autosave draft on step change.

### 5.6 Contributions and remittances — `/contributions`
- Four tiles: Posted this period · Unposted / on hold · Arrears · Adjustments YTD.
- Batch card header: "Post remittance batch" + period subtitle; actions "Import payroll file" (outline) and "Post batch" (primary).
- Table: Batch · Period · Source (Payroll deduction / Over-the-counter / Adjustment) · Members (right) · Amount (right) · Status badge · Posted by.
- Posting is irreversible: confirm in a `Dialog` stating batch id, member count, and total. Posted batches are corrected only by an Adjustment entry (see settings toggle).

### 5.6b Beneficiary change requests — `/approvals/beneficiaries`
Officer-side review queue. Same master–detail as Approvals (`1.25fr / 1fr`) — reuse that screen's parts.

- **Queue:** avatar, member name, "{kind} · BEN-ref", right side a yellow "For review" badge over the age (age turns `#c2410c` past SLA). Selected row `#f4f4f5`. Kinds: Add beneficiary · Update beneficiary · Remove beneficiary.
- **Detail:** "{kind} · {ref}" + "For review" badge; "Filed {date} by {member} · {membership id}"; 2-column fact grid (Member, Membership ID, Request type, Filed through — self-service vs officer-filed); **Requested change** box with two labelled rows, Current ("No entry" for additions) and Requested ("Removed from the record" for removals); Remarks textarea ("Required for Return and Deny. Recorded in the audit trail and visible to the member."); actions Approve · Return for revision · Deny.
- Approving an addition/update writes the row; approving a removal deletes it. Any decision clears the pending state and notifies the member.

### 5.6c My beneficiaries (member self-service) — `/me/beneficiaries`
Content max 980px. Identity strip (name, membership ID · position · division, status badge) then a `1fr / 1fr` pair.

- **My beneficiaries** — the same panel as the member record, read-only list plus the same request flow scoped to the signed-in member. Helper: "This list is read-only. Requests you file are reviewed by a membership officer before the record changes."
- **My change requests** — table Reference · Change (kind over "{name} · {relationship}") · Filed · Status. Statuses: Pending review (yellow) · Approved (green) · Returned (yellow) · Denied (gray). Returned and Denied rows print the officer's remark under the badge, 11px muted, max 180px.

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

### 5.6d Apply for a loan (member self-service) — `/me/loans/new`
Four-step wizard, same pattern and chrome as the membership application (header card with ref `LN-YYYY-NNNNN`, "Step n of 4", four segment bars, footer Back / Save draft / Continue). Content max 980px. Every loan screen carries the visible UNCONFIRMED placeholder banner described above.

Steps: **Loan details** → Co-maker → Documents → Review and submit.

- **Loan details**, `1.1fr / .9fr`: loan-type radio card (one product, preselected, subtitle carries the product limits); **Requested amount** field with a `₱` prefix, validated against the product maximum — over the cap gives `#fca5a5` border + red helper "Exceeds the ₱100,000 maximum for this product"; **Term** as three equal segmented cards (selected = primary border on `--primary-subtle`). Right column: **Indicative computation** panel on `#fafafa` — Principal, Flat interest, Total payable (ruled), Monthly installment × term. Copy states that interest is flat, computed once at approval, so every installment is equal.
- **Co-maker** (required, not optional): amber note stating the co-maker must be an Active member and must accept the request in BIMSS before the application moves to review; search field (name or membership ID); candidate rows with avatar, name, "{id} · {office}", and an eligibility badge — Inactive members render on `#fafafa` with `not-allowed` cursor and an "Inactive · not eligible" gray badge; selected row gets the primary border. Selected co-maker summary card shows a yellow "Acceptance pending" badge.
- **Documents:** dashed drop zone + file chips (same component as the membership application).
- **Review and submit:** key/value summary (type, amount, term, flat interest, installment, co-maker + acceptance state, document count) and a closing paragraph: submission authorizes payroll deduction, notifies the co-maker, and locks the application (cancellable only while For review).

### 5.6e My loan (member self-service) — `/me/loans`
Content max 1080px. Interest is flat and fixed at approval, so **no amortization-recalculation UI exists anywhere**.

- Four tiles: Current balance · Monthly installment ("Fixed at approval") · Paid to date · Next due.
- Active-loan strip: "Salary loan · LN-ref" + green "Active" badge, then released date · principal · term · flat interest · co-maker; right action "Download statement".
- `1.25fr / 1fr`: **Payment schedule** table (# · Due date · Amount right-aligned · Status badge — Paid green / Due yellow / Scheduled gray), subtitle "24 equal installments of ₱2,800.00. Fixed at approval and not recalculated.", paginated footer; **Payment history** table (Date · Reference · Amount · Balance, all tabular).
- **My loan applications** table — Reference · Filed · Amount · Term · Co-maker · Status, same history pattern as My change requests. Full status set: Draft · Submitted · For review · For approval · Approved · Disapproved · For release · Released · Active · Fully paid · Cancelled · Returned for correction. Badge mapping: green for Approved / Released / Active / Fully paid; yellow for Submitted / For review / For approval / For release / Returned for correction; gray for Draft / Cancelled / Disapproved. Returned, Disapproved, and Cancelled rows print the reason under the badge.

### 5.6f Loan account (officer) — `/loans/[ref]`
- Header card: "Salary loan · LN-ref" + status badge (orange only for attention, e.g. "Active · 1 missed installment"), member · membership ID · co-maker, five fact blocks (Principal, Flat interest, Installment, Balance, Penalties charged); actions "Post adjustment" and "Statement of account".
- A muted strip states that interest, installment, penalties, and running balance are computed server-side and the screen is read-only except for posting an adjustment, itself an `ADJ-` entry.
- `1fr / 1fr`: **Schedule** (# · Due date · Amount · Status — adds a "Missed" orange badge) and a right column of **Payments** (Date · Reference · Amount · running Balance) over **Adjustments** (Reference · Type + note · Amount · Posted by). Adjustment subtitle: "Penalty charges are flat fees per missed installment, never a percentage."

### 5.6g Loan payment import — `/loans/payments`
Same anatomy as 5.6 Contributions — it is the same payroll-deduction file shape. Four tiles (Posted this period · Unmatched lines · Missed installments · Portfolio balance); batch card header "Post loan payment batch" + period subtitle with "Import payroll file" (outline) and "Post batch" (primary); table Batch (`LRB-YYYY-MMDD`) · Period · Source · Accounts (right) · Amount (right) · Status badge with an optional note line under it (e.g. "6 unmatched lines") · Posted by. Posting is irreversible and confirmed in a Dialog, exactly as with remittance batches.

### 5.6h Elections module — `/elections`
New module; nothing existed before this design. Sidebar item "Elections" under Operations. Reference formats: `ELC-YYYY-NNN` election, `BAL-YYYY-NNNNN` ballot receipt.

**5.6ha Election list** — card header "Elections" with subtitle "Each election defines its own positions, seats, and candidate set. Nothing is shared between elections." + "Create election" primary. Table: Election (name over ref) · Voting window · Positions (right) · Eligible voters (right) · Status badge · action link. Statuses: Draft (gray) · Voting open (green) · Voting closed (yellow) · Finalized (green). The action link routes by state: Continue setup · Monitor · View results.

**5.6hb Election setup** — four-step wizard, same chrome as the membership application (header card with ref, "Step n of 4", segment bars, footer Back / Save draft / Continue).
1. **Election details** — name, voting opens, voting closes, notice to members.
2. **Positions and seats** — this election's own position list (no shared master list). Rows show name + "n seat(s)" with Remove. Add-position panel on `#fafafa`: name field + **Seats** number field + "Add position", helper "Seat count may be more than one. A member may select up to the seat count for that position."
3. **Candidates** — one bordered group per position, header "{position} · n seat(s)" + candidate count; rows are avatar + name + "{membership id} · {office}" + Remove; footer row is a member search + "Add candidate".
4. **Voter list** — status panel (amber "Not frozen" → white "Frozen"), fact blocks (before: Active members now / Pending verification / Inactive excluded; after: Eligible voters / Frozen at / Frozen by), then **Freeze voter list** (primary; becomes a disabled gray "Voter list frozen") and **Open voting** (outline). Freezing opens a confirmation **Dialog** — title "Freeze the voter list?", body stating it is a point-in-time snapshot that cannot be undone or refrozen, fact rows for captured/excluded counts, confirm "Freeze voter list". Success is a toast naming the eligible count. Opening voting is its own confirmation Dialog.

**5.6hc Monitoring (voting open)** — `/elections/[ref]`. Header card with election name + "Voting open" badge, window and freeze timestamp, actions "Close voting" and "Finalize results" (both outline; both separately confirmed). Participation card: subtitle "Turnout only. Candidate totals do not exist in this screen while voting is open."; 34px count + "of 3,412 eligible voters have voted" + percentage, 8px progress bar; below, **Turnout by office** (label + "n of m" + bar) and **Ballots by hour** bars. Closing footer note states that no screen links a member to a selection, per-candidate counts are not computed while voting is open, and results exist only after finalization.
- **Never** render candidate totals, partial tallies, or leader indications while voting is open.

**5.6hd Ballot (member self-service)** — `/me/vote`, max 820px, three steps: Select → Review → Receipt.
- **Select:** one card per position, header "{position}" + rule "Select 1" / "Select up to n". Candidate rows are 17px checkbox squares (primary fill when checked) + name + "{id} · {office}"; selecting beyond the seat count is ignored. Card footer: "x of up to n selected · leaving this position blank is allowed" and a "Leave blank (abstain)" action. **Abstention is valid — never force a selection or block Continue on empty positions.**
- **Review:** every position listed with its picks joined by "·", or muted "No selection — abstained for this position". Copy: once submitted a ballot cannot be changed, viewed, or retrieved, by the member or any officer.
- **Submit** opens a confirmation Dialog ("Submit your ballot?").
- **Receipt:** green check, "Your ballot has been recorded", paragraph stating the receipt proves participation and does not record or reveal selections, then a bordered receipt block: Receipt reference `BAL-YYYY-NNNNN` · Election ref · Recorded timestamp. Actions "Download receipt" and "Back to dashboard". **The receipt never shows selections.**

**5.6he Results** — published only after finalization. Header card: "{election} · final results" + "Finalized" badge, ref · finalized timestamp · finalizing officer · ballots cast of eligible with turnout %. A muted strip states results are the locked final tally recorded at finalization, not recomputed on view, with no per-member data contributing. One card per position: header "{position}" + "n seats · m candidates"; ranked rows with rank number, name (600 weight for winners), green "Elected" badge for the top-N by vote count, right-aligned vote count, and a bar (primary for elected, `#d4d4d8` otherwise) scaled to the leader; footer line "n ballots abstained for this position". Read this screen from a **static final-results data set** — no live tally query.

### 5.6i Notifications, announcements, and audit log

**5.6ia Notification centre (all roles)** — topbar bell, 38×36 outline button, 16px `lucide-react` `Bell`. Unread count sits on the button as a `#c2410c` pill (17px min, 1.5px white ring, tabular). No count when everything is read; the bell stays. Opening toggles a 396px panel anchored bottom-right of the button (`DropdownMenu` or `Popover`, shadcn shadow, `rounded-xl`, z above the sticky topbar).
- Panel header: "Notifications" + "{n} unread · personal notices only" (or "All caught up · …"), right side "Mark all as read" primary text link.
- Items: 7px status dot (primary unread / `#e4e4e7` read), message (13px, weight 500 unread / 400 read), then a meta row — record reference in primary + timestamp, both tabular. Unread rows sit on `#f8fbff`; hover `#fafafa`. Clicking marks read, closes the panel, and routes to the referenced record.
- List scrolls at 392px. Footer row links to Settings ("Notification and email settings").
- **Contents are personal only** — status changes on the member's own applications and requests, approvals and rejections, loan release, posted contributions, and election-open notices. Never other members' events, never a tally.
- Email: when "Email members on status change" is on in Settings, the same events also send email. No separate email screen exists, and no email preview lives in the UI.

**5.6ib Announcements — `/announcements`** (officer-authored broadcast). `1.15fr / 1fr`.
- **Compose card:** Title field · Body textarea (7 rows) · **Audience** select (All members / Active members / Officers only / Single office) · **Placement** select (Dashboard banner and list / List only) · a bordered muted note that members with email notifications enabled also receive the announcement by email · footer "Publish announcement" (primary) + "Save as draft" (outline), with the author name right-aligned. Publishing without a title toasts "Add a title before publishing."; publishing toasts the audience.
- **Published card:** most recent first — title + "On dashboard" pill for the pinned item, body, then meta (date · author · audience) with "Edit" and "Unpublish" text actions. Only one announcement is pinned at a time; publishing a new banner announcement unpins the previous one.
- **Display surface:** `#eff6ff` on `#bfdbfe` banner at the top of the dashboard — "ANNOUNCEMENT" pill, title, body, "Posted {date} by {author}", and an "All announcements" link. The banner renders only when a pinned announcement exists.
- Reference format `ANN-YYYY-NNNN`. Publishing, editing, and unpublishing each write an audit entry.

**5.6ic Audit log browser — `/audit`** (System Administrator). Sidebar item "Audit log" under ADMINISTRATION; the dashboard's "Full audit log" link and the report card "Audit trail extract" both point here.
- **Filter card:** Actor (text) · Action (select) · Object type (select) · Result (All / Success / Denied / Failed) · Date range · "Apply" (primary) + "Reset" (outline). Footer note: filtering, sorting, and paging run on the server against the audit table; entries are append-only and cannot be edited or deleted from this screen.
- **Table card**, same conventions as the Membership register (§5.3): header strip with the active filter summary ("Filtered by actor "…", denied" / "No filters · full trail"), "Export CSV", right-aligned "Showing x–y of n"; columns Timestamp · Actor (name over role) · Action · Object type · Object (primary link to the record) · Result badge · Source (IP, tabular); footer "Page n of m" with Previous / Next.
- **Server-side pagination and filtering are mandatory** — the trail is the largest table in the system (18k+ rows in the first year). Never fetch the trail into the client. Page size 10 in the prototype; use the register's page size in production.
- Result badges use the standard status colors: Success green · Denied yellow · Failed gray.
- The trail records elections participation only, never selections (§8b).

### 5.7 Approvals — `/approvals`
Master–detail, `1.25fr / 1fr`.

- **Queue:** avatar, name, "{kind} · {ref}", right-aligned amount + age. Age turns `--destructive` past the SLA (7 days). Selected row `--primary-subtle`.
- **Detail:** title "{kind} · {ref}" + state badge; filed-by line; 2-column fact grid in bordered boxes (Amount applied, Term, Monthly amortization, Interest, Net take-home after, Existing loan balance); **Eligibility checks** list — 16px round mark, `✓` green / `!` amber, label + value; Remarks textarea ("Recorded in the audit trail and visible to the member"); actions Approve (primary) · Return for revision (outline) · Deny (destructive outline).
- Deny and Return require remarks. Amounts above the two-person threshold show a second-approver notice **for non-loan items**; whether that threshold applies to loans is unconfirmed (see the loans placeholder block above).
- **Loans split the review into three explicit, separately-permissioned steps.** A 3-segment stage strip sits above the fact grid — Review (Officer) → Approve (Approver) → Release (Treasurer → Chair) — with the state badge tracking it (For review / For approval / For release) and an amber note explaining the current step. Actions change per step: Review = "Mark reviewed · endorse to approval" / Return for correction / Deny; Approve = "Approve loan" / Return for correction / Deny; Release = "Prepare release · Treasurer" / "Confirm release · Chair" / Hold. Remarks stay required on Return and Deny at every step, and the eligibility-checks list is unchanged.
- **Release is its own auditable step**, not a side effect of approval: it moves the loan from approved-on-paper to an active account and generates the payment schedule. Every step records actor and timestamp.

### 5.8 Reports — `/reports`
- 3-column grid of report cards. Card anatomy: name (14px/600) + group pill (Finance / Membership / Audit, top-right) · description · "Outputs {formats}" line · footer rule with cadence pill, "Last run {date}", and a primary "Run" affordance. Hover = primary border. Clicking a card opens a **Dialog** naming the period and output formats; confirming queues the run and toasts "{report} queued. You will be notified when it is ready."
- Reports (10, grouped by pill): **Finance** — Collection summary · Contribution collection by office · Remittance reconciliation · Loan portfolio · Loan releases and collections · Loan arrears ageing · Benefit claims released. **Membership** — Membership register · Delinquency report. **Audit** — Audit trail extract.
- The six finance cards are the contribution-collection and loan-portfolio reports the earlier drafts anticipated; they are real cards now, not placeholders. Parameters (period range, office, arrears bucket) belong in the run Dialog, server-side.
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
// contributionsYtd / lastPostedAt / office arrive in Phase 3 — until then the
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

Reference ID formats — keep them: `BKD-YYYY-NNNNN` member, `BI-YYYY-NNNNN` employee, `APP-` application, `LN-` loan, `CLM-` claim, `RC-` record correction, `RB-` remittance batch, `ADJ-` adjustment, `BEN-` beneficiary change request, `LRB-` loan payment batch, `ELC-YYYY-NNN` election, `BAL-` ballot receipt, `ANN-YYYY-NNNN` announcement, `OTC-` over-the-counter.

---

## 8. Rules the UI must express

1. Every state change is attributable — actor, timestamp, before/after — and surfaces in Recent activity and the record's Audit trail tab.
2. Posted financial records are immutable; corrections are new `ADJ-` entries with remarks.
3. Return and Deny require remarks; Approve does not.
4. Read-only fields sourced from the BI personnel record are visibly disabled with the reason stated.
5. Loans move Review → Approve → Release; Release is separately permissioned and generates the schedule. Interest is flat and computed once at approval — installments are equal and never recalculated. Penalties are flat fees per missed installment.
6. Every loan requires a co-maker; the application cannot be submitted without one.
7. Beneficiary changes are requests, not edits — the record only changes on officer approval, and the pending state is visible on the row until then.
8. Members never see other members' data anywhere in the UI, including counts and search.
9. Currency always `₱` with thousands separators and two decimals in ledgers; tiles may abbreviate (`₱4.12M`).
10. Notifications are personal: a member sees only events on their own records. The audit log is the officer-facing history; the notification list is never a substitute for it.
11. The audit trail is append-only and readable only by System Administrators. No screen edits or deletes an entry.

## 8b. Ballot secrecy (non-negotiable)

1. No screen, for any role including System Administrator, shows how a specific member voted. There is no member-to-selection view, export, report, or audit entry anywhere in the UI.
2. The audit trail records **that** a member cast a ballot (and when), never **what** was selected.
3. While voting is open, per-candidate counts are neither displayed nor computed for display; monitoring shows participation only.
4. Results exist only after finalization and are read as a locked, static tally.
5. The ballot receipt proves participation only. It never echoes selections back, not even to the member who cast it.

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
| §5.5 wizard | `app/dashboard/members/new/` | 6-step stepper |
| §3 badges | `lib/member-status.ts` | add badge-variant mapping |
| §5.6 | — | Contributions/remittances — Phase 3, not yet scaffolded |
| §5.6b, 5.6c | — | Beneficiary change requests + self-service — Phase 2, not yet scaffolded |
| §5.6d–5.6g | — | Loan application/self-service/officer screens — Phase 4, not yet scaffolded |
| §5.7 | — | Approvals (loan review/approve/release is the bulk of it) — Phase 4, not yet scaffolded |
| §5.6h | — | Elections module — Phase 5, not yet scaffolded |
| §5.6i, §5.8 | — | Notifications/announcements/audit log, Reports — Phase 6, not yet scaffolded |
| §5.9 | — | Settings and roles — partially exists, extend per §5.9 |

(This repo's phase numbering is `docs/DEVELOPMENT_ROADMAP.md`'s renumbered scheme — Beneficiaries=2, Contributions=3, Loans=4, Elections=5, Notifications/Reports=6, Benefits=7. Cross-check there if this drifts again.)

`nav-items.ts` needs a `group` discriminator to render the OPERATIONS / ADMINISTRATION headings, and `isNavItemActive` already handles the exact/prefix logic correctly — keep it. Note the mockup's flat `screen` state had a bug where two items sharing a target both highlighted; your pathname-based version does not have this problem.

## 11. Not yet designed

Member self-service beyond beneficiaries, loans, and voting; mobile layouts; ID card print template; import-batch error/staging screens; dark mode. **Benefit-claim screens** are a real future module (Phase 7 — Benefits, `docs/DEVELOPMENT_ROADMAP.md`), not yet scoped or designed — the "Benefit claims" text already sprinkled through Reports/dashboard/Approvals is intentional forward reference, not accidental placeholder content, but no schema or screens exist for it yet; don't build against it until Phase 7 gets its own business-question round with Buklod like every other phase. Ask before implementing any of the above.
