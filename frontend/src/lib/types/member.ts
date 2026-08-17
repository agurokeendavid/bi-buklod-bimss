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
// a business identifier and not editable through this request. Reused
// as-is for BIMSS-042's self-service update-request submission — the
// proposed-values shape is identical, only the workflow differs (apply
// immediately vs. queue for officer review).
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

// Mirrors Bimss.Contracts.Membership.MyProfileResponse — self-service's own
// projection. Carries both the resolved display name (for the read-only
// "My Profile" view) and the raw id (for BIMSS-042's edit form, which needs
// to pre-select the right option in each Select the same way
// MemberDetail's ids do for the officer-facing edit form).
export interface MyProfile {
  id: string;
  lastName: string;
  firstName: string;
  middleName: string | null;
  suffixId: string | null;
  suffixName: string | null;
  dateOfBirth: string;
  placeOfBirth: string;
  civilStatusId: string;
  civilStatusName: string;
  joiningReason: string | null;
  status: MemberStatus;
  employeeNumber: string;
  positionDesignation: string;
  officeUnitId: string;
  officeUnitName: string;
  permanentAppointmentDate: string | null;
}

// Mirrors Bimss.Contracts.Membership.SubmitMemberUpdateRequestResponse.
export interface SubmitMemberUpdateRequestResult {
  id: string;
}

// Mirrors Bimss.Contracts.Membership.MyContactResponse.
export interface MyContact {
  landline: string | null;
  mobileNumber: string | null;
  email: string | null;
  presentAddress: string | null;
  permanentAddress: string | null;
}

// Mirrors Bimss.Contracts.Membership.UpdateMyContactRequest — the one
// profile area a member can edit directly without officer review, per
// docs/DATA_DICTIONARY.md's confirmed decision.
export interface UpdateMyContactRequest {
  landline: string | null;
  mobileNumber: string;
  email: string;
  presentAddress: string | null;
  permanentAddress: string | null;
}
