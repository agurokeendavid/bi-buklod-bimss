"use client";

import { useState } from "react";
import { useAuth } from "@/lib/auth-context";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

// Calls a real protected Bimss.Api endpoint through fetchWithAuth to prove
// the bearer-token (and 401-triggered refresh) flow works end to end —
// BIMSS-027 onward replaces this with real Membership screens.
export default function DashboardPage() {
  const { fetchWithAuth } = useAuth();
  const [result, setResult] = useState<string | null>(null);

  const checkProtectedEndpoint = async () => {
    setResult(null);
    const response = await fetchWithAuth("/api/diagnostics/authorized-ping");
    setResult(response.ok ? `OK (${response.status})` : `Failed (${response.status})`);
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Welcome</CardTitle>
        <CardDescription>
          Frontend scaffold placeholder — Membership admin screens land in BIMSS-027 onward.
        </CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col items-start gap-3">
        <Button variant="outline" onClick={checkProtectedEndpoint}>
          Call a protected Bimss.Api endpoint
        </Button>
        {result ? <p className="text-sm text-muted-foreground">{result}</p> : null}
      </CardContent>
    </Card>
  );
}
