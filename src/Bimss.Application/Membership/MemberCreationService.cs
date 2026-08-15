using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

public sealed class MemberCreationService(
    IMemberRepository memberRepository, IAuditLogger auditLogger, TimeProvider timeProvider)
{
    public async Task<CreateMemberResult> CreateAsync(
        CreateMemberCommand command, Guid? actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Pre-flight check for a friendly ConflictException; the unique
        // index on MemberEmployment.EmployeeNumber remains the authoritative,
        // concurrency-safe guard against duplicates (AGENTS.md: "Use database
        // constraints ... for invariants that must survive concurrent
        // requests").
        if (await memberRepository.EmployeeNumberExistsAsync(command.EmployeeNumber, cancellationToken))
        {
            throw new ConflictException($"Employee number '{command.EmployeeNumber}' is already registered.");
        }

        var occurredAtUtc = timeProvider.GetUtcNow();
        var memberId = Guid.NewGuid();

        var member = new Member(
            memberId,
            command.LastName,
            command.FirstName,
            command.MiddleName,
            command.SuffixId,
            command.DateOfBirth,
            command.PlaceOfBirth,
            command.CivilStatusId,
            command.JoiningReason,
            occurredAtUtc);

        var employment = new MemberEmployment(
            Guid.NewGuid(),
            memberId,
            command.EmployeeNumber,
            command.PositionDesignation,
            command.OfficeUnitId,
            command.PermanentAppointmentDate);

        await memberRepository.AddAsync(member, employment, cancellationToken);

        await auditLogger.LogAsync(
            new AuditEntry(actorUserId, "Member.Create", "Member", memberId.ToString(), AuditResult.Success),
            cancellationToken);

        return new CreateMemberResult(memberId);
    }
}
