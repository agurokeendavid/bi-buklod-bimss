using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;

namespace Bimss.Application.Membership;

// Officer-direct-edit (BIMSS-030): an authorized officer updates a member's
// permitted fields directly, no review/approval step — unlike the future
// member-initiated self-service workflow (docs/DOMAIN_WORKFLOWS.md #2,
// Phase 1E's BIMSS-042/044), where the officer review IS the approval step.
// Authorization (Permission.Membership.Manage) is enforced by the controller
// that calls this service, not here — see MemberCreationService for the same
// convention.
public sealed class MemberProfileUpdateService(IMemberRepository memberRepository, IAuditLogger auditLogger)
{
    public async Task UpdateAsync(
        Guid memberId, UpdateMemberCommand command, Guid? actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var member = await memberRepository.GetTrackedByIdAsync(memberId, cancellationToken)
            ?? throw new NotFoundException("Member", memberId);

        var employment = await memberRepository.GetTrackedEmploymentByMemberIdAsync(memberId, cancellationToken)
            ?? throw new NotFoundException("MemberEmployment", memberId);

        member.UpdateProfile(
            command.LastName,
            command.FirstName,
            command.MiddleName,
            command.SuffixId,
            command.DateOfBirth,
            command.PlaceOfBirth,
            command.CivilStatusId,
            command.JoiningReason);

        employment.UpdateDetails(command.PositionDesignation, command.OfficeUnitId, command.PermanentAppointmentDate);

        await memberRepository.SaveChangesAsync(cancellationToken);

        await auditLogger.LogAsync(
            new AuditEntry(actorUserId, "Member.UpdateProfile", "Member", memberId.ToString(), AuditResult.Success),
            cancellationToken);
    }
}
