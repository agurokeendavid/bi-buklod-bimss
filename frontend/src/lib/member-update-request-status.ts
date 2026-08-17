import type { MemberUpdateRequestStatus } from "@/lib/types/member-update-request";

// Same centralized-badge-color convention as lib/member-status.ts.
export const memberUpdateRequestStatusBadgeClassName: Record<MemberUpdateRequestStatus, string> = {
  Pending: "border-transparent bg-[#fef9c3] text-[#854d0e] dark:bg-amber-500/15 dark:text-amber-400",
  Approved: "border-transparent bg-[#dcfce7] text-[#166534] dark:bg-emerald-500/15 dark:text-emerald-400",
  Rejected: "border-transparent bg-[#fee2e2] text-[#991b1b] dark:bg-red-500/15 dark:text-red-400",
};
