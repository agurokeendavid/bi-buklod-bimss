using Bimss.Application.Auditing;
using Bimss.Application.Membership;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberContactSelfServiceUpdateServiceTests
{
    [Fact]
    public async Task UpdateAsync_Throws_NotFound_WhenMemberDoesNotExist()
    {
        var repository = new FakeMemberRepository { MemberExists = false };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberContactSelfServiceUpdateService(repository, auditLogger);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), null, "0917 000 0000", "member@example.com", null, null));

        Assert.Null(auditLogger.LoggedEntry);
    }

    [Fact]
    public async Task UpdateAsync_AddsContactAndAddresses_WhenNoneExistYet()
    {
        var memberId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var repository = new FakeMemberRepository { MemberExists = true };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberContactSelfServiceUpdateService(repository, auditLogger);

        await service.UpdateAsync(
            memberId, actorUserId, "8888-1234", "0917 000 0000", "member@example.com", "123 Present St.", "456 Permanent St.");

        Assert.NotNull(repository.AddedContact);
        Assert.Equal(memberId, repository.AddedContact!.MemberId);
        Assert.Equal("0917 000 0000", repository.AddedContact.MobileNumber);
        Assert.Equal("member@example.com", repository.AddedContact.Email);

        Assert.Equal(2, repository.AddedAddresses.Count);
        Assert.Contains(repository.AddedAddresses, a => a.AddressType == MemberAddressType.Present && a.AddressLine == "123 Present St.");
        Assert.Contains(repository.AddedAddresses, a => a.AddressType == MemberAddressType.Permanent && a.AddressLine == "456 Permanent St.");

        Assert.True(repository.SaveChangesCalled);
        Assert.NotNull(auditLogger.LoggedEntry);
        Assert.Equal("Member.UpdateContactInfo", auditLogger.LoggedEntry!.Action);
        Assert.Equal(actorUserId, auditLogger.LoggedEntry.ActorUserId);
        Assert.Equal(memberId.ToString(), auditLogger.LoggedEntry.ObjectId);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingContactAndAddresses_InPlace()
    {
        var memberId = Guid.NewGuid();
        var contact = new MemberContact(Guid.NewGuid(), memberId, "8888-1234", "0917 000 0000", "old@example.com");
        var presentAddress = new MemberAddress(Guid.NewGuid(), memberId, MemberAddressType.Present, "Old present address");
        var repository = new FakeMemberRepository
        {
            MemberExists = true,
            Contact = contact,
            Addresses = [presentAddress],
        };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberContactSelfServiceUpdateService(repository, auditLogger);

        await service.UpdateAsync(
            memberId, Guid.NewGuid(), null, "0918 111 1111", "new@example.com", "New present address", null);

        Assert.Null(repository.AddedContact);
        Assert.Equal("0918 111 1111", contact.MobileNumber);
        Assert.Equal("new@example.com", contact.Email);
        Assert.Equal("New present address", presentAddress.AddressLine);
        Assert.Empty(repository.AddedAddresses);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task UpdateAsync_LeavesAddressUntouched_WhenSubmittedValueIsBlank()
    {
        var memberId = Guid.NewGuid();
        var repository = new FakeMemberRepository { MemberExists = true };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberContactSelfServiceUpdateService(repository, auditLogger);

        await service.UpdateAsync(memberId, Guid.NewGuid(), null, "0917 000 0000", "member@example.com", null, "   ");

        Assert.Empty(repository.AddedAddresses);
    }

    private sealed class FakeMemberRepository : IMemberRepository
    {
        public bool MemberExists { get; set; }

        public MemberContact? Contact { get; set; }

        public IReadOnlyList<MemberAddress> Addresses { get; set; } = [];

        public MemberContact? AddedContact { get; private set; }

        public List<MemberAddress> AddedAddresses { get; } = [];

        public bool SaveChangesCalled { get; private set; }

        public Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberContactSelfServiceUpdateService.");

        public Task AddAsync(Member member, MemberEmployment employment, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberContactSelfServiceUpdateService.");

        public Task<Member?> GetTrackedByIdAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberContactSelfServiceUpdateService.");

        public Task<MemberEmployment?> GetTrackedEmploymentByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberContactSelfServiceUpdateService.");

        public Task<bool> ExistsAsync(Guid memberId, CancellationToken cancellationToken)
            => Task.FromResult(MemberExists);

        public Task AddDocumentAsync(MemberDocument document, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberContactSelfServiceUpdateService.");

        public Task<bool> HasAnyDocumentAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberContactSelfServiceUpdateService.");

        public Task<MemberContact?> GetTrackedContactByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => Task.FromResult(Contact);

        public Task AddContactAsync(MemberContact contact, CancellationToken cancellationToken)
        {
            AddedContact = contact;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<MemberAddress>> GetTrackedAddressesByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => Task.FromResult(Addresses);

        public Task AddAddressAsync(MemberAddress address, CancellationToken cancellationToken)
        {
            AddedAddresses.Add(address);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalled = true;
            return Task.CompletedTask;
        }
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
}
