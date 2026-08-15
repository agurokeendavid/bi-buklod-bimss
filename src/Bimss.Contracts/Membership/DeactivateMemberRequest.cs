using System.ComponentModel.DataAnnotations;

namespace Bimss.Contracts.Membership;

public class DeactivateMemberRequest
{
    [Required]
    public Guid ReasonId { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }
}
