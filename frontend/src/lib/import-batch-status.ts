import type { ImportBatchStatus, ImportRowMatchStatus, ImportRowValidationStatus } from "@/lib/types/import-batch";

// Same centralized-badge-color convention as lib/member-status.ts — no
// component picks its own colors. Reuses the same semantic hues (green =
// good/done, amber = needs attention, zinc = neutral/not-yet, red = blocked).
export const importBatchStatusBadgeClassName: Record<ImportBatchStatus, string> = {
  Created: "border-transparent bg-[#f4f4f5] text-[#52525b] dark:bg-zinc-500/15 dark:text-zinc-400",
  Staged: "border-transparent bg-[#f4f4f5] text-[#52525b] dark:bg-zinc-500/15 dark:text-zinc-400",
  Validated: "border-transparent bg-[#fef9c3] text-[#854d0e] dark:bg-amber-500/15 dark:text-amber-400",
  Promoted: "border-transparent bg-[#dcfce7] text-[#166534] dark:bg-emerald-500/15 dark:text-emerald-400",
  Cancelled: "border-transparent bg-[#fee2e2] text-[#991b1b] dark:bg-red-500/15 dark:text-red-400",
};

export const importRowValidationStatusBadgeClassName: Record<ImportRowValidationStatus, string> = {
  NotValidated: "border-transparent bg-[#f4f4f5] text-[#52525b] dark:bg-zinc-500/15 dark:text-zinc-400",
  Valid: "border-transparent bg-[#dcfce7] text-[#166534] dark:bg-emerald-500/15 dark:text-emerald-400",
  Invalid: "border-transparent bg-[#fee2e2] text-[#991b1b] dark:bg-red-500/15 dark:text-red-400",
};

export const importRowMatchStatusBadgeClassName: Record<ImportRowMatchStatus, string> = {
  NotEvaluated: "border-transparent bg-[#f4f4f5] text-[#52525b] dark:bg-zinc-500/15 dark:text-zinc-400",
  NoMatch: "border-transparent bg-[#dcfce7] text-[#166534] dark:bg-emerald-500/15 dark:text-emerald-400",
  PossibleDuplicate: "border-transparent bg-[#fef9c3] text-[#854d0e] dark:bg-amber-500/15 dark:text-amber-400",
  ConfirmedDuplicate: "border-transparent bg-[#fee2e2] text-[#991b1b] dark:bg-red-500/15 dark:text-red-400",
};
