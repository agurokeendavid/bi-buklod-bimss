namespace Bimss.Contracts.Membership;

public class MyProfileResponse
{
    public Guid Id { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    public string? SuffixName { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string PlaceOfBirth { get; set; } = string.Empty;

    public string CivilStatusName { get; set; } = string.Empty;

    public string? JoiningReason { get; set; }

    public string Status { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public string PositionDesignation { get; set; } = string.Empty;

    public string OfficeUnitName { get; set; } = string.Empty;

    public DateOnly? PermanentAppointmentDate { get; set; }
}
