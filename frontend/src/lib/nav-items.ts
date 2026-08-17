import { ClipboardCheck, FileSpreadsheet, LayoutDashboard, Users, type LucideIcon } from "lucide-react";

export type NavGroup = "operations" | "administration";

export const NAV_GROUP_LABELS: Record<NavGroup, string> = {
  operations: "Operations",
  administration: "Administration",
};

export interface NavItem {
  href: string;
  label: string;
  icon: LucideIcon;
  group: NavGroup;
  exact?: boolean;
}

// Only routes that actually exist today. Later phases (Applications,
// Contributions, Loans, Approvals, Reports under "operations"; Settings,
// Audit log under "administration") add their own entries once their
// screens ship — see docs/design/BIMSS-UI-SPEC.md §4 for the full target
// nav and docs/PHASE1_BACKLOG.md for what's next.
export const NAV_ITEMS: NavItem[] = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard, group: "operations", exact: true },
  { href: "/dashboard/members", label: "Membership register", icon: Users, group: "operations" },
  { href: "/dashboard/import-batches", label: "Member imports", icon: FileSpreadsheet, group: "operations" },
  { href: "/dashboard/update-requests", label: "Update requests", icon: ClipboardCheck, group: "operations" },
];

export function isNavItemActive(item: NavItem, pathname: string): boolean {
  return item.exact ? pathname === item.href : pathname.startsWith(item.href);
}

export function activeNavLabel(pathname: string): string {
  const active = [...NAV_ITEMS].reverse().find((item) => isNavItemActive(item, pathname));
  return active?.label ?? "BIMSS";
}
