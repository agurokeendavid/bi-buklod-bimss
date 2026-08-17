export type MemberUpdateRequestStatus = "Pending" | "Approved" | "Rejected";

// Mirrors Bimss.Contracts.Membership.MemberUpdateRequestSummaryResponse.
export interface MemberUpdateRequestSummary {
  id: string;
  memberId: string;
  memberLastName: string;
  memberFirstName: string;
  submittedByUserId: string;
  submittedAtUtc: string;
  status: MemberUpdateRequestStatus;
}

// Mirrors Bimss.Contracts.Membership.MemberUpdateRequestChangeResponse.
export interface MemberUpdateRequestChange {
  id: string;
  fieldName: string;
  oldValue: string | null;
  newValue: string | null;
}

// Mirrors Bimss.Contracts.Membership.MemberUpdateRequestDetailResponse.
export interface MemberUpdateRequestDetail extends MemberUpdateRequestSummary {
  reviewedByUserId: string | null;
  reviewedAtUtc: string | null;
  reviewRemarks: string | null;
  changes: MemberUpdateRequestChange[];
}
