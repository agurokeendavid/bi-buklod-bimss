# Brief for updating the BIMSS Claude Design project

Paste this into the existing project (`https://claude.ai/design/p/1b192e5f-825d-43bf-bd09-34fc2ca56e1a`)
— don't start a new project. It already has the correct shell, tokens,
typography, and 7 of 9 screens; this brief only asks for corrections and
new screens.

## Before you touch anything: keep these as-is

Don't restyle or reintroduce new colors/spacing/typography. Everything
in `BIMSS-UI-SPEC.md`'s tokens/shell/typography sections is already
correct and already implemented in the real app — new screens must match
it exactly:

- Navy `#0b3b6f` sidebar rail only; primary stays blue-600 everywhere else.
- 16px root, the existing type scale (screen title 15px/600, card title
  14.5px/600, body/table 13px, form label 13px/500, helper 12.5px muted,
  table header/badge 12px/500).
- Card = `bg-card border border-border rounded-xl`, 18px/20px padding, no
  drop shadows.
- Status badges: green `#dcfce7`/`#166534` (Active/Posted/passed),
  yellow `#fef9c3`/`#854d0e` (Pending/For review/Adjusted), gray
  `#f4f4f5`/`#52525b` (Inactive/Draft), orange `#ffedd5`/`#9a3412`
  (attention/arrears only). Status is never color-only — always a text
  label too.
- Toasts (`sonner`) for confirmations only, never for validation errors —
  those are inline. Destructive/irreversible actions confirm in a
  `Dialog` naming the specific consequence.
- Reference ID format convention: `BKD-YYYY-NNNNN` member,
  `LN-` loan, `APP-` application, `ADJ-` adjustment, `RB-` remittance
  batch. Extend this pattern for new ID types rather than inventing a
  different shape (e.g. beneficiary change requests, elections, ballots).

## Two corrections to existing screens

1. **Member record → Contributions tab → Beneficiaries panel.** Remove
   the right-aligned share % column and drop "beneficiary shares must
   total 100%" as a validation rule entirely. A beneficiary is just a
   name + relationship — no share/percentage concept exists in this
   system.
2. **Membership application → step 2 → "Contribution basis" radio
   cards.** Remove this field completely (Standard/Supervisory/Voluntary
   top-up tiers). Contribution amount is a single fixed flat rate for
   every member — not chosen per application, not tied to rank. It isn't
   part of the membership application at all.

## New screens to add

### Beneficiaries (extends the Member record screen)

- The Beneficiaries panel (corrected above) needs an "Add beneficiary"
  action and a per-row edit/remove action, each opening a small form:
  name (text) + relationship (select, from the existing relationship
  reference list — same reference-data pattern as Civil status/Office
  already used elsewhere in the app).
- **Every add/update/remove is a change request, not a direct edit** —
  submitting shows a "Pending review" state on that beneficiary row
  (same yellow badge convention as elsewhere) until an officer decides
  it. No unlimited-count restriction in the UI — the "Add beneficiary"
  action is always available.
- Officer-side: a review queue for pending beneficiary change requests —
  reuse the existing Approvals screen's master-detail layout (queue list
  + detail panel with Approve/Return/Deny actions, remarks required on
  Return/Deny).
- Member self-service: a read-only beneficiaries list on their own
  profile/dashboard, plus the same add/edit/remove request flow scoped
  to themselves, and a status/history view of their own pending and past
  requests.

### Loans (extends the existing Approvals screen, which only covers the review/approve step)

- **Apply for a loan** (member self-service): loan type (currently one
  product), requested amount (validated against that product's fixed
  max), term, and a **required co-maker field** — select another member
  by name/ID (a required guarantor, not optional). Multi-step form
  following the same pattern as the membership application wizard.
- **Loan status tracking** (member self-service): a status/history view
  of the member's own applications (Draft/Submitted/For Review/For
  Approval/Approved/Disapproved/For Release/Released/Active/Fully Paid,
  plus Cancelled/Returned for Correction), same pattern as the
  Beneficiaries request history above.
