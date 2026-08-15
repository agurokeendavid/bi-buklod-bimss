using System.ComponentModel.DataAnnotations;

namespace Bimss.Contracts.Membership;

public class CreateMemberRequest
{
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? MiddleName { get; set; }

    public Guid? SuffixId { get; set; }

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    [StringLength(200)]
    public string PlaceOfBirth { get; set; } = string.Empty;

    [Required]
    public Guid CivilStatusId { get; set; }

    [StringLength(2000)]
    public string? JoiningReason { get; set; }

    [Required]
    [StringLength(50)]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string PositionDesignation { get; set; } = string.Empty;

    [Required]
    public Guid OfficeUnitId { get; set; }

    public DateOnly? PermanentAppointmentDate { get; set; }
}
