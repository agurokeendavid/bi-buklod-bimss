# BIMSS Initial Data Dictionary

This document maps the fields found in the current Buklod Google Forms Excel export into a normalized BIMSS domain model.

No real member values from the workbook are included here.

## Important migration note

The source workbook contains personal information. Treat it as a controlled migration source, not as a file to commit to the repository.

Import into staging first. Do not directly map spreadsheet columns into final tables without validation.

## Proposed entities

### Member
Core personal and membership identity.

### MemberEmployment
BI employment-related data.

### MemberEducation
Educational attainment and course information.

### MemberEligibility
Civil service / professional eligibility.

### MemberAddress
Present and permanent/provincial addresses.

### MemberContact
Phone/email contact values.

### MemberFamilyInformation / MemberChild
Spouse, parents, children.

### MemberBeneficiary
Normalized beneficiary rows.

### MemberDocument
Uploaded proof/document metadata.

### MemberPrivacyConsent
Versioned consent record.

### MemberImportStaging
Raw/normalized values from the migration source before approval.

## Excel field mapping

| # | Source field | Proposed target | Suggested type / rule |
|---:|---|---|---|
| 1 | Timestamp | Import metadata `SubmittedAt` | datetimeoffset; source audit only |
| 2 | Email Address | Import metadata `FormEmail` | string/email |
| 3 | Type of Submission | `MemberUpdateRequest.SubmissionType` or staging | enum/reference: new/update |
| 4 | Last Name | `Member.LastName` | nvarchar; required by policy |
| 5 | First Name | `Member.FirstName` | nvarchar |
| 6 | Middle Name | `Member.MiddleName` | nullable nvarchar |
| 7 | Suffix | `Member.Suffix` | nullable nvarchar/reference |
| 8 | Date of Birth | `Member.DateOfBirth` | date |
| 9 | Place of Birth | `Member.PlaceOfBirth` | nvarchar |
| 10 | Civil Status | `Member.CivilStatusId` | reference table |
| 11 | Spouse's Full Name (If Married) | `MemberFamilyInformation.SpouseFullName` | nullable nvarchar |
| 12 | BI Employee Number | `MemberEmployment.EmployeeNumber` | nvarchar business identifier; unique and mandatory (confirmed 2026-08-14) |
| 13 | Position/Designation | `MemberEmployment.PositionDesignation` | nvarchar/reference candidate |
| 14 | Division/Section/Unit | `MemberEmployment.OfficeUnitId` | reference table preferred |
| 15 | Date of Permanent Appointment | `MemberEmployment.PermanentAppointmentDate` | nullable date |
| 16 | Proof of Permanent Employment | `MemberDocument` | secure document metadata/storage reference |
| 17 | Highest Educational Attainment | `MemberEducation.HighestAttainmentId` | reference table |
| 18 | Degree or Course Completed | `MemberEducation.DegreeCourse` | nvarchar |
| 19 | Civil Service Eligibility/PRC Eligibility | `MemberEligibility.EligibilityType` | reference/text |
| 20 | Eligibility or Professional License Details | `MemberEligibility.Details` | nvarchar; do not assume numeric |
| 21 | Present Residential Address | `MemberAddress` type Present | nvarchar initially; later structured address if needed |
| 22 | Provincial/Permanent Address | `MemberAddress` type Permanent | nvarchar initially |
| 23 | Landline Number | `MemberContact.Landline` | string, not numeric |
| 24 | Cellphone Number | `MemberContact.MobileNumber` | string, not numeric |
| 25 | Current Email Address | `MemberContact.Email` | string/email |
| 26 | Names of Children | `MemberChild` collection or staged free text | manual/defined parsing rule required |
| 27 | Father’s Full Name | `MemberFamilyInformation.FatherFullName` | nvarchar |
| 28 | Mother’s Full Maiden Name | `MemberFamilyInformation.MotherMaidenName` | nvarchar |
| 29 | Parents’ Present Address | `MemberFamilyInformation.ParentsPresentAddress` | nvarchar |
| 30 | Beneficiary 1 — Complete Name | `MemberBeneficiary` row | normalize |
| 31 | Beneficiary 1 — Relationship to Member | same beneficiary row | reference/text |
| 32 | Beneficiary 2 — Complete Name | `MemberBeneficiary` row | normalize |
| 33 | Beneficiary 2 — Relationship to Member | same beneficiary row | reference/text |
| 34 | Beneficiary 3 — Complete Name | `MemberBeneficiary` row | normalize |
| 35 | Beneficiary 3 — Relationship to Member | same beneficiary row | reference/text |
| 36 | Beneficiary 4 — Complete Name | `MemberBeneficiary` row | normalize |
| 37 | Beneficiary 4 — Relationship to Member | same beneficiary row | reference/text |
| 38 | Reason for Joining Buklod | `Member.JoiningReason` | nvarchar(max) or bounded text |
| 39 | Privacy Notice consent | `MemberPrivacyConsent` | bool + notice version + timestamp + source |
| 40 | Additional Beneficiaries (Beneficiary 5 and above) | additional `MemberBeneficiary` rows after review | do not auto-parse until delimiter/format is agreed |

