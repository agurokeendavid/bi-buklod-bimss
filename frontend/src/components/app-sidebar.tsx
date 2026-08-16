"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { X } from "lucide-react";
import { NAV_GROUP_LABELS, NAV_ITEMS, isNavItemActive, type NavGroup } from "@/lib/nav-items";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const NAV_GROUP_ORDER: NavGroup[] = ["operations", "administration"];

function SidebarContent({ pathname, onNavigate, onClose }: { pathname: string; onNavigate?: () => void; onClose?: () => void }) {
  return (
    <div className="flex h-full flex-col bg-sidebar text-sidebar-foreground">
      <div className="flex h-16 items-center gap-2.5 px-4">
        <div className="flex size-[34px] shrink-0 items-center justify-center rounded-full border-2 border-white/50 text-xs font-semibold">
          BI
        </div>
        <div className="flex min-w-0 flex-col leading-tight">
          <span className="truncate text-sm font-semibold">BIMSS</span>
          <span className="truncate text-[11px] text-white/70">Buklod ng Kawani</span>
        </div>
        {onClose ? (
          <Button
            variant="ghost"
            size="icon"
            onClick={onClose}
            aria-label="Close navigation"
            className="ml-auto shrink-0 text-white hover:bg-white/10 hover:text-white"
          >
            <X className="size-5" />
          </Button>
        ) : null}
      </div>
      <div className="flex-1 overflow-y-auto px-2 pb-2">
        {NAV_GROUP_ORDER.map((group) => {
          const items = NAV_ITEMS.filter((item) => item.group === group);
          if (items.length === 0) {
            return null;
          }
          return (
            <div key={group} className="mb-4">
              <h2 className="mb-1.5 px-3 text-[10px] font-semibold tracking-[.14em] text-white/50 uppercase">
                {NAV_GROUP_LABELS[group]}
              </h2>
              <nav className="flex flex-col gap-0.5">
                {items.map((item) => {
                  const isActive = isNavItemActive(item, pathname);
                  const Icon = item.icon;
                  return (
                    <Link
                      key={item.href}
                      href={item.href}
                      onClick={onNavigate}
                      className={cn(
                        "flex items-center gap-3 rounded-lg px-2.5 py-2 text-[13.5px] transition-colors",
                        isActive
                          ? "bg-sidebar-accent font-semibold text-sidebar-accent-foreground"
                          : "text-sidebar-foreground/90 hover:bg-white/10",
                      )}
                    >
                      <Icon className="size-4.5 shrink-0" />
                      {item.label}
                    </Link>
                  );
                })}
              </nav>
            </div>
          );
        })}
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
      {/* Desktop: fixed navy sidebar rail, sticky for its full height. */}
      <aside className="sticky top-0 hidden h-screen w-[242px] shrink-0 border-r border-sidebar-border md:block">
        <SidebarContent pathname={pathname} />
      </aside>

      {/* Mobile: slides in as an overlay, opened from AppHeader's hamburger button. */}
      {isMobileOpen ? (
        <div className="fixed inset-0 z-50 md:hidden">
          <div className="absolute inset-0 bg-black/40" onClick={() => onMobileOpenChange(false)} />
          <aside className="absolute inset-y-0 left-0 w-[242px] border-r border-sidebar-border shadow-lg">
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
