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
