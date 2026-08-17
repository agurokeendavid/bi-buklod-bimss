using Bimss.Application.Membership;
using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.Infrastructure.Membership;

public sealed class MemberUpdateRequestRepository(BimssDbContext dbContext) : IMemberUpdateRequestRepository
{
    public async Task AddAsync(MemberUpdateRequest request, CancellationToken cancellationToken)
    {
        dbContext.MemberUpdateRequests.Add(request);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<MemberUpdateRequest?> GetTrackedByIdAsync(Guid requestId, CancellationToken cancellationToken)
    {
        return dbContext.MemberUpdateRequests
            .Include(request => request.Changes)
            .SingleOrDefaultAsync(request => request.Id == requestId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
