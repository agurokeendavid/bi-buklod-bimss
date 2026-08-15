// Mirrors Bimss.Contracts.Membership.MemberSummaryResponse.
export interface MemberSummary {
  id: string;
  lastName: string;
  firstName: string;
  middleName: string | null;
  status: "PendingVerification" | "Active" | "Inactive";
  employeeNumber: string | null;
}
