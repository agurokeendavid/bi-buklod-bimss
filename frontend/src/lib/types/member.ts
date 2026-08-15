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
