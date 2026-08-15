using Bimss.Domain.Membership;
using Bimss.Infrastructure.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberDocumentQueryServiceTests
{
    [Fact]
    public async Task ListByMemberIdAsync_ReturnsOnlyThatMembersDocuments_NewestFirst()
    {
        var databaseName = Guid.NewGuid().ToString();
        var memberId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid();

        await using (var writeContext = CreateDbContext(databaseName))
        {
            writeContext.MemberDocuments.Add(new MemberDocument(
                Guid.NewGuid(), memberId, "ProofOfEmployment", "coe.pdf", "application/pdf", "key-1", 100,
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), null));
            writeContext.MemberDocuments.Add(new MemberDocument(
                Guid.NewGuid(), memberId, "ValidId", "id.jpg", "image/jpeg", "key-2", 200,
                new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero), null));
            writeContext.MemberDocuments.Add(new MemberDocument(
                Guid.NewGuid(), otherMemberId, "ProofOfEmployment", "other.pdf", "application/pdf", "key-3", 300,
                new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero), null));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateDbContext(databaseName);
        var service = new MemberDocumentQueryService(readContext);

        var result = await service.ListByMemberIdAsync(memberId, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("ValidId", result[0].DocumentType);
        Assert.Equal("ProofOfEmployment", result[1].DocumentType);
    }

    [Fact]
    public async Task GetForDownloadAsync_ReturnsTheDocument_WhenItBelongsToTheMember()
    {
        var databaseName = Guid.NewGuid().ToString();
        var memberId = Guid.NewGuid();
        var documentId = Guid.NewGuid();

        await using (var writeContext = CreateDbContext(databaseName))
        {
            writeContext.MemberDocuments.Add(new MemberDocument(
                documentId, memberId, "ProofOfEmployment", "coe.pdf", "application/pdf", "storage-key", 100,
                DateTimeOffset.UtcNow, null));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateDbContext(databaseName);
        var service = new MemberDocumentQueryService(readContext);

        var result = await service.GetForDownloadAsync(memberId, documentId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("storage-key", result!.StorageKey);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("coe.pdf", result.OriginalFileName);
    }

    [Fact]
    public async Task GetForDownloadAsync_ReturnsNull_WhenTheDocumentBelongsToAnotherMember()
    {
        var databaseName = Guid.NewGuid().ToString();
        var documentId = Guid.NewGuid();

        await using (var writeContext = CreateDbContext(databaseName))
        {
            writeContext.MemberDocuments.Add(new MemberDocument(
                documentId, Guid.NewGuid(), "ProofOfEmployment", "coe.pdf", "application/pdf", "storage-key", 100,
                DateTimeOffset.UtcNow, null));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateDbContext(databaseName);
        var service = new MemberDocumentQueryService(readContext);

        var result = await service.GetForDownloadAsync(Guid.NewGuid(), documentId, CancellationToken.None);

        Assert.Null(result);
    }

    private static BimssDbContext CreateDbContext(string databaseName)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>().UseInMemoryDatabase(databaseName);
        return new BimssDbContext(optionsBuilder.Options);
    }
}
