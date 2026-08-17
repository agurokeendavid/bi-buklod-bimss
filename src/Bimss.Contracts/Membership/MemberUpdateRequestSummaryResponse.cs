namespace Bimss.Contracts.Membership;

public class MemberUpdateRequestSummaryResponse
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public string MemberLastName { get; set; } = string.Empty;

    public string MemberFirstName { get; set; } = string.Empty;

    public Guid SubmittedByUserId { get; set; }

    public DateTimeOffset SubmittedAtUtc { get; set; }

    public string Status { get; set; } = string.Empty;
}