- **My Loan** (member self-service, once active): payment schedule table
  (installment #, due date, amount, status), payment history, current
  balance. Interest is a flat rate computed once at approval, so every
  installment is equal — no amortization-recalculation UI needed.
- **Officer review** (extends the existing Approvals detail panel): the
  existing screen already shows review/approve actions — split this into
  two explicit, separately-permissioned steps ("Review" then "Approve"),
  plus a third distinct "Release" action that's its own auditable step
  (moves the loan from approved-on-paper to an active account with a
  generated schedule). Keep the existing eligibility-checks list and
  remarks-required-on-Return/Deny pattern.
- **Admin loan detail**: schedule, payments, adjustments (including
  penalty charges for missed payments — a flat fee, not a percentage),
  running balance, all computed server-side (don't design this as if
  the browser calculates the balance).
- **Payment batch import**: same layout as the Contributions screen's
  "Post remittance batch" card/table — this is a payroll-deduction file
  import, same shape as contributions.
- Confirm with me before assuming the existing "two-person approval
  above ₱50,000" toggle (in the current Settings screen) applies here —
  it was never actually confirmed with Buklod for Loans specifically.

### Elections (nothing exists yet — new module, new sidebar nav item)

- **Election setup** (officer/admin): create an election (name, voting
  window), then define positions **within that election** (not a shared
  fixed list — each election has its own position set), each position
  with a seat count (can be more than 1 — e.g. "Board Member" with 5
  seats), then add candidates per position, then a "Freeze voter list"
  action (captures currently-Active members as eligible — do this as an
  explicit, visible action with a confirmation dialog, since it's a
  point-in-time snapshot).
- **Election monitoring** (while open): show participation count only
  ("142 of 3,412 eligible voters have voted") — **never show live
  candidate totals or per-candidate vote counts while voting is open.**
- **Voting screen** (member self-service, when eligible + election
  open): one section per position, showing that position's seat count
  (e.g. "Select up to 5"); each position can be left blank (abstention
  is allowed — don't force a selection to proceed); a review step
  showing all selections before a final "Submit ballot" confirmation;
  after submission, a non-secret receipt/reference screen (never shows
  back what was selected — the receipt only proves participation).
- **Finalize action** (officer/admin): a distinct, explicitly-confirmed
  action separate from closing the voting window — closing prevents new
  ballots, finalizing computes and locks in the results.
- **Results screen**: published only after finalization, showing winners
  per position (top-N by vote count for multi-seat positions) — design
  this as reading a static "final results" data set, not a live tally.

### Notifications, Reports, Audit log (extends the existing Reports screen + sidebar)

- **Notification center**: a bell icon in the topbar with an unread
  count badge, opening a dropdown/panel list of personal notifications
  (status changes, approvals/rejections, election open) — each with a
  timestamp and a link to the relevant record, read/unread state.
- **Announcements**: officer-authored broadcast messages, a compose
  screen (title + body + publish action) and a display surface (e.g. a
  banner or list on the member dashboard).
- **Audit log browser** (admin, extends Reports' sidebar section):
  filterable table over the system's audit trail — actor, action,
  object type, timestamp, result — same table conventions as the
  Membership register (server-side pagination/filtering, not client-side,
  since this can be a large dataset).
- **Finance reports**: extend the existing Reports screen's card grid
  with contribution-collection and loan-portfolio report cards, same
  visual pattern as the existing Collection summary/Loan portfolio cards
  already shown there (these were already anticipated — just need to
  actually exist as real report cards, not placeholders).
- Email is in scope for this phase (status-change notifications,
  approvals) — the existing Settings screen's "email members on status
  change" toggle already anticipates this; no new screen needed for it
  beyond that toggle.

## What NOT to add

Don't design contribution-tier selection, beneficiary share/percentage
fields, or a direct-edit (no-approval) path for beneficiaries — all
explicitly ruled out above. Don't show live election results while
voting is open. Don't add a `MemberId`-to-candidate-selection view
anywhere (even for admins) — ballot secrecy means no screen, anywhere,
ever shows how a specific member voted.

## Logo/background asset swap (attach the files directly in Claude Design first)

Once real brand assets are attached in the Claude Design chat, paste
something like this — adjust the bracketed background placement to
match what was actually attached:

> I'm attaching our official logo and a background image — use them to
> replace placeholder branding only, don't redesign anything else.
>
> **Logo**: replace the placeholder "BI" text-in-a-ringed-circle
> everywhere it appears — the login panel (46px) and the sidebar brand
> block (34px). Keep the surrounding layout exactly as-is (position,
> spacing, the "Bureau of Immigration · Buklod ng Kawani" text beside/
> below it) — only swap the placeholder graphic itself for the attached
> logo, sized appropriately for each spot.
>
> **Background**: apply the attached background image to [the login
> screen's left navy panel, replacing/layering under the existing
> decorative circles — OR the general app content background behind
> cards, replacing `--app-bg` — pick whichever this actually is].
> Keep it readable: if the image is busy, preserve enough contrast for
> the white login text / dark card text respectively, and don't let it
> interfere with any existing component.
>
> Don't change anything else — all screens, tokens, layout, and copy
> stay exactly as they are. This is purely a branding-asset swap.

This is the first real image asset the project will have —
`frontend/public/` in the actual Next.js app is currently empty. Once
this is done in Claude Design and exported, the logo file should also
land in `frontend/public/` for the real app to use eventually, not just
the mockup.
