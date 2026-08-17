"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { toast } from "sonner";
import { OctagonAlert } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import type { MemberUpdateRequestDetail } from "@/lib/types/member-update-request";
import { memberUpdateRequestStatusBadgeClassName } from "@/lib/member-update-request-status";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Breadcrumbs } from "@/components/breadcrumbs";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Textarea } from "@/components/ui/textarea";

export default function UpdateRequestDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const { fetchWithAuth } = useAuth();

  const [request, setRequest] = useState<MemberUpdateRequestDetail | null>(null);
  const [notFound, setNotFound] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [remarks, setRemarks] = useState("");
  const [actionError, setActionError] = useState<string | null>(null);
  const [pendingAction, setPendingAction] = useState<"approve" | "reject" | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadRequest() {
      const response = await fetchWithAuth(`/api/update-requests/${params.id}`);
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

  async function handleReview(action: "approve" | "reject") {
    if (action === "reject" && !remarks.trim()) {
      setActionError("Remarks are required to reject an update request.");
      return;
    }

    setActionError(null);
    setPendingAction(action);

    try {
      const response = await fetchWithAuth(`/api/update-requests/${params.id}/${action}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ remarks: remarks || null }),
      });

      if (!response.ok) {
        const problem = await response.json().catch(() => null);
        setActionError((problem?.detail as string | undefined) ?? `Failed to ${action} the request (${response.status}).`);
        return;
      }

      const updated = (await response.json()) as MemberUpdateRequestDetail;
      setRequest(updated);
      toast.success(action === "approve" ? "Update request approved." : "Update request rejected.");
      router.push("/dashboard/update-requests");
    } finally {
      setPendingAction(null);
    }
  }

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

  const isPending = request.status === "Pending";

  return (
    <div className="flex flex-col gap-4">
      <Breadcrumbs
        items={[
          { label: "Dashboard", href: "/dashboard" },
          { label: "Update requests", href: "/dashboard/update-requests" },
          { label: `${request.memberLastName}, ${request.memberFirstName}` },
        ]}
      />

      <Card className="rounded-xl shadow-none">
        <CardHeader className="flex flex-row items-center gap-2.5">
          <CardTitle className="text-[14.5px] font-semibold">
            {request.memberLastName}, {request.memberFirstName}
          </CardTitle>
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
                <TableHead>Current value</TableHead>
                <TableHead>Proposed value</TableHead>
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
            <div className="flex flex-col gap-1">
              <span className="text-[11.5px] text-muted-foreground">Review remarks</span>
              <span className="text-sm">{request.reviewRemarks}</span>
            </div>
          ) : null}

          {isPending ? (
            <div className="flex flex-col gap-3 border-t pt-[18px]">
              <div className="flex flex-col gap-2">
                <Label htmlFor="remarks">Remarks</Label>
                <Textarea
                  id="remarks"
                  placeholder="Recorded in the audit trail and visible to the member"
                  value={remarks}
                  onChange={(event) => setRemarks(event.target.value)}
                />
              </div>

              {actionError ? (
                <p role="alert" className="text-sm text-destructive">
                  {actionError}
                </p>
              ) : null}

              <div className="flex flex-wrap gap-2.5">
                <Button type="button" disabled={pendingAction !== null} onClick={() => handleReview("approve")}>
                  {pendingAction === "approve" ? "Approving…" : "Approve"}
                </Button>
                <Button type="button" variant="outline" disabled={pendingAction !== null} onClick={() => handleReview("reject")}>
                  {pendingAction === "reject" ? "Rejecting…" : "Reject"}
                </Button>
              </div>
            </div>
          ) : null}
        </CardContent>
      </Card>
    </div>
  );
}
