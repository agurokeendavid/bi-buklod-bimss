"use client";

import { useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useTheme } from "next-themes";
import { Menu, Moon, Sun } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import { decodeJwtDisplayName } from "@/lib/jwt";
import { activeNavLabel } from "@/lib/nav-items";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";

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
