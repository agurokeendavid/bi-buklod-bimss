namespace Bimss.Domain.Membership;

// Immutable audit-style record — a new row is appended for every consent
// event (re-consenting to a later notice version, or withdrawing consent);
// existing rows are never edited, per AGENTS.md's rule against overwriting
// auditable records. There is no update method by design.
public sealed class MemberPrivacyConsent
{
    public MemberPrivacyConsent(
        Guid id, Guid memberId, bool consentGiven, string noticeVersion, DateTimeOffset consentedAtUtc, string source)
    {
        if (memberId == Guid.Empty)
        {
            throw new ArgumentException("Member is required.", nameof(memberId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(noticeVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        Id = id;
        MemberId = memberId;
        ConsentGiven = consentGiven;
        NoticeVersion = noticeVersion;
        ConsentedAtUtc = consentedAtUtc;
        Source = source;
    }

    public Guid Id { get; private set; }

    public Guid MemberId { get; private set; }

    public bool ConsentGiven { get; private set; }

    public string NoticeVersion { get; private set; } = string.Empty;

    public DateTimeOffset ConsentedAtUtc { get; private set; }

    public string Source { get; private set; } = string.Empty;
}
