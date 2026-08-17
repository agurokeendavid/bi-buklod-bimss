"use client";

import { useEffect, useId, useRef, useState, type FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { OctagonAlert, Upload } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import type { ImportBatchIngestResult, ImportBatchSummary } from "@/lib/types/import-batch";
import { importBatchStatusBadgeClassName } from "@/lib/import-batch-status";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { cn } from "@/lib/utils";

export default function ImportBatchesPage() {
  const { fetchWithAuth } = useAuth();
  const router = useRouter();
  const fileInputId = useId();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [batches, setBatches] = useState<ImportBatchSummary[] | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [file, setFile] = useState<File | null>(null);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadBatches() {
      const response = await fetchWithAuth("/api/import-batches");
      if (cancelled) {
        return;
      }

      if (!response.ok) {
        setLoadError(`Failed to load import batches (${response.status}).`);
        return;
      }

      setBatches((await response.json()) as ImportBatchSummary[]);
    }

    void loadBatches();

    return () => {
      cancelled = true;
    };
  }, [fetchWithAuth]);

  async function handleUpload(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!file) {
      setUploadError("Choose an Excel file to upload.");
      return;
    }

    setUploadError(null);
    setIsUploading(true);

    try {
      const formData = new FormData();
      formData.append("file", file);

      const response = await fetchWithAuth("/api/import-batches", { method: "POST", body: formData });

      if (!response.ok) {
        const problem = await response.json().catch(() => null);
        setUploadError((problem?.detail as string | undefined) ?? `Failed to upload the file (${response.status}).`);
        return;
      }

      const body = (await response.json()) as ImportBatchIngestResult;
      toast.success(`Loaded ${body.rowCount} row${body.rowCount === 1 ? "" : "s"} to staging.`);
      router.push(`/dashboard/import-batches/${body.id}`);
    } finally {
      setIsUploading(false);
    }
  }

  return (
    <Card className="rounded-xl shadow-none">
      <CardHeader>
        <CardTitle className="text-[14.5px] font-semibold">Member imports</CardTitle>
        <CardDescription>
          Load legacy member records from an Excel export into staging for review before they become member
          records. Existing-member matching and promotion happen per row on the batch&apos;s own page.
        </CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-6">
        <form className="flex flex-wrap items-end gap-3 border-b pb-6" onSubmit={handleUpload}>
          <div className="flex flex-col gap-2">
            <label htmlFor={fileInputId} className="text-sm font-medium">
              Excel file (.xlsx)
            </label>
            <div className="flex items-center gap-3">
              <label htmlFor={fileInputId} className={cn(buttonVariants({ variant: "outline", size: "default" }), "cursor-pointer")}>
                <Upload className="size-4" />
                Choose file
              </label>
              <input
                ref={fileInputRef}
                id={fileInputId}
                type="file"
                accept=".xlsx"
                className="sr-only"
                onChange={(event) => setFile(event.target.files?.[0] ?? null)}
              />
              <span className="text-sm text-muted-foreground">{file ? file.name : "No file chosen"}</span>
            </div>
          </div>
          <Button type="submit" size="sm" disabled={isUploading}>
            {isUploading ? "Uploading…" : "Upload and stage"}
          </Button>
        </form>

        {uploadError ? (
          <p role="alert" className="text-sm text-destructive">
            {uploadError}
          </p>
        ) : null}

        {loadError ? (
          <Alert variant="destructive">
            <OctagonAlert />
            <AlertDescription>{loadError}</AlertDescription>
          </Alert>
        ) : batches ? (
          batches.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>File name</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Rows</TableHead>
                  <TableHead>Uploaded</TableHead>
                  <TableHead />
                </TableRow>
              </TableHeader>
              <TableBody>
                {batches.map((batch) => (
                  <TableRow key={batch.id}>
                    <TableCell className="font-medium">{batch.fileName}</TableCell>
                    <TableCell>
                      <Badge className={importBatchStatusBadgeClassName[batch.status]}>{batch.status}</Badge>
                    </TableCell>
                    <TableCell className="text-right tabular-nums">{batch.rowCount ?? "—"}</TableCell>
                    <TableCell className="text-muted-foreground">{new Date(batch.uploadedAtUtc).toLocaleString()}</TableCell>
                    <TableCell>
                      <Link href={`/dashboard/import-batches/${batch.id}`} className={cn(buttonVariants({ variant: "outline", size: "sm" }))}>
                        Review
                      </Link>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <p className="text-sm text-muted-foreground">No import batches yet. Upload an Excel file to get started.</p>
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
