"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { OctagonAlert } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import type { MemberUpdateRequestSummary } from "@/lib/types/member-update-request";
import { memberUpdateRequestStatusBadgeClassName } from "@/lib/member-update-request-status";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { cn } from "@/lib/utils";

export default function UpdateRequestsPage() {
  const { fetchWithAuth } = useAuth();
  const [requests, setRequests] = useState<MemberUpdateRequestSummary[] | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadRequests() {
      const response = await fetchWithAuth("/api/update-requests?status=Pending");
      if (cancelled) {
        return;
      }

      if (!response.ok) {
        setLoadError(`Failed to load update requests (${response.status}).`);
        return;
      }

      setRequests((await response.json()) as MemberUpdateRequestSummary[]);
    }

    void loadRequests();

    return () => {
      cancelled = true;
    };
  }, [fetchWithAuth]);

  return (
    <Card className="rounded-xl shadow-none">
      <CardHeader>
        <CardTitle className="text-[14.5px] font-semibold">Update requests</CardTitle>
        <CardDescription>Profile changes members have submitted for review.</CardDescription>
      </CardHeader>
      <CardContent>
        {loadError ? (
          <Alert variant="destructive">
            <OctagonAlert />
            <AlertDescription>{loadError}</AlertDescription>
          </Alert>
        ) : requests ? (
          requests.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Member</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Submitted</TableHead>
                  <TableHead />
                </TableRow>
              </TableHeader>
              <TableBody>
                {requests.map((request) => (
                  <TableRow key={request.id}>
                    <TableCell className="font-medium">
                      {request.memberLastName}, {request.memberFirstName}
                    </TableCell>
                    <TableCell>
                      <Badge className={memberUpdateRequestStatusBadgeClassName[request.status]}>{request.status}</Badge>
                    </TableCell>
                    <TableCell className="text-muted-foreground">{new Date(request.submittedAtUtc).toLocaleString()}</TableCell>
                    <TableCell>
                      <Link
                        href={`/dashboard/update-requests/${request.id}`}
                        className={cn(buttonVariants({ variant: "outline", size: "sm" }))}
                      >
                        Review
                      </Link>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <p className="text-sm text-muted-foreground">No pending update requests.</p>
          )
        ) : (
          <div className="flex flex-col gap-3.5">
            <Skeleton className="h-9 w-full max-w-md" />
            <Skeleton className="h-48 w-full" />
          </div>
        )}
      </CardContent>
    </Card>
  );
}
