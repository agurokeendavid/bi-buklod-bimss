import type { MemberStatus } from "@/lib/types/member";

// Status badge colors from docs/design/BIMSS-UI-SPEC.md §2 ("Status
// colors") — fixed hex, not theme tokens, since these are semantic
// (success/pending/neutral) and defined independently of the app's
// primary/secondary palette. Centralized here so no component picks its
// own colors — both the members list and detail page use this map. The
// spec doesn't cover dark mode, so the dark: variants below (translucent
// versions of the same hues) are this app's own extension, needed because
// dark mode is already a shipped feature.
export const memberStatusBadgeClassName: Record<MemberStatus, string> = {
  Active: "border-transparent bg-[#dcfce7] text-[#166534] dark:bg-emerald-500/15 dark:text-emerald-400",
  PendingVerification: "border-transparent bg-[#fef9c3] text-[#854d0e] dark:bg-amber-500/15 dark:text-amber-400",
  Inactive: "border-transparent bg-[#f4f4f5] text-[#52525b] dark:bg-zinc-500/15 dark:text-zinc-400",
};
