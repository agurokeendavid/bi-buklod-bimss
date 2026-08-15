"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { CheckCircle2, Clock, Users, XCircle } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import type { MemberSummary } from "@/lib/types/member";
import { Card, CardContent } from "@/components/ui/card";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

interface StatCard {
  label: string;
  icon: typeof Users;
  iconClassName: string;
  href: string;
}

const STAT_CARDS: (StatCard & { key: "total" | "active" | "pending" | "inactive" })[] = [
  { key: "total", label: "Total members", icon: Users, iconClassName: "bg-primary/10 text-primary", href: "/dashboard/members" },
  {
    key: "active",
    label: "Active",
    icon: CheckCircle2,
    iconClassName: "bg-emerald-100 text-emerald-700",
    href: "/dashboard/members?status=Active",
  },
  {
    key: "pending",
    label: "Pending verification",
    icon: Clock,
    iconClassName: "bg-amber-100 text-amber-700",
    href: "/dashboard/members?status=PendingVerification",
  },
  {
    key: "inactive",
    label: "Inactive",
    icon: XCircle,
    iconClassName: "bg-slate-100 text-slate-700",
    href: "/dashboard/members?status=Inactive",
  },
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
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-semibold">Membership overview</h2>
          <p className="text-sm text-muted-foreground">Current standing of Buklod membership records.</p>
        </div>
        <Link href="/dashboard/members" className={cn(buttonVariants({ size: "sm" }))}>
          View all members
        </Link>
      </div>

      {error ? (
        <p className="text-sm text-destructive">{error}</p>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
          {STAT_CARDS.map((card) => {
            const Icon = card.icon;
            return (
              <Link key={card.key} href={card.href}>
                <Card className="transition-shadow hover:shadow-md">
                  <CardContent className="flex items-center gap-4">
                    <div className={cn("flex size-12 shrink-0 items-center justify-center rounded-full", card.iconClassName)}>
                      <Icon className="size-6" />
                    </div>
                    <div>
                      <p className="text-2xl font-bold">{members ? counts[card.key] : "—"}</p>
                      <p className="text-sm text-muted-foreground">{card.label}</p>
                    </div>
                  </CardContent>
                </Card>
              </Link>
            );
          })}
        </div>
      )}
    </div>
  );
}
