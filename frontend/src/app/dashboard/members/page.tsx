"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/lib/auth-context";
import type { MemberSummary } from "@/lib/types/member";
import { MembersTable } from "@/components/members-table";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export default function MembersPage() {
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
      <CardHeader>
        <CardTitle>Members</CardTitle>
        <CardDescription>Buklod membership roster.</CardDescription>
      </CardHeader>
      <CardContent>
        {error ? (
          <p className="text-sm text-destructive">{error}</p>
        ) : members ? (
          <MembersTable members={members} />
        ) : (
          <p className="text-sm text-muted-foreground">Loading…</p>
        )}
      </CardContent>
    </Card>
  );
}
