"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { OctagonAlert } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import type { MemberSummary } from "@/lib/types/member";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

interface StatCard {
  label: string;
  note: string;
  href: string;
}

const STAT_CARDS: (StatCard & { key: "total" | "active" | "pending" | "inactive" })[] = [
  { key: "total", label: "Total members", note: "All membership records", href: "/dashboard/members" },
  { key: "active", label: "Active", note: "Verified, in good standing", href: "/dashboard/members?status=Active" },
  { key: "pending", label: "Pending verification", note: "Awaiting officer review", href: "/dashboard/members?status=PendingVerification" },
  { key: "inactive", label: "Inactive", note: "Deactivated records", href: "/dashboard/members?status=Inactive" },
];

export default function DashboardPage() {
  const { fetchWithAuth } = useAuth();
  const [members, setMembers] = useState<MemberSummary[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadMembers() {
      const response = await fetchWithAuth("/api/members");

      if (cancelled) {
        return;
      }

      if (!response.ok) {
        setError(`Failed to load members (${response.status}).`);
        return;
      }

      setMembers((await response.json()) as MemberSummary[]);
    }

    void loadMembers();

    return () => {
      cancelled = true;
    };
  }, [fetchWithAuth]);

  const counts = {
    total: members?.length ?? 0,
    active: members?.filter((member) => member.status === "Active").length ?? 0,
    pending: members?.filter((member) => member.status === "PendingVerification").length ?? 0,
    inactive: members?.filter((member) => member.status === "Inactive").length ?? 0,
  };

  return (
    <div className="flex flex-col gap-3.5">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-[15px] font-semibold">Membership overview</h2>
          <p className="text-xs text-muted-foreground">Current standing of Buklod membership records.</p>
        </div>
        <Link href="/dashboard/members" className={cn(buttonVariants({ size: "sm" }))}>
          View all members
        </Link>
      </div>

      {error ? (
        <Alert variant="destructive">
          <OctagonAlert />
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      ) : (
        <div className="grid grid-cols-1 gap-3.5 sm:grid-cols-2 xl:grid-cols-4">
          {STAT_CARDS.map((card) => (
            <Link key={card.key} href={card.href}>
              <Card className="rounded-xl py-4.5 shadow-none transition-colors hover:border-primary/40">
                <CardContent className="flex flex-col gap-1.5 px-5">
                  <p className="text-xs text-muted-foreground">{card.label}</p>
                  {members ? (
                    <p className="text-[27px] font-semibold tracking-tighter tabular-nums">{counts[card.key]}</p>
                  ) : (
                    <Skeleton className="h-[33px] w-16" />
                  )}
                  <p className="text-xs text-muted-foreground">{card.note}</p>
                </CardContent>
              </Card>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
