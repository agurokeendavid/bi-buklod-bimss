"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth-context";
import type { MemberDetail, MemberDocument, MemberStatusHistoryEntry, ReferenceDataItem } from "@/lib/types/member";
import { OctagonAlert } from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Button, buttonVariants } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { MemberDocumentsPanel } from "@/components/member-documents-panel";
import { MemberStatusHistoryPanel } from "@/components/member-status-history-panel";
import { Breadcrumbs } from "@/components/breadcrumbs";
import { memberStatusBadgeClassName, memberStatusLabel } from "@/lib/member-status";
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

function FactBlock({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col gap-1">
      <span className="text-[11.5px] text-muted-foreground">{label}</span>
      <span className="text-sm font-semibold tabular-nums">{value}</span>
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

      {notFound ? (
        <Card className="rounded-xl shadow-none">
          <CardContent className="py-8 text-center text-sm text-muted-foreground">Member not found.</CardContent>
        </Card>
      ) : error ? (
        <Alert variant="destructive">
          <OctagonAlert />
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      ) : !member ? (
        <Card className="rounded-xl shadow-none">
          <CardContent className="flex flex-col gap-4 py-6">
            <Skeleton className="h-19 w-19 rounded-lg" />
            <Skeleton className="h-6 w-64" />
            <Skeleton className="h-16 w-full" />
          </CardContent>
        </Card>
      ) : (
        <>
          <Card className="rounded-xl shadow-none">
            <CardContent className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
              <div className="flex flex-1 gap-4">
                <div
                  aria-hidden="true"
                  className="flex size-19 shrink-0 items-center justify-center rounded-lg text-[9.5px] text-muted-foreground"
                  style={{
                    backgroundImage:
                      "repeating-linear-gradient(135deg, var(--muted) 0 6px, color-mix(in oklch, var(--muted), var(--foreground) 4%) 6px 12px)",
                  }}
                >
                  ID photo
                </div>
                <div className="flex flex-col gap-3">
                  <div>
                    <div className="flex flex-wrap items-center gap-2">
                      <h2 className="text-[21px] font-semibold tracking-tight">
                        {member.lastName}, {member.firstName}
                        {member.middleName ? ` ${member.middleName.charAt(0)}.` : ""}
                      </h2>
                      <Badge variant="outline" className={memberStatusBadgeClassName[member.status]}>
                        {memberStatusLabel[member.status]}
                      </Badge>
                    </div>
                    <p className="text-[13px] text-muted-foreground">{member.positionDesignation ?? "Position not on file"}</p>
                  </div>
                  <div className="flex flex-wrap gap-x-6.5 gap-y-3">
                    <FactBlock label="Membership ID" value={member.id} />
                    <FactBlock label="Employee no." value={member.employeeNumber ?? "—"} />
                    <FactBlock label="Permanent appointment" value={member.permanentAppointmentDate ?? "—"} />
                  </div>
                </div>
              </div>
              <div className="flex shrink-0 gap-2">
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
                  Edit record
                </Link>
              </div>
            </CardContent>
            {activeAction ? (
              <CardContent className="pt-0">
                <form className="flex flex-col gap-3 rounded-lg border border-border p-4" onSubmit={handleActionSubmit}>
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
              </CardContent>
            ) : null}
          </Card>

          <Tabs defaultValue="personal">
            <TabsList variant="line">
              <TabsTrigger value="personal">Personal</TabsTrigger>
              <TabsTrigger value="documents">Documents</TabsTrigger>
              <TabsTrigger value="audit">Audit trail</TabsTrigger>
            </TabsList>
            <TabsContent value="personal" className="mt-3">
              <Card className="rounded-xl shadow-none">
                <CardContent className="grid grid-cols-2 gap-4 sm:grid-cols-3">
                  <Field label="Last name" value={member.lastName} />
                  <Field label="First name" value={member.firstName} />
                  <Field label="Middle name" value={member.middleName ?? "—"} />
                  <Field label="Date of birth" value={member.dateOfBirth} />
                  <Field label="Place of birth" value={member.placeOfBirth} />
                  <Field label="Employee number" value={member.employeeNumber ?? "—"} />
                  <Field label="Position / designation" value={member.positionDesignation ?? "—"} />
                  <Field label="Permanent appointment date" value={member.permanentAppointmentDate ?? "—"} />
                  <Field label="Joining reason" value={member.joiningReason ?? "—"} />
                </CardContent>
              </Card>
            </TabsContent>
            <TabsContent value="documents" className="mt-3">
              <Card className="rounded-xl shadow-none">
                <CardContent>
                  <MemberDocumentsPanel
                    memberId={params.id}
                    documents={documents}
                    onUploaded={(doc) => setDocuments((current) => [doc, ...current])}
                  />
                </CardContent>
              </Card>
            </TabsContent>
            <TabsContent value="audit" className="mt-3">
              <Card className="rounded-xl shadow-none">
                <CardContent>
                  <MemberStatusHistoryPanel history={statusHistory} statusReasons={statusReasons} />
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </>
      )}
    </div>
  );
}
