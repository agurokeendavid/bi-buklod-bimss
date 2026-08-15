using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberContactPersistenceTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task MemberContact_RoundTrips_ThroughPersistence()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var contact = new MemberContact(id, memberId, "(02) 8123-4567", "09171234567", "juan@example.com");
            writeContext.MemberContacts.Add(contact);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberContacts.SingleAsync();

        Assert.Equal(id, persisted.Id);
        Assert.Equal(memberId, persisted.MemberId);
        Assert.Equal("(02) 8123-4567", persisted.Landline);
        Assert.Equal("09171234567", persisted.MobileNumber);
        Assert.Equal("juan@example.com", persisted.Email);
    }

    [Fact]
    public async Task UpdateDetails_Persists_AcrossReloads()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var contact = new MemberContact(Guid.NewGuid(), memberId, "(02) 8123-4567", "09171234567", "juan@example.com");
            writeContext.MemberContacts.Add(contact);
            await writeContext.SaveChangesAsync();
        }

        await using (var updateContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var contact = await updateContext.MemberContacts.SingleAsync();
            contact.UpdateDetails(null, "09179876543", "juan.new@example.com");
            await updateContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberContacts.SingleAsync();

        Assert.Null(persisted.Landline);
        Assert.Equal("09179876543", persisted.MobileNumber);
        Assert.Equal("juan.new@example.com", persisted.Email);
    }
}
