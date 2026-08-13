---
applyTo: "**"
---

# Security and Privacy Instructions

BIMSS processes personal, beneficiary, employment, financial, and election information.

- Apply least privilege.
- Use permission/policy-based authorization.
- Protect state-changing browser requests against CSRF.
- Validate uploads and keep uploaded documents outside publicly executable/static locations.
- Never commit secrets.
- Never use production member data for development or AI prompts.
- Never log secret ballot content or a member's candidate selections.
- Never log full sensitive member profiles.
- Do not expose another member's contributions, loans, beneficiaries, or documents through predictable IDs.
- Protect against mass assignment by binding request DTOs, not domain/EF entities.
- All sensitive exports/reports require explicit authorization and auditability.
