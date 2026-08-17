using Bimss.Application.Membership;
using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;

namespace Bimss.Infrastructure.Membership;

public sealed class MemberUpdateRequestRepository(BimssDbContext dbContext) : IMemberUpdateRequestRepository
{
    public async Task AddAsync(MemberUpdateRequest request, CancellationToken cancellationToken)
    {
        dbContext.MemberUpdateRequests.Add(request);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
