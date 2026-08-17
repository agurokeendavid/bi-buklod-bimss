export type ImportBatchStatus = "Created" | "Staged" | "Validated" | "Promoted" | "Cancelled";

export type ImportRowValidationStatus = "NotValidated" | "Valid" | "Invalid";

export type ImportRowMatchStatus = "NotEvaluated" | "NoMatch" | "PossibleDuplicate" | "ConfirmedDuplicate";

export type ImportValidationSeverity = "Warning" | "Error";

// Mirrors Bimss.Contracts.Membership.ImportBatchSummaryResponse.
export interface ImportBatchSummary {
  id: string;
  fileName: string;
  status: ImportBatchStatus;
  rowCount: number | null;
  uploadedAtUtc: string;
  uploadedByUserId: string;
}

// Mirrors Bimss.Contracts.Membership.ImportBatchDetailResponse.
export interface ImportBatchDetail extends ImportBatchSummary {
  stagedAtUtc: string | null;
  validatedAtUtc: string | null;
  promotedAtUtc: string | null;
  cancelledAtUtc: string | null;
  remarks: string | null;
}

// Mirrors Bimss.Contracts.Membership.MemberImportStagingRowResponse.
export interface MemberImportStagingRow {
  id: string;
  rowNumber: number;
  lastName: string | null;
  firstName: string | null;
  employeeNumber: string | null;
  validationStatus: ImportRowValidationStatus;
  matchStatus: ImportRowMatchStatus;
  matchedMemberId: string | null;
  promotedMemberId: string | null;
}

// Mirrors Bimss.Contracts.Membership.ImportValidationErrorResponse.
export interface ImportValidationErrorEntry {
  id: string;
  memberImportStagingId: string | null;
  fieldName: string | null;
  severity: ImportValidationSeverity;
  message: string;
}

// Mirrors Bimss.Contracts.Membership.ImportBatchIngestResponse.
export interface ImportBatchIngestResult {
  id: string;
  rowCount: number;
}
