using Bimss.Application.Auditing;
using Bimss.Application.Membership;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberDocumentUploadServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task UploadAsync_Succeeds_AndLogsAudit()
    {
        var memberId = Guid.NewGuid();
        var repository = new FakeMemberRepository { MemberExists = true };
        var storage = new FakeMemberDocumentStorage();
        var auditLogger = new FakeAuditLogger();
        var service = new MemberDocumentUploadService(repository, storage, auditLogger, new FixedTimeProvider(Now));
        var actorUserId = Guid.NewGuid();

        await using var content = new MemoryStream([1, 2, 3]);
        var documentId = await service.UploadAsync(
            memberId, "ProofOfEmployment", "coe.pdf", "application/pdf", content, content.Length, actorUserId);

        Assert.NotEqual(Guid.Empty, documentId);
        Assert.NotNull(repository.AddedDocument);
        Assert.Equal(memberId, repository.AddedDocument!.MemberId);
        Assert.Equal("ProofOfEmployment", repository.AddedDocument.DocumentType);
        Assert.Equal(storage.LastSavedKey, repository.AddedDocument.StorageKey);
        Assert.NotNull(auditLogger.LoggedEntry);
        Assert.Equal("Member.UploadDocument", auditLogger.LoggedEntry!.Action);
        Assert.Equal(actorUserId, auditLogger.LoggedEntry.ActorUserId);
    }

    [Fact]
    public async Task UploadAsync_Throws_NotFound_WhenMemberDoesNotExist()
    {
        var repository = new FakeMemberRepository { MemberExists = false };
        var storage = new FakeMemberDocumentStorage();
        var auditLogger = new FakeAuditLogger();
        var service = new MemberDocumentUploadService(repository, storage, auditLogger, new FixedTimeProvider(Now));

        await using var content = new MemoryStream([1, 2, 3]);

        await Assert.ThrowsAsync<NotFoundException>(() => service.UploadAsync(
            Guid.NewGuid(), "ProofOfEmployment", "coe.pdf", "application/pdf", content, content.Length, Guid.NewGuid()));

        Assert.Null(repository.AddedDocument);
        Assert.Null(auditLogger.LoggedEntry);
        Assert.False(storage.SaveAsyncCalled);
    }

    private sealed class FakeMemberRepository : IMemberRepository
    {
        public bool MemberExists { get; set; }

        public MemberDocument? AddedDocument { get; private set; }

        public Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberDocumentUploadService.");

        public Task AddAsync(Member member, MemberEmployment employment, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberDocumentUploadService.");

        public Task<Member?> GetTrackedByIdAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberDocumentUploadService.");

        public Task<MemberEmployment?> GetTrackedEmploymentByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberDocumentUploadService.");

        public Task<bool> ExistsAsync(Guid memberId, CancellationToken cancellationToken)
            => Task.FromResult(MemberExists);

        public Task AddDocumentAsync(MemberDocument document, CancellationToken cancellationToken)
        {
            AddedDocument = document;
            return Task.CompletedTask;
        }

        public Task<bool> HasAnyDocumentAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberDocumentUploadService.");

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberDocumentUploadService.");
    }

    private sealed class FakeMemberDocumentStorage : IMemberDocumentStorage
    {
        public bool SaveAsyncCalled { get; private set; }

        public string? LastSavedKey { get; private set; }

        public Task<string> SaveAsync(Stream content, CancellationToken cancellationToken)
        {
            SaveAsyncCalled = true;
            LastSavedKey = Guid.NewGuid().ToString("N");
            return Task.FromResult(LastSavedKey);
        }

        public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberDocumentUploadService.");

        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberDocumentUploadService.");
    }

    private sealed class FakeAuditLogger : IAuditLogger
    {
        public AuditEntry? LoggedEntry { get; private set; }

        public Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
        {
            LoggedEntry = entry;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
