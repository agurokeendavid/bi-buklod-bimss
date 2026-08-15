namespace Bimss.Domain.Membership;

public sealed class MemberEducation
{
    public MemberEducation(Guid id, Guid memberId, Guid highestAttainmentId, string? degreeCourse)
    {
        if (memberId == Guid.Empty)
        {
            throw new ArgumentException("Member is required.", nameof(memberId));
        }

        if (highestAttainmentId == Guid.Empty)
        {
            throw new ArgumentException("Highest educational attainment is required.", nameof(highestAttainmentId));
        }

        Id = id;
        MemberId = memberId;
        HighestAttainmentId = highestAttainmentId;
        DegreeCourse = degreeCourse;
    }

    public Guid Id { get; private set; }

    public Guid MemberId { get; private set; }

    public Guid HighestAttainmentId { get; private set; }

    public string? DegreeCourse { get; private set; }

    public void UpdateDetails(Guid highestAttainmentId, string? degreeCourse)
    {
        if (highestAttainmentId == Guid.Empty)
        {
            throw new ArgumentException("Highest educational attainment is required.", nameof(highestAttainmentId));
        }

        HighestAttainmentId = highestAttainmentId;
        DegreeCourse = degreeCourse;
    }
}
