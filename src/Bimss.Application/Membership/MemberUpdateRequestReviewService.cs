using System.Globalization;
using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Implements docs/DOMAIN_WORKFLOWS.md's "Member profile update" workflow's
// review step: "Membership Officer reviews differences -> Approve / Reject
// -> Approved changes applied -> History/audit recorded." Reuses
// MemberProfileUpdateService (BIMSS-030's officer-direct-edit path) to
// actually apply an approved change — the request's per-field diffs are
// replayed onto the member's current values to reconstruct the same
// UpdateMemberCommand shape that flow already validates and persists,
// rather than re-implementing field-by-field mutation here.
public sealed class MemberUpdateRequestReviewService(
    IMemberUpdateRequestRepository memberUpdateRequestRepository,
    IMemberRepository memberRepository,
    MemberProfileUpdateService memberProfileUpdateService,
    IAuditLogger auditLogger,
    TimeProvider timeProvider)
{
    public async Task ApproveAsync(
        Guid requestId, Guid actorUserId, string? remarks, CancellationToken cancellationToken = default)
    {
        var request = await memberUpdateRequestRepository.GetTrackedByIdAsync(requestId, cancellationToken)
            ?? throw new NotFoundException("MemberUpdateRequest", requestId);

        var member = await memberRepository.GetTrackedByIdAsync(request.MemberId, cancellationToken)
            ?? throw new NotFoundException("Member", request.MemberId);
        var employment = await memberRepository.GetTrackedEmploymentByMemberIdAsync(request.MemberId, cancellationToken)
            ?? throw new NotFoundException("MemberEmployment", request.MemberId);

        var command = ApplyChanges(member, employment, request.Changes);

        var occurredAtUtc = timeProvider.GetUtcNow();
        request.Approve(actorUserId, occurredAtUtc, remarks);
        await memberUpdateRequestRepository.SaveChangesAsync(cancellationToken);

        // Applies and persists the actual Member/MemberEmployment change,
        // including its own "Member.UpdateProfile" audit entry — separate
        // from the "MemberUpdateRequest.Approve" entry logged below, since
        // they are two distinct auditable facts (the review decision, and
        // the resulting profile change).
        await memberProfileUpdateService.UpdateAsync(request.MemberId, command, actorUserId, cancellationToken);

        await auditLogger.LogAsync(
            new AuditEntry(actorUserId, "MemberUpdateRequest.Approve", "MemberUpdateRequest", requestId.ToString(), AuditResult.Success, remarks),
            cancellationToken);
    }

    public async Task RejectAsync(
        Guid requestId, Guid actorUserId, string remarks, CancellationToken cancellationToken = default)
    {
        var request = await memberUpdateRequestRepository.GetTrackedByIdAsync(requestId, cancellationToken)
            ?? throw new NotFoundException("MemberUpdateRequest", requestId);

        request.Reject(actorUserId, timeProvider.GetUtcNow(), remarks);
        await memberUpdateRequestRepository.SaveChangesAsync(cancellationToken);

        await auditLogger.LogAsync(
            new AuditEntry(actorUserId, "MemberUpdateRequest.Reject", "MemberUpdateRequest", requestId.ToString(), AuditResult.Success, remarks),
            cancellationToken);
    }

    // Mirrors MemberUpdateRequestSubmissionService.BuildChanges' encoding
    // exactly (FieldName values, "O" round-trip date format) — decode must
    // match encode.
    private static UpdateMemberCommand ApplyChanges(
        Member member, MemberEmployment employment, IReadOnlyCollection<MemberUpdateRequestChange> changes)
    {
        var lastName = member.LastName;
        var firstName = member.FirstName;
        var middleName = member.MiddleName;
        var suffixId = member.SuffixId;
        var dateOfBirth = member.DateOfBirth;
        var placeOfBirth = member.PlaceOfBirth;
        var civilStatusId = member.CivilStatusId;
        var joiningReason = member.JoiningReason;
        var positionDesignation = employment.PositionDesignation;
        var officeUnitId = employment.OfficeUnitId;
        var permanentAppointmentDate = employment.PermanentAppointmentDate;

        foreach (var change in changes)
        {
            switch (change.FieldName)
            {
                case nameof(Member.LastName):
                    lastName = change.NewValue ?? lastName;
                    break;
                case nameof(Member.FirstName):
                    firstName = change.NewValue ?? firstName;
                    break;
                case nameof(Member.MiddleName):
                    middleName = change.NewValue;
                    break;
                case nameof(Member.SuffixId):
                    suffixId = change.NewValue is null ? null : Guid.Parse(change.NewValue);
                    break;
                case nameof(Member.DateOfBirth):
                    dateOfBirth = ParseDate(change.NewValue) ?? dateOfBirth;
                    break;
                case nameof(Member.PlaceOfBirth):
                    placeOfBirth = change.NewValue ?? placeOfBirth;
                    break;
                case nameof(Member.CivilStatusId):
                    civilStatusId = change.NewValue is null ? civilStatusId : Guid.Parse(change.NewValue);
                    break;
                case nameof(Member.JoiningReason):
                    joiningReason = change.NewValue;
                    break;
                case nameof(MemberEmployment.PositionDesignation):
                    positionDesignation = change.NewValue ?? positionDesignation;
                    break;
                case nameof(MemberEmployment.OfficeUnitId):
                    officeUnitId = change.NewValue is null ? officeUnitId : Guid.Parse(change.NewValue);
                    break;
                case nameof(MemberEmployment.PermanentAppointmentDate):
                    permanentAppointmentDate = ParseDate(change.NewValue);
                    break;
            }
        }

        return new UpdateMemberCommand(
            lastName,
            firstName,
            middleName,
            suffixId,
            dateOfBirth,
            placeOfBirth,
            civilStatusId,
            joiningReason,
            positionDesignation,
            officeUnitId,
            permanentAppointmentDate);
    }

    private static DateOnly? ParseDate(string? raw)
    {
        return raw is null ? null : DateOnly.ParseExact(raw, "O", CultureInfo.InvariantCulture);
    }
}
