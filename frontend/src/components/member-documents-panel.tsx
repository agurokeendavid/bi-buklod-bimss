"use client";

import { useState, type FormEvent } from "react";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth-context";
import type { MemberDocument } from "@/lib/types/member";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

const DOCUMENT_TYPE_OPTIONS = ["Proof of Employment", "Valid ID", "Other"];

function formatFileSize(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} B`;
  }

  const kilobytes = bytes / 1024;
  if (kilobytes < 1024) {
    return `${kilobytes.toFixed(1)} KB`;
  }

  return `${(kilobytes / 1024).toFixed(1)} MB`;
}

export function MemberDocumentsPanel({
  memberId,
  documents,
  onUploaded,
}: {
  memberId: string;
  documents: MemberDocument[];
  onUploaded: (doc: MemberDocument) => void;
}) {
  const { fetchWithAuth } = useAuth();
  const [documentType, setDocumentType] = useState(DOCUMENT_TYPE_OPTIONS[0]);
  const [file, setFile] = useState<File | null>(null);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [downloadingId, setDownloadingId] = useState<string | null>(null);

  async function handleUpload(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!file) {
      setUploadError("Choose a file to upload.");
      return;
    }

    setUploadError(null);
    setIsUploading(true);

    try {
      const formData = new FormData();
      formData.append("file", file);
      formData.append("documentType", documentType);

      const response = await fetchWithAuth(`/api/members/${memberId}/documents`, {
        method: "POST",
        body: formData,
      });

      if (!response.ok) {
        const problem = await response.json().catch(() => null);
        setUploadError((problem?.detail as string | undefined) ?? `Failed to upload document (${response.status}).`);
        return;
      }

      const body = (await response.json()) as { id: string };
      onUploaded({
        id: body.id,
        documentType,
        originalFileName: file.name,
        contentType: file.type,
        fileSizeBytes: file.size,
        uploadedAtUtc: new Date().toISOString(),
        uploadedByUserId: null,
      });
      toast.success("Document uploaded.");
      setFile(null);
      const input = document.getElementById("documentFile") as HTMLInputElement | null;
      if (input) {
        input.value = "";
      }
    } finally {
      setIsUploading(false);
    }
  }

  async function handleDownload(doc: MemberDocument) {
    setDownloadingId(doc.id);
    try {
      const response = await fetchWithAuth(`/api/members/${memberId}/documents/${doc.id}/download`);
      if (!response.ok) {
        toast.error(`Failed to download document (${response.status}).`);
        return;
      }

      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = doc.originalFileName;
      link.click();
      URL.revokeObjectURL(url);
    } finally {
      setDownloadingId(null);
    }
  }

  return (
    <div className="flex flex-col gap-3">
      <p className="text-sm font-medium">Documents</p>

      {documents.length > 0 ? (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Type</TableHead>
              <TableHead>File name</TableHead>
              <TableHead>Size</TableHead>
              <TableHead>Uploaded</TableHead>
              <TableHead />
            </TableRow>
          </TableHeader>
          <TableBody>
            {documents.map((doc) => (
              <TableRow key={doc.id}>
                <TableCell>{doc.documentType}</TableCell>
                <TableCell>{doc.originalFileName}</TableCell>
                <TableCell>{formatFileSize(doc.fileSizeBytes)}</TableCell>
                <TableCell>{new Date(doc.uploadedAtUtc).toLocaleDateString()}</TableCell>
                <TableCell>
                  <Button
                    type="button"
                    size="sm"
                    variant="outline"
                    disabled={downloadingId === doc.id}
                    onClick={() => handleDownload(doc)}
                  >
                    {downloadingId === doc.id ? "Downloading…" : "Download"}
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      ) : (
        <p className="text-sm text-muted-foreground">No documents uploaded yet.</p>
      )}

      <form className="flex flex-wrap items-end gap-3" onSubmit={handleUpload}>
        <div className="flex flex-col gap-2">
          <Label htmlFor="documentType">Document type</Label>
          <Select value={documentType} onValueChange={(value) => setDocumentType(value ?? DOCUMENT_TYPE_OPTIONS[0])}>
            <SelectTrigger id="documentType">
              <SelectValue>{(value) => value}</SelectValue>
            </SelectTrigger>
            <SelectContent>
              {DOCUMENT_TYPE_OPTIONS.map((option) => (
                <SelectItem key={option} value={option}>
                  {option}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="flex flex-col gap-2">
          <Label htmlFor="documentFile">File (PDF, JPG, PNG)</Label>
          <input
            id="documentFile"
            type="file"
            accept=".pdf,.jpg,.jpeg,.png"
            className="text-sm"
            onChange={(event) => setFile(event.target.files?.[0] ?? null)}
          />
        </div>
        <Button type="submit" size="sm" disabled={isUploading}>
          {isUploading ? "Uploading…" : "Upload"}
        </Button>
      </form>

      {uploadError ? (
        <p role="alert" className="text-sm text-destructive">
          {uploadError}
        </p>
      ) : null}
    </div>
  );
}
