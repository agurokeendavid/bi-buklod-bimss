namespace Bimss.Domain.Membership;

public sealed class MemberEmployment
{
    public MemberEmployment(
        Guid id,
        Guid memberId,
        string employeeNumber,
        string positionDesignation,
        Guid officeUnitId,
        DateOnly? permanentAppointmentDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(positionDesignation);
        if (memberId == Guid.Empty)
        {
            throw new ArgumentException("Member is required.", nameof(memberId));
        }

        if (officeUnitId == Guid.Empty)
        {
            throw new ArgumentException("Office unit is required.", nameof(officeUnitId));
        }

        Id = id;
        MemberId = memberId;
        EmployeeNumber = employeeNumber;
        PositionDesignation = positionDesignation;
        OfficeUnitId = officeUnitId;
        PermanentAppointmentDate = permanentAppointmentDate;
    }

    public Guid Id { get; private set; }

    public Guid MemberId { get; private set; }

    public string EmployeeNumber { get; private set; } = string.Empty;

    public string PositionDesignation { get; private set; } = string.Empty;

    public Guid OfficeUnitId { get; private set; }

    public DateOnly? PermanentAppointmentDate { get; private set; }

    public void UpdateDetails(string positionDesignation, Guid officeUnitId, DateOnly? permanentAppointmentDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(positionDesignation);
        if (officeUnitId == Guid.Empty)
        {
            throw new ArgumentException("Office unit is required.", nameof(officeUnitId));
        }

        PositionDesignation = positionDesignation;
        OfficeUnitId = officeUnitId;
        PermanentAppointmentDate = permanentAppointmentDate;
    }
}
