using Bimss.Application.Membership;
using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.Infrastructure.Membership;

public sealed class MemberRepository(BimssDbContext dbContext) : IMemberRepository
{
    public Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken)
    {
        return dbContext.MemberEmployments.AnyAsync(employment => employment.EmployeeNumber == employeeNumber, cancellationToken);
    }

    public async Task AddAsync(Member member, MemberEmployment employment, CancellationToken cancellationToken)
    {
        dbContext.Members.Add(member);
        dbContext.MemberEmployments.Add(employment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Member?> GetTrackedByIdAsync(Guid memberId, CancellationToken cancellationToken)
    {
        return dbContext.Members
            .Include(member => member.StatusHistory)
            .SingleOrDefaultAsync(member => member.Id == memberId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
