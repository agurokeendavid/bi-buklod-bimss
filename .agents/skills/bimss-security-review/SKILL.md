---
name: bimss-security-review
description: Perform a security/privacy review of BIMSS code or a proposed change, especially authentication, authorization, PII, file uploads, finance, APIs, logs, and election data.
---

# BIMSS Security Review

Read `docs/SECURITY_AND_PRIVACY.md`.

Review the changed/requested scope for:

1. Authentication assumptions
2. Permission/policy authorization
3. Object-level authorization
4. CSRF for browser state changes
5. Input validation and over-posting
6. SQL/query safety
7. Sensitive response/DTO exposure
8. Sensitive logging/error leakage
9. Upload/download safety
10. Secret/config handling
11. Financial auditability
12. Election voter/ballot separation
13. Concurrency/integrity abuse
14. Tests proving protected behavior

Report findings by severity and point to the affected file/behavior.
Prefer concrete fixes over generic security advice.
