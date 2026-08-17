"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useTheme } from "next-themes";
import { Moon, Sun } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import { decodeJwtDisplayName } from "@/lib/jwt";
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

// Member self-service's own header — no sidebar/mobile-nav toggle (this
// shell has no sidebar at all, per docs/PHASE1_BACKLOG.md's Phase 1E note:
// "distinct from the officer-facing /dashboard admin screens"), otherwise
// the same dark-mode toggle + avatar/sign-out pattern as AppHeader.
export function MemberHeader() {
  const router = useRouter();
  const { accessToken, logout } = useAuth();
  const { theme, setTheme } = useTheme();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
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
      <h1 className="text-[15px] font-semibold">BIMSS · My Buklod</h1>

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
