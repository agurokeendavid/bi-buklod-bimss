"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { X } from "lucide-react";
import { NAV_ITEMS, isNavItemActive } from "@/lib/nav-items";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

function SidebarContent({ pathname, onNavigate, onClose }: { pathname: string; onNavigate?: () => void; onClose?: () => void }) {
  return (
    <div className="flex h-full flex-col bg-sidebar text-sidebar-foreground">
      <div className="flex h-16 items-center justify-between px-4">
        <span className="text-lg font-semibold">BIMSS</span>
        {onClose ? (
          <Button variant="ghost" size="icon" onClick={onClose} aria-label="Close navigation">
            <X className="size-5" />
          </Button>
        ) : null}
      </div>
      <div className="flex-1 overflow-y-auto p-2">
        <h2 className="mb-2 px-3 text-xs font-medium tracking-wide text-muted-foreground uppercase">Main menu</h2>
        <nav className="flex flex-col gap-1">
          {NAV_ITEMS.map((item) => {
            const isActive = isNavItemActive(item, pathname);
            const Icon = item.icon;
            return (
              <Link
                key={item.href}
                href={item.href}
                onClick={onNavigate}
                className={cn(
                  "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-primary/10 text-primary"
                    : "text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
                )}
              >
                <Icon className="size-5 shrink-0" />
                {item.label}
              </Link>
            );
          })}
        </nav>
      </div>
    </div>
  );
}

export function AppSidebar({
  isMobileOpen,
  onMobileOpenChange,
}: {
  isMobileOpen: boolean;
  onMobileOpenChange: (open: boolean) => void;
}) {
  const pathname = usePathname();

  return (
    <>
      {/* Desktop: fixed sidebar column. */}
      <aside className="hidden w-64 shrink-0 border-r border-sidebar-border md:block">
        <SidebarContent pathname={pathname} />
      </aside>

      {/* Mobile: slides in as an overlay, opened from AppHeader's hamburger button. */}
      {isMobileOpen ? (
        <div className="fixed inset-0 z-50 md:hidden">
          <div className="absolute inset-0 bg-black/40" onClick={() => onMobileOpenChange(false)} />
          <aside className="absolute inset-y-0 left-0 w-64 border-r border-sidebar-border shadow-lg">
            <SidebarContent
              pathname={pathname}
              onNavigate={() => onMobileOpenChange(false)}
              onClose={() => onMobileOpenChange(false)}
            />
          </aside>
        </div>
      ) : null}
    </>
  );
}
