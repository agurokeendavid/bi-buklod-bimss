"use client";

import { useAuth } from "@/lib/auth-context";
import { decodeJwtDisplayName } from "@/lib/jwt";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export default function MemberDashboardPage() {
  const { accessToken } = useAuth();
  const displayName = accessToken ? decodeJwtDisplayName(accessToken) : null;

  return (
    <Card className="rounded-xl shadow-none">
      <CardHeader>
        <CardTitle className="text-[14.5px] font-semibold">{displayName ? `Welcome, ${displayName}` : "Welcome"}</CardTitle>
        <CardDescription>Your Buklod membership record, contributions, and requests.</CardDescription>
      </CardHeader>
      <CardContent>
        <p className="text-sm text-muted-foreground">
          Your profile isn&apos;t available here yet — this is on its way in an upcoming update.
        </p>
      </CardContent>
    </Card>
  );
}
