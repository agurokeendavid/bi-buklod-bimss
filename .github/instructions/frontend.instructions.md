---
applyTo: "frontend/**/*.tsx,frontend/**/*.ts,frontend/**/*.css"
---

# Next.js / React / shadcn / Tailwind Instructions

- Tailwind CSS controls layout and responsive structure; shadcn/ui (Radix-based) provides components.
- Use a data-table library (e.g. TanStack Table) for data-heavy grids, filtering, lookups, export, and administrative workflows.
- Use semantic HTML for simple forms/content — don't reach for a data-table component to replace a plain list.
- Keep client-side logic minimal; authoritative decisions belong on the server (`Bimss.Api`). Never compute authoritative totals, balances, or eligibility in the browser.
- Prefer server components/data-fetching patterns where the framework supports them; keep client components focused on interactivity.
- Do not rely on a hidden/disabled button or client-side validation for security — the server enforces permissions regardless of what the UI shows.
- Encode/escape untrusted output (React does this by default for JSX text content — be deliberate about any `dangerouslySetInnerHTML` usage, which should essentially never be needed).
- Keep forms keyboard accessible and provide visible labels/validation messages.
- For data-table server operations (paging, filtering, sorting), validate authorization and permitted filters on the server (`Bimss.Api`), never trust client-supplied filter parameters as-is.
- Access token: keep in memory only (React context/state), never `localStorage`/`sessionStorage`. Refresh token: an httpOnly cookie set by `Bimss.Api` — the frontend never reads or stores it directly; the API client only needs to call `/api/auth/refresh` with `credentials: 'include'`. See `docs/SECURITY_AND_PRIVACY.md`.
- Add or update Playwright coverage for critical UI workflows.
