using System.ComponentModel.DataAnnotations;

namespace Bimss.Contracts.Membership;

// Used for both Approve (Remarks optional) and Reject (Remarks required —
// enforced server-side by MemberUpdateRequest.Reject's own guard, per
// docs/design/BIMSS-UI-SPEC.md's "Return and Deny require remarks" rule;
// not re-declared here with [Required] since Approve reuses the same type).
public class ReviewMemberUpdateRequestRequest
{
    [StringLength(2000)]
    public string? Remarks { get; set; }
}
