---
name: bimss-election-integrity
description: Implement or review BIMSS election and voting features with voter eligibility, one-vote enforcement, ballot secrecy, concurrency safety, finalization, and auditability.
---

# BIMSS Election Integrity Workflow

Read `docs/DOMAIN_WORKFLOWS.md` and `docs/SECURITY_AND_PRIVACY.md`.

Mandatory properties:

1. Only an eligible authenticated member can vote.
2. Voting is accepted only while the election is open.
3. One eligible voter can create at most one submitted ballot.
4. Duplicate-vote prevention must survive concurrent requests.
5. Participation data must not reveal candidate selections.
6. Ballot/vote rows must not contain MemberId for a secret election.
7. Ballot rules are validated server-side.
8. Candidate totals are not exposed while voting is open by default.
9. Close/finalize/publish actions require explicit permission and audit events.
10. Finalized results are derived from persisted valid ballots.
11. Tests cover concurrent/double-submit behavior and unauthorized access.

Before finishing, explicitly inspect the schema and code for any path that reconstructs `member -> candidate selection`.
