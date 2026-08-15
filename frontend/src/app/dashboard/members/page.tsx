"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useAuth } from "@/lib/auth-context";
import type { MemberStatus, MemberSummary } from "@/lib/types/member";
import { MembersTable } from "@/components/members-table";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const VALID_STATUSES: MemberStatus[] = ["PendingVerification", "Active", "Inactive"];

export default function MembersPage() {
  const { fetchWithAuth } = useAuth();
  const searchParams = useSearchParams();
  const [members, setMembers] = useState<MemberSummary[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  const statusParam = searchParams.get("status");
  const initialStatusFilter = VALID_STATUSES.find((status) => status === statusParam);

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

      const body = (await response.json()) as MemberSummary[];
      if (!cancelled) {
        setMembers(body);
      }
    }

    void loadMembers();

    return () => {
      cancelled = true;
    };
  }, [fetchWithAuth]);

  return (
    <Card>
      <CardHeader className="flex flex-row items-start justify-between">
        <div>
          <CardTitle>Members</CardTitle>
          <CardDescription>Buklod membership roster.</CardDescription>
        </div>
        <Link href="/dashboard/members/new" className={cn(buttonVariants({ size: "sm" }))}>
          Create member
        </Link>
      </CardHeader>
      <CardContent>
        {error ? (
          <p className="text-sm text-destructive">{error}</p>
        ) : members ? (
          // `key` forces a remount when the URL's `?status=` filter changes —
          // MembersTable only reads `initialStatusFilter` once (via
          // `useState`'s initial value), and Next.js reuses this page's
          // component instance for a search-param-only navigation (e.g.
          // clicking a different dashboard stat card while already here),
          // so without this the filter wouldn't update.
          <MembersTable key={statusParam ?? "all"} members={members} initialStatusFilter={initialStatusFilter} />
        ) : (
          <p className="text-sm text-muted-foreground">Loading…</p>
        )}
      </CardContent>
    </Card>
  );
}
