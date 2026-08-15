import { LayoutDashboard, Users, type LucideIcon } from "lucide-react";

export interface NavItem {
  href: string;
  label: string;
  icon: LucideIcon;
  exact?: boolean;
}

export const NAV_ITEMS: NavItem[] = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard, exact: true },
  { href: "/dashboard/members", label: "Members", icon: Users },
];

export function isNavItemActive(item: NavItem, pathname: string): boolean {
  return item.exact ? pathname === item.href : pathname.startsWith(item.href);
}

export function activeNavLabel(pathname: string): string {
  const active = [...NAV_ITEMS].reverse().find((item) => isNavItemActive(item, pathname));
  return active?.label ?? "BIMSS";
}
