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

    public Task<MemberEmployment?> GetTrackedEmploymentByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
    {
        return dbContext.MemberEmployments
            .SingleOrDefaultAsync(employment => employment.MemberId == memberId, cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid memberId, CancellationToken cancellationToken)
    {
        return dbContext.Members.AnyAsync(member => member.Id == memberId, cancellationToken);
    }

    public async Task AddDocumentAsync(MemberDocument document, CancellationToken cancellationToken)
    {
        dbContext.MemberDocuments.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> HasAnyDocumentAsync(Guid memberId, CancellationToken cancellationToken)
    {
        return dbContext.MemberDocuments.AnyAsync(document => document.MemberId == memberId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
