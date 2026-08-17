using System.ComponentModel.DataAnnotations;

namespace Bimss.Contracts.Membership;

public class UpdateMyContactRequest
{
    [StringLength(30)]
    public string? Landline { get; set; }

    [Required]
    [StringLength(30)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(500)]
    public string? PresentAddress { get; set; }

    [StringLength(500)]
    public string? PermanentAddress { get; set; }
}
