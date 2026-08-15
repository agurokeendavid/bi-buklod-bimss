"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth-context";
import type { MemberDetail, MemberDocument, MemberStatusHistoryEntry, ReferenceDataItem } from "@/lib/types/member";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button, buttonVariants } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import { MemberDocumentsPanel } from "@/components/member-documents-panel";
import { MemberStatusHistoryPanel } from "@/components/member-status-history-panel";
import { Breadcrumbs } from "@/components/breadcrumbs";
import { memberStatusBadgeClassName } from "@/lib/member-status";
import { cn } from "@/lib/utils";

type StatusAction = "verify" | "deactivate" | "reactivate";

const actionLabel: Record<StatusAction, string> = {
  verify: "Verify",
  deactivate: "Deactivate",
  reactivate: "Reactivate",
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
  const [statusReasons, setStatusReasons] = useState<ReferenceDataItem[]>([]);
  const [documents, setDocuments] = useState<MemberDocument[]>([]);
  const [statusHistory, setStatusHistory] = useState<MemberStatusHistoryEntry[]>([]);

  const [activeAction, setActiveAction] = useState<StatusAction | null>(null);
  const [remarks, setRemarks] = useState("");
  const [reasonId, setReasonId] = useState("");
  const [actionError, setActionError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function loadStatusHistory() {
    const response = await fetchWithAuth(`/api/members/${params.id}/status-history`);
    if (response.ok) {
      setStatusHistory((await response.json()) as MemberStatusHistoryEntry[]);
    }
  }

  useEffect(() => {
    let cancelled = false;

    async function loadMember() {
      const [memberResponse, reasonsResponse, documentsResponse, historyResponse] = await Promise.all([
        fetchWithAuth(`/api/members/${params.id}`),
        fetchWithAuth("/api/reference-data/member-status-reasons"),
        fetchWithAuth(`/api/members/${params.id}/documents`),
        fetchWithAuth(`/api/members/${params.id}/status-history`),
      ]);

      if (cancelled) {
        return;
      }

      if (memberResponse.status === 404) {
        setNotFound(true);
        return;
      }

      if (!memberResponse.ok) {
        setError(`Failed to load member (${memberResponse.status}).`);
        return;
      }

      const body = (await memberResponse.json()) as MemberDetail;
      setMember(body);

      if (reasonsResponse.ok) {
        setStatusReasons((await reasonsResponse.json()) as ReferenceDataItem[]);
      }

      if (documentsResponse.ok) {
        setDocuments((await documentsResponse.json()) as MemberDocument[]);
      }

      if (historyResponse.ok) {
        setStatusHistory((await historyResponse.json()) as MemberStatusHistoryEntry[]);
      }
    }

    void loadMember();

    return () => {
      cancelled = true;
    };
  }, [fetchWithAuth, params.id]);

  function openAction(action: StatusAction) {
    setActiveAction(action);
    setRemarks("");
    setReasonId("");
    setActionError(null);
  }

  function closeAction() {
    setActiveAction(null);
    setActionError(null);
  }

  async function handleActionSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!activeAction) {
      return;
    }

    setActionError(null);
    setIsSubmitting(true);

    try {
      const endpoint = `/api/members/${params.id}/${activeAction}`;
      const body =
        activeAction === "deactivate"
          ? { reasonId, remarks: remarks || null }
          : { remarks: remarks || null };

      const response = await fetchWithAuth(endpoint, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
      });

      if (!response.ok) {
        const problem = await response.json().catch(() => null);
        const detail = problem?.detail as string | undefined;
        setActionError(detail ?? `Failed to ${actionLabel[activeAction].toLowerCase()} member (${response.status}).`);
        return;
      }

      const updated = (await response.json()) as MemberDetail;
      setMember(updated);
      await loadStatusHistory();
      toast.success(`Member ${actionLabel[activeAction].toLowerCase()}d.`);
      setActiveAction(null);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="flex flex-col gap-4">
      <Breadcrumbs
        items={[
          { label: "Dashboard", href: "/dashboard" },
          { label: "Members", href: "/dashboard/members" },
          { label: member ? `${member.lastName}, ${member.firstName}` : "Member" },
        ]}
      />

      <Card>
        <CardHeader className="flex flex-row items-start justify-between">
          <div>
            <CardTitle>Member details</CardTitle>
            <CardDescription>Core identity and employment information.</CardDescription>
          </div>
          {member ? (
            <div className="flex gap-2">
              {member.status === "PendingVerification" ? (
                <Button size="sm" variant="outline" onClick={() => openAction("verify")}>
                  Verify
                </Button>
              ) : null}
              {member.status === "Active" ? (
                <Button
                  size="sm"
                  variant="outline"
                  className="border-amber-300 text-amber-700 hover:bg-amber-50"
                  onClick={() => openAction("deactivate")}
                >
                  Deactivate
                </Button>
              ) : null}
              {member.status === "Inactive" ? (
                <Button size="sm" variant="outline" onClick={() => openAction("reactivate")}>
                  Reactivate
                </Button>
              ) : null}
              <Link href={`/dashboard/members/${params.id}/edit`} className={cn(buttonVariants({ size: "sm" }))}>
                Edit
              </Link>
            </div>
          ) : null}
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          {notFound ? (
            <p className="text-sm text-muted-foreground">Member not found.</p>
          ) : error ? (
            <p className="text-sm text-destructive">{error}</p>
          ) : member ? (
            <>
              <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
                <Field label="Last name" value={member.lastName} />
                <Field label="First name" value={member.firstName} />
                <Field label="Middle name" value={member.middleName ?? "—"} />
                <Field label="Date of birth" value={member.dateOfBirth} />
                <Field label="Place of birth" value={member.placeOfBirth} />
                <div className="flex flex-col gap-1">
                  <span className="text-xs text-muted-foreground">Status</span>
                  <Badge variant="outline" className={cn("w-fit", memberStatusBadgeClassName[member.status])}>
                    {member.status}
                  </Badge>
                </div>
                <Field label="Employee number" value={member.employeeNumber ?? "—"} />
                <Field label="Position / designation" value={member.positionDesignation ?? "—"} />
                <Field label="Permanent appointment date" value={member.permanentAppointmentDate ?? "—"} />
                <Field label="Joining reason" value={member.joiningReason ?? "—"} />
              </div>

              {activeAction ? (
                <form
                  className="flex flex-col gap-3 rounded-lg border border-border p-4"
                  onSubmit={handleActionSubmit}
                >
                  <p className="text-sm font-medium">{actionLabel[activeAction]} this member</p>

                  {activeAction === "deactivate" ? (
                    <div className="flex flex-col gap-2">
                      <Label htmlFor="reasonId">Reason</Label>
                      <Select value={reasonId} onValueChange={(value) => setReasonId(value ?? "")} required>
                        <SelectTrigger id="reasonId">
                          <SelectValue placeholder="Select a reason">
                            {(value) => statusReasons.find((item) => item.id === value)?.name ?? "Select a reason"}
                          </SelectValue>
                        </SelectTrigger>
                        <SelectContent>
                          {statusReasons.map((item) => (
                            <SelectItem key={item.id} value={item.id}>
                              {item.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  ) : null}

                  <div className="flex flex-col gap-2">
                    <Label htmlFor="remarks">Remarks (optional)</Label>
                    <Textarea id="remarks" value={remarks} onChange={(event) => setRemarks(event.target.value)} />
                  </div>

                  {actionError ? (
                    <p role="alert" className="text-sm text-destructive">
                      {actionError}
                    </p>
                  ) : null}

                  <div className="flex gap-2">
                    <Button type="submit" size="sm" disabled={isSubmitting}>
                      {isSubmitting ? "Saving…" : `Confirm ${actionLabel[activeAction].toLowerCase()}`}
                    </Button>
                    <Button type="button" size="sm" variant="outline" onClick={closeAction} disabled={isSubmitting}>
                      Cancel
                    </Button>
                  </div>
                </form>
              ) : null}

              <Separator />
              <MemberDocumentsPanel
                memberId={params.id}
                documents={documents}
                onUploaded={(doc) => setDocuments((current) => [doc, ...current])}
              />

              <Separator />
              <MemberStatusHistoryPanel history={statusHistory} statusReasons={statusReasons} />
            </>
          ) : (
            <p className="text-sm text-muted-foreground">Loading…</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
