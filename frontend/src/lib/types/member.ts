export type MemberStatus = "PendingVerification" | "Active" | "Inactive";

// Mirrors Bimss.Contracts.Membership.MemberSummaryResponse.
export interface MemberSummary {
  id: string;
  lastName: string;
  firstName: string;
  middleName: string | null;
  status: MemberStatus;
  employeeNumber: string | null;
}

// Mirrors Bimss.Contracts.Membership.MemberDetailResponse. DateOnly fields
// serialize as "yyyy-MM-dd" strings.
export interface MemberDetail {
  id: string;
  lastName: string;
  firstName: string;
  middleName: string | null;
  suffixId: string | null;
  dateOfBirth: string;
  placeOfBirth: string;
  civilStatusId: string;
  joiningReason: string | null;
  status: MemberStatus;
  employeeNumber: string | null;
  positionDesignation: string | null;
  officeUnitId: string | null;
  permanentAppointmentDate: string | null;
}

// Mirrors Bimss.Contracts.Membership.ReferenceDataItemResponse.
export interface ReferenceDataItem {
  id: string;
  code: string;
  name: string;
}

// Mirrors Bimss.Contracts.Membership.CreateMemberRequest. DateOnly fields
// are sent as "yyyy-MM-dd" strings.
export interface CreateMemberRequest {
  lastName: string;
  firstName: string;
  middleName: string | null;
  suffixId: string | null;
  dateOfBirth: string;
  placeOfBirth: string;
  civilStatusId: string;
  joiningReason: string | null;
  employeeNumber: string;
  positionDesignation: string;
  officeUnitId: string;
  permanentAppointmentDate: string | null;
}

// Mirrors Bimss.Contracts.Membership.UpdateMemberRequest. EmployeeNumber is
// a business identifier and not editable through this request.
export interface UpdateMemberRequest {
  lastName: string;
  firstName: string;
  middleName: string | null;
  suffixId: string | null;
  dateOfBirth: string;
  placeOfBirth: string;
  civilStatusId: string;
  joiningReason: string | null;
  positionDesignation: string;
  officeUnitId: string;
  permanentAppointmentDate: string | null;
}

// Mirrors Bimss.Contracts.Membership.MemberDocumentSummaryResponse.
export interface MemberDocument {
  id: string;
  documentType: string;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedAtUtc: string;
  uploadedByUserId: string | null;
}

// Mirrors Bimss.Contracts.Membership.MyProfileResponse — self-service's own
// projection, with reference values already resolved to display names
// server-side (unlike MemberDetail, which keeps raw ids for the
// officer-facing edit form's Select components).
export interface MyProfile {
  id: string;
  lastName: string;
  firstName: string;
  middleName: string | null;
  suffixName: string | null;
  dateOfBirth: string;
  placeOfBirth: string;
  civilStatusName: string;
  joiningReason: string | null;
  status: MemberStatus;
  employeeNumber: string;
  positionDesignation: string;
  officeUnitName: string;
  permanentAppointmentDate: string | null;
}

// Mirrors Bimss.Contracts.Membership.MemberStatusHistoryResponse.
export interface MemberStatusHistoryEntry {
  id: string;
  fromStatus: MemberStatus | null;
  toStatus: MemberStatus;
  reasonId: string | null;
  actorUserId: string | null;
  occurredAtUtc: string;
  remarks: string | null;
}
