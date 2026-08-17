using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Implements docs/DOMAIN_WORKFLOWS.md's "Member profile update" workflow's
// submission step. Reuses UpdateMemberCommand — the same shape the
// officer-direct-edit flow (BIMSS-030) already applies immediately — as
// the "proposed values" input, since the set of editable fields is
// identical; only the workflow differs (apply immediately vs. queue for
// review). EmployeeNumber is not included because UpdateMemberCommand
// itself never included it (BIMSS-016: not mutable through
// MemberEmployment). Contact information (phone/email/mailing address) is
// deliberately excluded too — that's BIMSS-044's direct-edit path per
// docs/DATA_DICTIONARY.md's confirmed decision, not this approval
// workflow.
public sealed class MemberUpdateRequestSubmissionService(
    IMemberRepository memberRepository,
    IMemberUpdateRequestRepository memberUpdateRequestRepository,
    IAuditLogger auditLogger,
    TimeProvider timeProvider)
{
    public async Task<Guid> SubmitAsync(
        Guid memberId, Guid submittedByUserId, UpdateMemberCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var member = await memberRepository.GetTrackedByIdAsync(memberId, cancellationToken)
            ?? throw new NotFoundException("Member", memberId);
        var employment = await memberRepository.GetTrackedEmploymentByMemberIdAsync(memberId, cancellationToken)
            ?? throw new NotFoundException("MemberEmployment", memberId);

        var changes = BuildChanges(member, employment, command);
        if (changes.Count == 0)
        {
            throw new DomainValidationException(
                "No changes were proposed.",
                new Dictionary<string, string[]> { ["Changes"] = ["At least one field must differ from your current record."] });
        }

        var occurredAtUtc = timeProvider.GetUtcNow();
        var request = new MemberUpdateRequest(Guid.NewGuid(), memberId, submittedByUserId, occurredAtUtc, changes);

        await memberUpdateRequestRepository.AddAsync(request, cancellationToken);

        await auditLogger.LogAsync(
            new AuditEntry(submittedByUserId, "MemberUpdateRequest.Submit", "MemberUpdateRequest", request.Id.ToString(), AuditResult.Success),
            cancellationToken);

        return request.Id;
    }

    private static List<MemberUpdateRequestChangeInput> BuildChanges(Member member, MemberEmployment employment, UpdateMemberCommand command)
    {
        var changes = new List<MemberUpdateRequestChangeInput>();

        AddIfChanged(changes, nameof(Member.LastName), member.LastName, command.LastName);
        AddIfChanged(changes, nameof(Member.FirstName), member.FirstName, command.FirstName);
        AddIfChanged(changes, nameof(Member.MiddleName), member.MiddleName, command.MiddleName);
        AddIfChanged(changes, nameof(Member.SuffixId), member.SuffixId?.ToString(), command.SuffixId?.ToString());
        AddIfChanged(changes, nameof(Member.DateOfBirth), member.DateOfBirth.ToString("O"), command.DateOfBirth.ToString("O"));
        AddIfChanged(changes, nameof(Member.PlaceOfBirth), member.PlaceOfBirth, command.PlaceOfBirth);
        AddIfChanged(changes, nameof(Member.CivilStatusId), member.CivilStatusId.ToString(), command.CivilStatusId.ToString());
        AddIfChanged(changes, nameof(Member.JoiningReason), member.JoiningReason, command.JoiningReason);
        AddIfChanged(changes, nameof(MemberEmployment.PositionDesignation), employment.PositionDesignation, command.PositionDesignation);
        AddIfChanged(changes, nameof(MemberEmployment.OfficeUnitId), employment.OfficeUnitId.ToString(), command.OfficeUnitId.ToString());
        AddIfChanged(
            changes,
            nameof(MemberEmployment.PermanentAppointmentDate),
            employment.PermanentAppointmentDate?.ToString("O"),
            command.PermanentAppointmentDate?.ToString("O"));

        return changes;
    }

    private static void AddIfChanged(List<MemberUpdateRequestChangeInput> changes, string fieldName, string? oldValue, string? newValue)
    {
        if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            changes.Add(new MemberUpdateRequestChangeInput(fieldName, oldValue, newValue));
        }
    }
}
