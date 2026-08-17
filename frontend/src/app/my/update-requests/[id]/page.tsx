"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { OctagonAlert } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import type { MemberUpdateRequestDetail } from "@/lib/types/member-update-request";
import { memberUpdateRequestStatusBadgeClassName } from "@/lib/member-update-request-status";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Breadcrumbs } from "@/components/breadcrumbs";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

// Read-only mirror of dashboard/update-requests/[id] — a member can see the
// outcome of their own request but not act on it; approve/reject stays an
// officer-only action on MemberUpdateRequestsController (Permission.Membership.Manage).
export default function MyUpdateRequestDetailPage() {
  const params = useParams<{ id: string }>();
  const { fetchWithAuth } = useAuth();

  const [request, setRequest] = useState<MemberUpdateRequestDetail | null>(null);
  const [notFound, setNotFound] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadRequest() {
      const response = await fetchWithAuth(`/api/my/update-requests/${params.id}`);
      if (cancelled) {
        return;
      }

      if (response.status === 404) {
        setNotFound(true);
        return;
      }

      if (!response.ok) {
        setLoadError(`Failed to load the update request (${response.status}).`);
        return;
      }

      setRequest((await response.json()) as MemberUpdateRequestDetail);
    }

    void loadRequest();

    return () => {
      cancelled = true;
    };
  }, [fetchWithAuth, params.id]);

  if (notFound) {
    return <p className="text-sm text-muted-foreground">Update request not found.</p>;
  }

  if (loadError) {
    return (
      <Alert variant="destructive">
        <OctagonAlert />
        <AlertDescription>{loadError}</AlertDescription>
      </Alert>
    );
  }

  if (!request) {
    return (
      <div className="flex flex-col gap-3.5">
        <Skeleton className="h-9 w-full max-w-md" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-4">
      <Breadcrumbs
        items={[
          { label: "My Buklod", href: "/my" },
          { label: "Update requests", href: "/my/update-requests" },
          { label: new Date(request.submittedAtUtc).toLocaleDateString() },
        ]}
      />

      <Card className="rounded-xl shadow-none">
        <CardHeader className="flex flex-row items-center gap-2.5">
          <CardTitle className="text-[14.5px] font-semibold">Update request</CardTitle>
          <Badge className={memberUpdateRequestStatusBadgeClassName[request.status]}>{request.status}</Badge>
        </CardHeader>
        <CardContent className="flex flex-col gap-6">
          <p className="text-[12.5px] text-muted-foreground">
            Submitted {new Date(request.submittedAtUtc).toLocaleString()}
            {request.reviewedAtUtc ? ` · Reviewed ${new Date(request.reviewedAtUtc).toLocaleString()}` : ""}
          </p>

          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Field</TableHead>
                <TableHead>Previous value</TableHead>
                <TableHead>Requested value</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {request.changes.map((change) => (
                <TableRow key={change.id}>
                  <TableCell className="font-medium">{change.fieldName}</TableCell>
                  <TableCell className="text-muted-foreground">{change.oldValue ?? "—"}</TableCell>
                  <TableCell>{change.newValue ?? "—"}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>

          {request.reviewRemarks ? (
            <div className="flex flex-col gap-1 border-t pt-[18px]">
              <span className="text-[11.5px] text-muted-foreground">Officer remarks</span>
              <span className="text-sm">{request.reviewRemarks}</span>
            </div>
          ) : null}
        </CardContent>
      </Card>
    </div>
  );
}
