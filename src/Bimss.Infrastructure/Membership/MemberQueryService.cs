using Bimss.Application.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.Infrastructure.Membership;

// Projects directly with LINQ joins (Member/MemberEmployment have no
// navigation properties between them, by design — see MemberConfiguration)
// so both queries below run as a single SQL statement each, avoiding N+1.
public sealed class MemberQueryService(BimssDbContext dbContext) : IMemberQueryService
{
    public Task<MemberDetail?> GetByIdAsync(Guid memberId, CancellationToken cancellationToken)
    {
        return (
            from member in dbContext.Members.AsNoTracking()
            where member.Id == memberId
            join employment in dbContext.MemberEmployments.AsNoTracking()
                on member.Id equals employment.MemberId into employmentGroup
            from employment in employmentGroup.DefaultIfEmpty()
            select new MemberDetail(
                member.Id,
                member.LastName,
                member.FirstName,
                member.MiddleName,
                member.SuffixId,
                member.DateOfBirth,
                member.PlaceOfBirth,
                member.CivilStatusId,
                member.JoiningReason,
                member.Status,
                employment != null ? employment.EmployeeNumber : null,
                employment != null ? employment.PositionDesignation : null,
                employment != null ? (Guid?)employment.OfficeUnitId : null,
                employment != null ? employment.PermanentAppointmentDate : null))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MemberSummary>> ListAsync(CancellationToken cancellationToken)
    {
        return await (
            from member in dbContext.Members.AsNoTracking()
            join employment in dbContext.MemberEmployments.AsNoTracking()
                on member.Id equals employment.MemberId into employmentGroup
            from employment in employmentGroup.DefaultIfEmpty()
            select new MemberSummary(
                member.Id,
                member.LastName,
                member.FirstName,
                member.MiddleName,
                member.Status,
                employment != null ? employment.EmployeeNumber : null))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MemberStatusHistoryEntry>> ListStatusHistoryAsync(Guid memberId, CancellationToken cancellationToken)
    {
        return await dbContext.MemberStatusHistories
            .AsNoTracking()
            .Where(entry => entry.MemberId == memberId)
            .OrderBy(entry => entry.OccurredAtUtc)
            .Select(entry => new MemberStatusHistoryEntry(
                entry.Id, entry.FromStatus, entry.ToStatus, entry.ReasonId, entry.ActorUserId, entry.OccurredAtUtc, entry.Remarks))
            .ToListAsync(cancellationToken);
    }
}
