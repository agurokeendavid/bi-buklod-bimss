"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter, usePathname } from "next/navigation";
import { useTheme } from "next-themes";
import { toast } from "sonner";
import { Bell, Menu, Moon, Search, Sun } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import { decodeJwtDisplayName } from "@/lib/jwt";
import { activeNavLabel } from "@/lib/nav-items";
import { Button, buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";

function initialsFor(name: string | null): string {
  if (!name) {
    return "?";
  }

  return name.slice(0, 2).toUpperCase();
}

export function AppHeader({ onOpenMobileNav }: { onOpenMobileNav: () => void }) {
  const router = useRouter();
  const pathname = usePathname();
  const { accessToken, logout } = useAuth();
  const { theme, setTheme } = useTheme();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    // next-themes only knows the persisted theme after mount (it reads
    // localStorage client-side); gating the toggle icon on `mounted`
    // avoids a server/client mismatch for which icon renders first. No
    // effect-free alternative for this one-shot mount detection.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setMounted(true);
  }, []);

  const handleLogout = async () => {
    await logout();
    router.push("/login");
  };

  const displayName = accessToken ? decodeJwtDisplayName(accessToken) : null;

  return (
    <header className="sticky top-0 z-40 flex h-14 items-center justify-between border-b border-border bg-card px-4 md:px-6">
      <div className="flex items-center gap-2">
        <Tooltip>
          <TooltipTrigger
            render={
              <Button variant="ghost" size="icon" className="md:hidden" onClick={onOpenMobileNav} aria-label="Open navigation">
                <Menu className="size-5" />
              </Button>
            }
          />
          <TooltipContent>Open navigation</TooltipContent>
        </Tooltip>
        <h1 className="text-[15px] font-semibold">{activeNavLabel(pathname)}</h1>
      </div>

      <div className="flex items-center gap-2">
        <div className="relative hidden lg:block">
          <Search className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground" />
          {/* Not wired to a real search yet — no cross-entity search endpoint exists.
              Present per docs/design/BIMSS-UI-SPEC.md's topbar anatomy; will search
              once that feature lands. */}
          <Input placeholder="Search…" className="w-[258px] pl-8" aria-label="Search (not yet available)" />
        </div>

        <Tooltip>
          <TooltipTrigger
            render={
              <Button
                variant="outline"
                size="icon"
                onClick={() => toast.info("Notifications aren't available yet — coming in a later phase.")}
                aria-label="Notifications"
              >
                <Bell className="size-5" />
              </Button>
            }
          />
          <TooltipContent>Notifications (coming soon)</TooltipContent>
        </Tooltip>

        <Link href="/dashboard/members/new" className={cn(buttonVariants({ size: "sm" }), "hidden sm:inline-flex")}>
          New member
        </Link>

        {mounted ? (
          <Tooltip>
            <TooltipTrigger
              render={
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
                  aria-label={theme === "dark" ? "Switch to light mode" : "Switch to dark mode"}
                >
                  {theme === "dark" ? <Sun className="size-5" /> : <Moon className="size-5" />}
                </Button>
              }
            />
            <TooltipContent>{theme === "dark" ? "Switch to light mode" : "Switch to dark mode"}</TooltipContent>
          </Tooltip>
        ) : (
          <div className="size-10" aria-hidden="true" />
        )}

        <DropdownMenu>
          <DropdownMenuTrigger className="rounded-full outline-none focus-visible:ring-3 focus-visible:ring-ring/50">
            <Avatar>
              <AvatarFallback>{initialsFor(displayName)}</AvatarFallback>
            </Avatar>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            {displayName ? (
              <div className="px-1.5 py-1 text-sm font-medium">{displayName}</div>
            ) : null}
            <DropdownMenuItem onClick={handleLogout}>Sign out</DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </header>
  );
}