## Do not mirror these spreadsheet patterns into SQL

Avoid:

```text
Beneficiary1Name
Beneficiary1Relationship
Beneficiary2Name
Beneficiary2Relationship
...
JanuaryContribution
FebruaryContribution
...
```

Use child/transaction tables instead.

## Proposed core database tables

```text
Identity
  Users
  Roles / Permissions

Membership
  Members
  MemberEmployments
  MemberContacts
  MemberAddresses
  MemberEducationRecords
  MemberEligibilities
  MemberFamilyInformation
  MemberChildren
  MemberUpdateRequests
  MemberUpdateRequestChanges
  MemberStatusHistory

Beneficiaries
  MemberBeneficiaries
  MemberBeneficiaryHistory

Documents
  MemberDocuments

Privacy
  PrivacyNoticeVersions
  MemberPrivacyConsents

Imports
  ImportBatches
  MemberImportStaging
  ImportValidationErrors

Contributions
  ContributionBatches
  Contributions
  ContributionAdjustments

Loans
  LoanTypes
  LoanApplications
  LoanApplicationStatusHistory
  LoanApprovals
  Loans
  LoanPaymentSchedules
  LoanPayments
  LoanAdjustments

Elections
  Elections
  ElectionPositions
  ElectionCandidates
  ElectionEligibleVoters
  ElectionParticipation
  ElectionBallots
  ElectionVotes
  ElectionFinalizedResults

Audit
  AuditEvents
```

## Confirmed decisions (Buklod, 2026-08-14)

1. **BI Employee Number** is unique and mandatory for all Buklod members.
   `MemberEmployment.EmployeeNumber` gets a database-level unique constraint;
   member creation requires a value. (Resolves former question 1; feeds
   BIMSS-016.)
2. **Self-service direct edit** (no officer approval) is limited to contact
   information only (phone, email, mailing address). All other profile
   fields — name, BI Employee Number, employment, civil status, etc. — go
   through the officer review/approval workflow. (Resolves former question
   3; scopes Phase 1E's `MemberUpdateRequest` vs. direct-edit split —
   BIMSS-030 vs. BIMSS-042/044.)
3. **Proof of employment** is mandatory before member verification; accepted
   file types are PDF, JPG, and PNG. (Resolves former question 4; feeds
   BIMSS-021's file validation rules.)
4. **MemberChild** records require both name and birth date; birth date is
   not optional. (Resolves former question 5; feeds BIMSS-019.)

## Confirmed decisions (Buklod, 2026-08-17)

5. **Beneficiary shares/percentages are not required.** A beneficiary is
   just a name + relationship, matching how the Excel export already
   captures it (no percentage column). (Resolves former question 2; feeds
   BIMSS-048.)
6. **Beneficiary count is unlimited** — no hard cap enforced at the
   database level, consistent with `docs/DOMAIN_WORKFLOWS.md` #3's "no
   fixed maximum" note. (Resolves former question 3; feeds BIMSS-048.)
7. **Beneficiary changes require officer review/approval.** Members cannot
   directly edit their beneficiaries the way BIMSS-044 lets them directly
   edit contact info — every add/update/remove goes through the review
   workflow in `docs/DOMAIN_WORKFLOWS.md` #3. (Resolves former question 4;
   feeds BIMSS-049/051.)

Still open, and distinct from question 2 above: the Excel export's
free-text "Additional Beneficiaries (Beneficiary 5 and above)" column has
no agreed delimiter/format for splitting it into individual rows — see
`docs/PHASE2_BACKLOG.md`'s BIMSS-054 note.

## Confirmed decisions (Buklod, 2026-08-17, Contributions)

8. **Contribution amount is a fixed flat rate**, the same for every
   member — not tied to salary/rank/office. The rate itself can still
   change over time (e.g. a new amount effective a given date), so it
   needs an effective-dated rate table, not a per-member override.
   (Resolves former question 2; feeds BIMSS-057.)
