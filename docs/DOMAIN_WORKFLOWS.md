# BIMSS Domain Workflows

## 1. Existing member migration / update

```text
Create Import Batch
  -> Load spreadsheet rows to staging
  -> Validate required/format fields
  -> Match possible existing member
  -> Detect duplicate Employee Number / identity candidates
  -> Normalize beneficiaries and related records
  -> Flag ambiguous rows
  -> Reviewer confirms
  -> Create/update member through normal application services
  -> Record migration audit
```

Do not bypass domain validation during migration.

## 2. Member profile update

Recommended model:

```text
Member edits permitted fields
  -> Submit Update Request
  -> Pending Review
  -> Membership Officer reviews differences
  -> Approve / Reject
  -> Approved changes applied
  -> History/audit recorded
```

Low-risk fields may later be configured for direct self-service updates if Buklod approves that policy.

## 3. Beneficiary update

```text
Member proposes Add/Update/Remove
  -> Pending Review
  -> Authorized reviewer checks request
  -> Approve / Reject
  -> Effective beneficiary set updated
  -> Previous record retained in history
```

Beneficiaries are a collection with no fixed maximum at the database level.

## 4. Contribution posting

```text
Finance creates/imports Contribution Batch
  -> Validate rows and Member references
  -> Resolve errors
  -> Post batch
  -> Create immutable contribution transactions
  -> Member ledger/dashboard updated
  -> Audit recorded
```

Corrections should use a traceable adjustment/reversal workflow.

## 5. Loan application

Suggested initial lifecycle:

```text
Draft
  -> Submitted
  -> For Review
  -> For Approval
  -> Approved / Disapproved
  -> For Release
  -> Released
  -> Active
  -> Fully Paid / Closed
```

Other states may include Cancelled or Returned for Correction.

Each transition requires:
- current-state validation
- actor permission
- timestamp
- history record
- remarks/reason when required

## 6. Loan payment

```text
Payment received/imported
  -> Validate loan is eligible for posting
  -> Create payment transaction
  -> Recompute server-side balance/read model
  -> Mark fully paid when rules are satisfied
  -> Audit posting/adjustment
```

## 7. Election setup

```text
Create Election (Draft)
  -> Define positions
  -> Add candidates
  -> Define eligibility rules / freeze voter list
  -> Validate configuration
  -> Schedule/Open voting
```

## 8. Voting

```text
Authenticated member opens election
  -> Server checks election open
  -> Server checks member is eligible
  -> Server checks member has not already voted
  -> Member completes ballot
  -> Review ballot
  -> Confirm submission
  -> Server validates selections
  -> Atomically record participation + anonymous ballot
  -> Return non-secret vote receipt/reference
```

Important: participation proves that a member voted, but must not reveal candidate selections.

## 9. Election closing and results

```text
Voting period ends / authorized close
  -> Prevent new ballots
  -> Validate ballot counts/integrity
  -> Authorized finalization
  -> Persist finalized totals/results
  -> Publish approved result view
```

Do not expose live candidate totals during voting by default.

## 10. Sensitive report/export

```text
User requests report
  -> Check explicit permission
  -> Apply scope/filter restrictions
  -> Generate report
  -> Audit export where sensitive
```
