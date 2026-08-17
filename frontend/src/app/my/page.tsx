"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { OctagonAlert } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import type { MyProfile } from "@/lib/types/member";
import { memberStatusBadgeClassName } from "@/lib/member-status";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";

function FactBlock({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col gap-1">
      <span className="text-[11.5px] text-muted-foreground">{label}</span>
      <span className="text-sm font-semibold">{value}</span>
    </div>
  );
}

export default function MemberDashboardPage() {
  const { fetchWithAuth } = useAuth();
  const [profile, setProfile] = useState<MyProfile | null>(null);
  const [notLinked, setNotLinked] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadProfile() {
      const response = await fetchWithAuth("/api/my/profile");
      if (cancelled) {
        return;
      }

      if (response.status === 404) {
        setNotLinked(true);
        return;
      }

      if (!response.ok) {
        setLoadError(`Failed to load your profile (${response.status}).`);
        return;
      }

      setProfile((await response.json()) as MyProfile);
    }

    void loadProfile();

    return () => {
      cancelled = true;
    };
  }, [fetchWithAuth]);

  if (notLinked) {
    return (
      <Card className="rounded-xl shadow-none">
        <CardContent className="py-8 text-center text-sm text-muted-foreground">
          Your account isn&apos;t linked to a membership record yet. Contact a Membership Officer to set this up.
        </CardContent>
      </Card>
    );
  }

  if (loadError) {
    return (
      <Alert variant="destructive">
        <OctagonAlert />
        <AlertDescription>{loadError}</AlertDescription>
      </Alert>
    );
  }

  if (!profile) {
    return (
      <div className="flex flex-col gap-3.5">
        <Skeleton className="h-9 w-full max-w-md" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  const fullName = [profile.lastName, profile.firstName].join(", ") + (profile.suffixName ? ` ${profile.suffixName}` : "");

  return (
    <Card className="rounded-xl shadow-none">
      <CardHeader className="flex flex-row items-start justify-between">
        <div>
          <div className="flex items-center gap-2.5">
            <CardTitle className="text-[14.5px] font-semibold">{fullName}</CardTitle>
            <Badge className={memberStatusBadgeClassName[profile.status]}>{profile.status}</Badge>
          </div>
          <CardDescription>{profile.positionDesignation} · {profile.officeUnitName}</CardDescription>
        </div>
        <div className="flex items-center gap-2">
          <Link href="/my/contact" className={cn(buttonVariants({ variant: "outline", size: "sm" }))}>
            Update contact info
          </Link>
          <Link href="/my/update-request" className={cn(buttonVariants({ variant: "outline", size: "sm" }))}>
            Request a profile change
          </Link>
          <Link href="/my/update-requests" className={cn(buttonVariants({ variant: "outline", size: "sm" }))}>
            My update requests
          </Link>
        </div>
      </CardHeader>
      <CardContent className="grid grid-cols-2 gap-4 sm:grid-cols-3">
        <FactBlock label="Membership status" value={profile.status} />
        <FactBlock label="BI employee number" value={profile.employeeNumber} />
        <FactBlock label="Civil status" value={profile.civilStatusName} />
        <FactBlock label="Date of birth" value={new Date(profile.dateOfBirth).toLocaleDateString()} />
        <FactBlock label="Place of birth" value={profile.placeOfBirth} />
        <FactBlock
          label="Permanent appointment"
          value={profile.permanentAppointmentDate ? new Date(profile.permanentAppointmentDate).toLocaleDateString() : "—"}
        />
      </CardContent>
    </Card>
  );
}
