using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberDocumentPersistenceTests
{
    private static readonly DateTimeOffset UploadedAt = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task MemberDocument_RoundTrips_ThroughPersistence()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var uploadedByUserId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var document = new MemberDocument(
                id, memberId, "ProofOfEmployment", "coe.pdf", "application/pdf", "a1b2c3", 102_400, UploadedAt, uploadedByUserId);
            writeContext.MemberDocuments.Add(document);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberDocuments.SingleAsync();

        Assert.Equal(id, persisted.Id);
        Assert.Equal(memberId, persisted.MemberId);
        Assert.Equal("ProofOfEmployment", persisted.DocumentType);
        Assert.Equal("coe.pdf", persisted.OriginalFileName);
        Assert.Equal("application/pdf", persisted.ContentType);
        Assert.Equal("a1b2c3", persisted.StorageKey);
        Assert.Equal(102_400, persisted.FileSizeBytes);
        Assert.Equal(UploadedAt, persisted.UploadedAtUtc);
        Assert.Equal(uploadedByUserId, persisted.UploadedByUserId);
    }

    [Fact]
    public async Task MemberDocument_RoundTrips_MultipleRows_ForTheSameMember()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.MemberDocuments.Add(new MemberDocument(
                Guid.NewGuid(), memberId, "ProofOfEmployment", "coe.pdf", "application/pdf", "a1b2c3", 102_400, UploadedAt, null));
            writeContext.MemberDocuments.Add(new MemberDocument(
                Guid.NewGuid(), memberId, "ValidId", "id.jpg", "image/jpeg", "d4e5f6", 51_200, UploadedAt, null));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var documents = await readContext.MemberDocuments.Where(d => d.MemberId == memberId).ToListAsync();

        Assert.Equal(2, documents.Count);
        Assert.Contains(documents, d => d.DocumentType == "ProofOfEmployment" && d.ContentType == "application/pdf");
        Assert.Contains(documents, d => d.DocumentType == "ValidId" && d.ContentType == "image/jpeg");
    }
}
