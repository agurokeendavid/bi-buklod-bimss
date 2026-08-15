namespace Bimss.Contracts.Membership;

public class MemberDetailResponse
{
    public Guid Id { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    public Guid? SuffixId { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string PlaceOfBirth { get; set; } = string.Empty;

    public Guid CivilStatusId { get; set; }

    public string? JoiningReason { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? EmployeeNumber { get; set; }

    public string? PositionDesignation { get; set; }

    public Guid? OfficeUnitId { get; set; }

    public DateOnly? PermanentAppointmentDate { get; set; }
}