9. **One contribution type only** — regular monthly dues. No separate
   special-assessment or other-fund categories to track distinctly for
   now. (Feeds BIMSS-059 — no `ContributionType` reference table needed
   yet; add one later if Buklod introduces a second contribution
   category.)
10. **Contributions are a pure ledger** — the system records what was
    actually received/imported. It does not maintain an "expected
    amount owed" or flag members who missed a contribution; that kind of
    report, if ever needed, is computed from the ledger after the fact,
    not from a dedicated expectation/schedule concept. (Feeds
    BIMSS-058/059 — no missing-contribution tracking in the schema.)

## Confirmed decisions (Buklod, 2026-08-17, Loans)

11. **One generic loan product for now.** A single configurable
    `LoanType` (rate/max amount/term/penalty as data, not hardcoded
    rules) — more products can be added as rows later without a schema
    change. (Feeds BIMSS-066.)
12. **Interest is a flat rate on the original principal**
    (`Interest = Principal × Rate × Term`), computed once at approval —
    not diminishing-balance amortization, not interest-free. (Feeds
    BIMSS-068's schedule generation.)
13. **A co-maker/guarantor is required** on every loan application —
    another member who co-signs. (Feeds BIMSS-067 — `LoanApplication`
    needs a required co-maker reference.)
14. **Payments are collected via payroll deduction**, arriving as a
    periodic batch import — same shape as Contributions' batch ingestion
    (BIMSS-060/061). (Feeds BIMSS-073.)
15. **A flat penalty fee applies per missed/late payment** (not a
    percentage). (Feeds BIMSS-066's `LoanType.PenaltyAmount` and
    BIMSS-073's posting logic.)
16. **Maximum loanable amount is a fixed cap per loan product**, not
    computed from a member's contribution balance — no cross-module
    dependency on the Contributions ledger. (Feeds BIMSS-066.)
17. **A member may have only one active loan at a time.** A new
    application isn't allowed while an existing loan is still active.
    (Feeds BIMSS-070's eligibility check.)

(Resolves former question 2; see `docs/PHASE4_BACKLOG.md`.)

## Confirmed decisions (Buklod, 2026-08-18, Elections)

18. **Positions are configured per election**, not a fixed constitutional
    set reused every cycle — each `Election` owns its own
    `ElectionPosition` rows, defined during setup. (Feeds BIMSS-077.)
19. **A position can have multiple seats** (e.g. a "Board Member"
    position with 5 winners) — the top-N candidates by vote count win,
    not just a single winner per position. (Feeds BIMSS-077's
    `SeatCount` field and BIMSS-083's tally logic.)
20. **Abstention is allowed** — a voter can leave any position blank and
    still submit a valid ballot. (Feeds BIMSS-079/082 — no
    "every position required" validation on ballot submission.)
21. **Voting eligibility is Active membership status only**, as of the
    voter-list freeze date — no additional tenure/standing criteria.
    (Feeds BIMSS-078.)

(Resolves former question 2; see `docs/PHASE5_BACKLOG.md`.)

## Confirmed decisions (Buklod, 2026-08-18, Notifications & Reports)

Not resolutions of a numbered open question (Notifications/Reports never
blocked a specific schema the way Contributions/Loans/Elections did) —
recorded here as scope preferences confirmed before drafting
`docs/PHASE6_BACKLOG.md`, the same way the others were confirmed before
their backlogs.

22. **Email delivery is needed**, not just in-app notifications. No
    SMTP/email-sending infrastructure exists yet (gap noted since
    BIMSS-005) — this phase adds it. (Feeds BIMSS-090.)
23. **Both officer-broadcast announcements and personal event-triggered
    notifications are in scope** (e.g. "your update request was
    approved," "loan application status changed," "election is open").
    (Feeds BIMSS-088/089/091.)
24. **Membership and Finance reports are built together** in this phase
    — both `Permission.Report.ViewMembership` and
    `Permission.Report.ViewFinance` (reserved since BIMSS-006) get real
    screens in the same phase rather than splitting across two. (Feeds
    BIMSS-095/096.)

## Questions to confirm with Buklod before final schema

1. Are retirees/former employees/honorary members possible?
2. What record-retention rules apply after membership ends?
3. Benefits (confirmed real 2026-08-18, not yet scoped — see
   `docs/DEVELOPMENT_ROADMAP.md` Phase 7): what benefit types exist
   (death/calamity/education/retirement/other), what eligibility rules
   apply, what payout amount rules apply, what approval workflow
   applies, and is it funded from the same pool as loans/contributions
   or a separate benefit fund?
