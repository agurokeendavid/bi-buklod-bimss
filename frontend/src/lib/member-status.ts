import type { MemberStatus } from "@/lib/types/member";

// Semantic status colors (success/pending/neutral) rather than reusing
// whatever the primary/secondary theme tokens happen to be — kept as a
// shared className map since both the members list and detail page need it.
export const memberStatusBadgeClassName: Record<MemberStatus, string> = {
  PendingVerification: "border-amber-200 bg-amber-100 text-amber-800",
  Active: "border-emerald-200 bg-emerald-100 text-emerald-800",
  Inactive: "border-slate-200 bg-slate-100 text-slate-700",
};
