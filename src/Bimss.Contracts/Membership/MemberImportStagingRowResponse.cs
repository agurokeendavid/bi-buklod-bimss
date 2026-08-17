namespace Bimss.Contracts.Membership;

public class MemberImportStagingRowResponse
{
    public Guid Id { get; set; }

    public int RowNumber { get; set; }

    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    public string? EmployeeNumber { get; set; }

    public string ValidationStatus { get; set; } = string.Empty;

    public string MatchStatus { get; set; } = string.Empty;

    public Guid? MatchedMemberId { get; set; }

    public Guid? PromotedMemberId { get; set; }
}
