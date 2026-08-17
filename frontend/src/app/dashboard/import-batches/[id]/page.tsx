"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { toast } from "sonner";
import { OctagonAlert } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import type { ImportBatchDetail, ImportValidationErrorEntry, MemberImportStagingRow } from "@/lib/types/import-batch";
import {
  importBatchStatusBadgeClassName,
  importRowMatchStatusBadgeClassName,
  importRowValidationStatusBadgeClassName,
} from "@/lib/import-batch-status";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Breadcrumbs } from "@/components/breadcrumbs";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { cn } from "@/lib/utils";

function FactBlock({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col gap-1">
      <span className="text-[11.5px] text-muted-foreground">{label}</span>
      <span className="text-sm font-semibold tabular-nums">{value}</span>
    </div>
  );
}

export default function ImportBatchDetailPage() {
  const params = useParams<{ id: string }>();
  const { fetchWithAuth } = useAuth();

  const [batch, setBatch] = useState<ImportBatchDetail | null>(null);
  const [rows, setRows] = useState<MemberImportStagingRow[]>([]);
  const [errors, setErrors] = useState<ImportValidationErrorEntry[]>([]);
  const [notFound, setNotFound] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [isValidating, setIsValidating] = useState(false);
  const [isMatching, setIsMatching] = useState(false);
  const [promotingRowId, setPromotingRowId] = useState<string | null>(null);

  async function loadRowsAndErrors() {
    const [rowsResponse, errorsResponse] = await Promise.all([
      fetchWithAuth(`/api/import-batches/${params.id}/rows`),
      fetchWithAuth(`/api/import-batches/${params.id}/errors`),
    ]);

    if (rowsResponse.ok) {
      setRows((await rowsResponse.json()) as MemberImportStagingRow[]);
    }

    if (errorsResponse.ok) {
      setErrors((await errorsResponse.json()) as ImportValidationErrorEntry[]);
    }
  }

  useEffect(() => {
    let cancelled = false;

    async function loadBatch() {
      const [batchResponse, rowsResponse, errorsResponse] = await Promise.all([
        fetchWithAuth(`/api/import-batches/${params.id}`),
        fetchWithAuth(`/api/import-batches/${params.id}/rows`),
        fetchWithAuth(`/api/import-batches/${params.id}/errors`),
      ]);

      if (cancelled) {
        return;
      }

      if (batchResponse.status === 404) {
        setNotFound(true);
        return;
      }

      if (!batchResponse.ok || !rowsResponse.ok || !errorsResponse.ok) {
        setLoadError(`Failed to load import batch (${batchResponse.status}).`);
        return;
      }

      setBatch((await batchResponse.json()) as ImportBatchDetail);
      setRows((await rowsResponse.json()) as MemberImportStagingRow[]);
      setErrors((await errorsResponse.json()) as ImportValidationErrorEntry[]);
    }

    void loadBatch();

    return () => {
      cancelled = true;
    };
  }, [fetchWithAuth, params.id]);

  async function handleValidate() {
    setIsValidating(true);
    try {
      const response = await fetchWithAuth(`/api/import-batches/${params.id}/validate`, { method: "POST" });
      if (!response.ok) {
        const problem = await response.json().catch(() => null);
        toast.error((problem?.detail as string | undefined) ?? `Failed to validate the batch (${response.status}).`);
        return;
      }

      setBatch((await response.json()) as ImportBatchDetail);
      await loadRowsAndErrors();
      toast.success("Batch validated.");
    } finally {
      setIsValidating(false);
    }
  }

  async function handleMatch() {
    setIsMatching(true);
    try {
      const response = await fetchWithAuth(`/api/import-batches/${params.id}/match`, { method: "POST" });
      if (!response.ok) {
        const problem = await response.json().catch(() => null);
        toast.error((problem?.detail as string | undefined) ?? `Failed to match the batch (${response.status}).`);
        return;
      }

      setBatch((await response.json()) as ImportBatchDetail);
      await loadRowsAndErrors();
      toast.success("Duplicate matching complete.");
    } finally {
      setIsMatching(false);
    }
  }

  async function handlePromote(row: MemberImportStagingRow) {
    setPromotingRowId(row.id);
    try {
      const response = await fetchWithAuth(`/api/import-batches/${params.id}/rows/${row.id}/promote`, { method: "POST" });
      if (!response.ok) {
        const problem = await response.json().catch(() => null);
        toast.error((problem?.detail as string | undefined) ?? `Failed to promote row ${row.rowNumber} (${response.status}).`);
        return;
      }

      await loadRowsAndErrors();
      toast.success(`Row ${row.rowNumber} promoted to a member record.`);
    } finally {
      setPromotingRowId(null);
    }
  }

  if (notFound) {
    return <p className="text-sm text-muted-foreground">Import batch not found.</p>;
  }

  if (loadError) {
    return (
      <Alert variant="destructive">
        <OctagonAlert />
        <AlertDescription>{loadError}</AlertDescription>
      </Alert>
    );
  }

  if (!batch) {
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
          { label: "Dashboard", href: "/dashboard" },
          { label: "Member imports", href: "/dashboard/import-batches" },
          { label: batch.fileName },
        ]}
      />

      <Card className="rounded-xl shadow-none">
        <CardHeader className="flex flex-row items-start justify-between">
          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2.5">
              <CardTitle className="text-[14.5px] font-semibold">{batch.fileName}</CardTitle>
              <Badge className={importBatchStatusBadgeClassName[batch.status]}>{batch.status}</Badge>
            </div>
            <span className="text-[12.5px] text-muted-foreground">
              Uploaded {new Date(batch.uploadedAtUtc).toLocaleString()}
            </span>
          </div>
          <div className="flex gap-2.5">
            <Button type="button" variant="outline" size="sm" disabled={batch.status !== "Staged" || isValidating} onClick={handleValidate}>
              {isValidating ? "Validating…" : "Validate"}
            </Button>
            <Button type="button" variant="outline" size="sm" disabled={batch.status !== "Validated" || isMatching} onClick={handleMatch}>
              {isMatching ? "Matching…" : "Match against existing members"}
            </Button>
          </div>
        </CardHeader>
        <CardContent className="flex flex-col gap-6">
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
            <FactBlock label="Rows staged" value={batch.rowCount?.toString() ?? "—"} />
            <FactBlock label="Validated" value={batch.validatedAtUtc ? new Date(batch.validatedAtUtc).toLocaleString() : "—"} />
            <FactBlock label="Promoted" value={batch.promotedAtUtc ? new Date(batch.promotedAtUtc).toLocaleString() : "—"} />
            <FactBlock
              label="Rows promoted"
              value={rows.filter((row) => row.promotedMemberId).length.toString()}
            />
          </div>

          {errors.length > 0 ? (
            <div className="flex flex-col gap-2 rounded-lg border border-amber-200 bg-amber-50 p-3.5 dark:border-amber-500/30 dark:bg-amber-500/10">
              <p className="text-sm font-medium text-amber-900 dark:text-amber-400">
                {errors.length} validation issue{errors.length === 1 ? "" : "s"} found
              </p>
              <ul className="flex flex-col gap-1 text-[12.5px] text-amber-800 dark:text-amber-400/90">
                {errors.map((error) => {
                  const row = rows.find((r) => r.id === error.memberImportStagingId);
                  return (
                    <li key={error.id}>
                      {row ? `Row ${row.rowNumber}` : "Batch"}
                      {error.fieldName ? ` · ${error.fieldName}` : ""} — {error.message}
                    </li>
                  );
                })}
              </ul>
            </div>
          ) : null}

          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-14">Row</TableHead>
                <TableHead>Last name</TableHead>
                <TableHead>First name</TableHead>
                <TableHead>Employee no.</TableHead>
                <TableHead>Validation</TableHead>
                <TableHead>Match</TableHead>
                <TableHead />
              </TableRow>
            </TableHeader>
            <TableBody>
              {rows.map((row) => {
                const isEligible = row.validationStatus === "Valid" && row.matchStatus === "NoMatch" && !row.promotedMemberId;
                return (
                  <TableRow key={row.id}>
                    <TableCell className="tabular-nums">{row.rowNumber}</TableCell>
                    <TableCell>{row.lastName ?? "—"}</TableCell>
                    <TableCell>{row.firstName ?? "—"}</TableCell>
                    <TableCell className="tabular-nums">{row.employeeNumber ?? "—"}</TableCell>
                    <TableCell>
                      <Badge className={importRowValidationStatusBadgeClassName[row.validationStatus]}>
                        {row.validationStatus}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge className={importRowMatchStatusBadgeClassName[row.matchStatus]}>{row.matchStatus}</Badge>
                    </TableCell>
                    <TableCell>
                      {row.promotedMemberId ? (
                        <Link
                          href={`/dashboard/members/${row.promotedMemberId}`}
                          className={cn(buttonVariants({ variant: "outline", size: "sm" }))}
                        >
                          View member
                        </Link>
                      ) : (
                        <Button
                          type="button"
                          size="sm"
                          disabled={!isEligible || promotingRowId === row.id}
                          onClick={() => handlePromote(row)}
                        >
                          {promotingRowId === row.id ? "Promoting…" : "Promote"}
                        </Button>
                      )}
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
