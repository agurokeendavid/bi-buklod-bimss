using Bimss.Application.Membership;
using Bimss.Domain.Membership;
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

    // Requires employment to exist (inner join, not left) — every member
    // created through MemberCreationService (the only creation path today)
    // gets both rows atomically, per CreateMemberCommand's own mandatory
    // EmployeeNumber. Suffix stays a left join since it's genuinely
    // optional on Member itself.
    public Task<MyProfileDetail?> GetMyProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return (
            from user in dbContext.Users.AsNoTracking()
            where user.Id == userId && user.MemberId != null
            join member in dbContext.Members.AsNoTracking() on user.MemberId equals member.Id
            join employment in dbContext.MemberEmployments.AsNoTracking() on member.Id equals employment.MemberId
            join civilStatus in dbContext.CivilStatuses.AsNoTracking() on member.CivilStatusId equals civilStatus.Id
            join officeUnit in dbContext.OfficeUnits.AsNoTracking() on employment.OfficeUnitId equals officeUnit.Id
            join suffix in dbContext.Suffixes.AsNoTracking() on member.SuffixId equals suffix.Id into suffixGroup
            from suffix in suffixGroup.DefaultIfEmpty()
            select new MyProfileDetail(
                member.Id,
                member.LastName,
                member.FirstName,
                member.MiddleName,
                member.SuffixId,
                suffix != null ? suffix.Name : null,
                member.DateOfBirth,
                member.PlaceOfBirth,
                member.CivilStatusId,
                civilStatus.Name,
                member.JoiningReason,
                member.Status,
                employment.EmployeeNumber,
                employment.PositionDesignation,
                employment.OfficeUnitId,
                officeUnit.Name,
                employment.PermanentAppointmentDate))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<Guid?> GetMemberIdByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => user.MemberId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<MyContactDetail?> GetMyContactByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return (
            from user in dbContext.Users.AsNoTracking()
            where user.Id == userId && user.MemberId != null
            join member in dbContext.Members.AsNoTracking() on user.MemberId equals member.Id
            join contact in dbContext.MemberContacts.AsNoTracking() on member.Id equals contact.MemberId into contactGroup
            from contact in contactGroup.DefaultIfEmpty()
            join presentAddress in dbContext.MemberAddresses.AsNoTracking().Where(address => address.AddressType == MemberAddressType.Present)
                on member.Id equals presentAddress.MemberId into presentAddressGroup
            from presentAddress in presentAddressGroup.DefaultIfEmpty()
            join permanentAddress in dbContext.MemberAddresses.AsNoTracking().Where(address => address.AddressType == MemberAddressType.Permanent)
                on member.Id equals permanentAddress.MemberId into permanentAddressGroup
            from permanentAddress in permanentAddressGroup.DefaultIfEmpty()
            select new MyContactDetail(
                member.Id,
                contact != null ? contact.Landline : null,
                contact != null ? contact.MobileNumber : null,
                contact != null ? contact.Email : null,
                presentAddress != null ? presentAddress.AddressLine : null,
                permanentAddress != null ? permanentAddress.AddressLine : null))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
