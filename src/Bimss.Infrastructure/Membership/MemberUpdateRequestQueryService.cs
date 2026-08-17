using Bimss.Application.Membership;
using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.Infrastructure.Membership;

// Projects with LINQ joins (MemberUpdateRequest has no navigation property
// to Member, by design — same reasoning as MemberConfiguration's other
// FKs) so the list query runs as a single SQL statement, avoiding N+1.
public sealed class MemberUpdateRequestQueryService(BimssDbContext dbContext) : IMemberUpdateRequestQueryService
{
    public async Task<IReadOnlyList<MemberUpdateRequestSummary>> ListAsync(
        MemberUpdateRequestStatus? status, CancellationToken cancellationToken)
    {
        var requests = dbContext.MemberUpdateRequests.AsNoTracking();

        // Filter and order on the source entity, before projecting into
        // MemberUpdateRequestSummary — SQL Server's provider can't
        // translate a Where/OrderBy applied after a constructor-projection
        // like this record's (it throws InvalidOperationException at
        // query-translation time, only visible against a real SQL Server
        // provider; the EF Core InMemory provider used by this class's
        // own tests is more permissive and never caught it).
        if (status is not null)
        {
            requests = requests.Where(request => request.Status == status);
        }

        var query =
            from request in requests
            join member in dbContext.Members.AsNoTracking() on request.MemberId equals member.Id
            orderby request.SubmittedAtUtc descending
            select new MemberUpdateRequestSummary(
                request.Id,
                request.MemberId,
                member.LastName,
                member.FirstName,
                request.SubmittedByUserId,
                request.SubmittedAtUtc,
                request.Status);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MemberUpdateRequestSummary>> ListByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
    {
        var query =
            from request in dbContext.MemberUpdateRequests.AsNoTracking()
            join member in dbContext.Members.AsNoTracking() on request.MemberId equals member.Id
            where request.MemberId == memberId
            orderby request.SubmittedAtUtc descending
            select new MemberUpdateRequestSummary(
                request.Id,
                request.MemberId,
                member.LastName,
                member.FirstName,
                request.SubmittedByUserId,
                request.SubmittedAtUtc,
                request.Status);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<MemberUpdateRequestDetail?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var request =
            await (from r in dbContext.MemberUpdateRequests.AsNoTracking()
                   join member in dbContext.Members.AsNoTracking() on r.MemberId equals member.Id
                   where r.Id == requestId
                   select new
                   {
                       r.Id,
                       r.MemberId,
                       member.LastName,
                       member.FirstName,
                       r.SubmittedByUserId,
                       r.SubmittedAtUtc,
                       r.Status,
                       r.ReviewedByUserId,
                       r.ReviewedAtUtc,
                       r.ReviewRemarks,
                   })
                .SingleOrDefaultAsync(cancellationToken);

        if (request is null)
        {
            return null;
        }

        var changes = await dbContext.MemberUpdateRequestChanges
            .AsNoTracking()
            .Where(change => change.MemberUpdateRequestId == requestId)
            .Select(change => new MemberUpdateRequestChangeSummary(change.Id, change.FieldName, change.OldValue, change.NewValue))
            .ToListAsync(cancellationToken);

        return new MemberUpdateRequestDetail(
            request.Id,
            request.MemberId,
            request.LastName,
            request.FirstName,
            request.SubmittedByUserId,
            request.SubmittedAtUtc,
            request.Status,
            request.ReviewedByUserId,
            request.ReviewedAtUtc,
            request.ReviewRemarks,
            changes);
    }
}
