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
| 12 | BI Employee Number | `MemberEmployment.EmployeeNumber` | nvarchar business identifier; unique after policy confirmation |
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

## Questions to confirm with Buklod before final schema

1. Is BI Employee Number unique and mandatory for all Buklod members?
2. Are retirees/former employees/honorary members possible?
3. Which profile fields may a member change without approval?
4. Is proof of permanent employment mandatory and what file types are accepted?
5. Should children be stored individually with birth dates, or is a names-only list sufficient?
6. Are beneficiary percentages/shares required?
7. Can a member have unlimited beneficiaries?
8. Does changing beneficiaries require officer approval?
9. What contribution amount/rules vary by member or year?
10. What official loan products, interest rules, terms, penalties, and eligibility rules exist?
11. What election positions and voting rules apply, including abstention and number of selections per position?
12. What record-retention rules apply after membership ends?
