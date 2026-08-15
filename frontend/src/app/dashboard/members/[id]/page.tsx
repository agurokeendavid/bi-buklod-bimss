"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/lib/auth-context";
import type { MemberDetail } from "@/lib/types/member";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const statusBadgeVariant: Record<MemberDetail["status"], "default" | "secondary" | "outline"> = {
  PendingVerification: "secondary",
  Active: "default",
  Inactive: "outline",
};

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col gap-1">
      <span className="text-xs text-muted-foreground">{label}</span>
      <span className="text-sm">{value}</span>
    </div>
  );
}

export default function MemberDetailPage() {
  const params = useParams<{ id: string }>();
  const { fetchWithAuth } = useAuth();
  const [member, setMember] = useState<MemberDetail | null>(null);
  const [notFound, setNotFound] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadMember() {
      const response = await fetchWithAuth(`/api/members/${params.id}`);

      if (cancelled) {
        return;
      }

      if (response.status === 404) {
        setNotFound(true);
        return;
      }

      if (!response.ok) {
        setError(`Failed to load member (${response.status}).`);
        return;
      }

      const body = (await response.json()) as MemberDetail;
      if (!cancelled) {
        setMember(body);
      }
    }

    void loadMember();

    return () => {
      cancelled = true;
    };
  }, [fetchWithAuth, params.id]);

  return (
    <div className="flex flex-col gap-4">
      <Link href="/dashboard/members" className={cn(buttonVariants({ variant: "outline", size: "sm" }), "w-fit")}>
        ← Back to members
      </Link>

      <Card>
        <CardHeader className="flex flex-row items-start justify-between">
          <div>
            <CardTitle>Member details</CardTitle>
            <CardDescription>Core identity and employment information.</CardDescription>
          </div>
          {member ? (
            <Link href={`/dashboard/members/${params.id}/edit`} className={cn(buttonVariants({ size: "sm" }))}>
              Edit
            </Link>
          ) : null}
        </CardHeader>
        <CardContent>
          {notFound ? (
            <p className="text-sm text-muted-foreground">Member not found.</p>
          ) : error ? (
            <p className="text-sm text-destructive">{error}</p>
          ) : member ? (
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
              <Field label="Last name" value={member.lastName} />
              <Field label="First name" value={member.firstName} />
              <Field label="Middle name" value={member.middleName ?? "—"} />
              <Field label="Date of birth" value={member.dateOfBirth} />
              <Field label="Place of birth" value={member.placeOfBirth} />
              <div className="flex flex-col gap-1">
                <span className="text-xs text-muted-foreground">Status</span>
                <Badge variant={statusBadgeVariant[member.status]} className="w-fit">
                  {member.status}
                </Badge>
              </div>
              <Field label="Employee number" value={member.employeeNumber ?? "—"} />
              <Field label="Position / designation" value={member.positionDesignation ?? "—"} />
              <Field label="Permanent appointment date" value={member.permanentAppointmentDate ?? "—"} />
              <Field label="Joining reason" value={member.joiningReason ?? "—"} />
            </div>
          ) : (
            <p className="text-sm text-muted-foreground">Loading…</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
