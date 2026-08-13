# BIMSS Security and Privacy Baseline

## Data classification

BIMSS contains sensitive organizational and personal data, including:

- member identity and contact information
- BI employment information
- family and beneficiary information
- uploaded documents
- contribution and loan records
- authentication/authorization data
- election participation data

Ballot selections require an additional confidentiality boundary.

## Core principles

1. Least privilege
2. Server-side authorization
3. Data minimization
4. Audit sensitive actions
5. Protect data in transit and at rest according to BI infrastructure policy
6. Do not expose production data to AI coding tools
7. Separate election participation from secret ballot contents

## Authentication

Use an approved authentication mechanism.

Potential directions:
- ASP.NET Core Identity for application accounts
- BI/enterprise SSO when available and approved

Do not invent a custom password hashing implementation.

## Authorization

Prefer permission/policy-based authorization.

Examples:
- a member may read only their own contributions
- finance may post contributions but not manage elections
- election committee may configure elections but should not gain unrestricted financial access
- system administrators should not automatically be able to link a voter to candidate selections

## Object-level authorization

Every endpoint that receives an ID must validate that the current user is allowed to access that specific object.

Never assume an ID is safe because it came from the UI.

## CSRF and browser security

State-changing MVC/browser operations require anti-forgery protection.

Use secure cookie settings and the application's approved HTTPS/TLS configuration.

## File uploads

For proof documents and future loan attachments:

- allow only approved extensions/types
- limit file size
- generate server-side storage names
- do not trust original file paths
- store outside executable/static web locations
- authorize every download
- log sensitive document access if required
- consider malware scanning based on BI infrastructure capabilities

## Logging

Do not log:
- passwords
- tokens/secrets
- complete government/employee identifiers when not required
- full addresses
- beneficiary data
- full loan details in generic diagnostic logs
- ballot candidate selections

Use structured logging with masked identifiers.

## Audit records

Audit records should capture:
- actor
- action
- object type/id
- timestamp
- result
- reason/remarks where relevant
- safe before/after metadata for sensitive changes

Avoid storing secret ballot content in general audit events.

## Election secrecy model

Recommended separation:

```text
ElectionEligibleVoter
  MemberId
  ElectionId
  eligibility/status

ElectionParticipation
  ElectionId
  eligible-voter reference
  voted timestamp
  receipt/reference
  NO candidate choice

ElectionBallot / ElectionVotes
  random ballot identifier
  election/position/candidate selections
  NO MemberId
```

Submission must be atomic enough that a voter cannot cast two ballots during concurrent requests.

Do not expose database-level relationships that allow ordinary administrators to reconstruct voter choice.

## Development / test data

Use synthetic names and identifiers.

Do not:
- commit the Google Forms export
- copy production rows into local seed data
- paste member records into GitHub issues
- send real personal/loan/beneficiary data to Claude, Copilot, Codex, or other external assistants unless explicitly approved under BI policy

## AI/MCP access

Allowed AI tools should operate on:
- source code
- documentation
- synthetic fixtures
- local/dev databases

Do not grant AI tools direct production SQL Server credentials.

If a database MCP/tool is used in development, use a separate non-production database and the minimum permissions needed.
