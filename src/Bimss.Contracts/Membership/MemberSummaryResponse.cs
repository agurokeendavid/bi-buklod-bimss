namespace Bimss.Contracts.Membership;

public class MemberSummaryResponse
{
    public Guid Id { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? EmployeeNumber { get; set; }
}
