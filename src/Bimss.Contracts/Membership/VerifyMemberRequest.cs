using System.ComponentModel.DataAnnotations;

namespace Bimss.Contracts.Membership;

public class VerifyMemberRequest
{
    [StringLength(1000)]
    public string? Remarks { get; set; }
}
