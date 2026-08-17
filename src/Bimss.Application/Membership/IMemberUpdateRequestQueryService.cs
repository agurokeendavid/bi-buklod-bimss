using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

public interface IMemberUpdateRequestQueryService
{
    Task<IReadOnlyList<MemberUpdateRequestSummary>> ListAsync(MemberUpdateRequestStatus? status, CancellationToken cancellationToken);

    Task<MemberUpdateRequestDetail?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken);
}
