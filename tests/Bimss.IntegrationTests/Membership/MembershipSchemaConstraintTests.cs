using Bimss.Domain.Membership;
using Bimss.Domain.Membership.ReferenceData;
using Bimss.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

// Real SQL Server constraint verification. EF Core InMemory (the provider
// used everywhere else in this project since BIMSS-011) does not enforce
// unique indexes/FK constraints and cannot run real migrations, so those
// specific guarantees can only be checked against a real database — see
// docs/PHASE1_BACKLOG.md's Environment notes.
//
// Runs for real only when BIMSS_TEST_SQLSERVER_CONNECTION_STRING is set
// (the CI workflow's SQL Server service container provides it). Locally,
// where that variable is normally unset, each test no-ops rather than
// failing — this is the only test class in the solution with that
// requirement; everything else still runs with no external dependency.
public class MembershipSchemaConstraintTests : IAsyncLifetime
{
    private const string ConnectionStringEnvironmentVariable = "BIMSS_TEST_SQLSERVER_CONNECTION_STRING";

    private string? _connectionString;
    private bool _isAvailable;

    public async Task InitializeAsync()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            _isAvailable = false;
            return;
        }

        var connectionStringBuilder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = $"BimssConstraintTests_{Guid.NewGuid():N}",
        };
        _connectionString = connectionStringBuilder.ConnectionString;
        _isAvailable = true;

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (!_isAvailable)
        {
            return;
        }

        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Migrations_ApplyCleanly_ToARealDatabase()
    {
        if (!_isAvailable)
        {
            return;
        }

        await using var dbContext = CreateDbContext();

        var applied = await dbContext.Database.GetAppliedMigrationsAsync();

        Assert.Contains(applied, migration => migration.EndsWith("_AddImportStagingSchema", StringComparison.Ordinal));
    }

    [Fact]
    public async Task EmployeeNumber_UniqueConstraint_IsEnforced()
    {
        if (!_isAvailable)
        {
            return;
        }

        var civilStatusId = await SeedCivilStatusAsync();
        var officeUnitId = await SeedOfficeUnitAsync();
        var employeeNumber = $"BI-{Guid.NewGuid():N}"[..15];

        await using (var dbContext = CreateDbContext())
        {
            var member = CreateMember(civilStatusId);
            dbContext.Members.Add(member);
            dbContext.MemberEmployments.Add(
                new MemberEmployment(Guid.NewGuid(), member.Id, employeeNumber, "Officer I", officeUnitId, null));
            await dbContext.SaveChangesAsync();
        }

        await using var conflictingContext = CreateDbContext();
        var otherMember = CreateMember(civilStatusId);
        conflictingContext.Members.Add(otherMember);
        conflictingContext.MemberEmployments.Add(
            new MemberEmployment(Guid.NewGuid(), otherMember.Id, employeeNumber, "Officer II", officeUnitId, null));

        await Assert.ThrowsAsync<DbUpdateException>(() => conflictingContext.SaveChangesAsync());
    }

    [Fact]
    public async Task MemberEmployment_OnePerMember_UniqueConstraint_IsEnforced()
    {
        if (!_isAvailable)
        {
            return;
        }

        var civilStatusId = await SeedCivilStatusAsync();
        var officeUnitId = await SeedOfficeUnitAsync();

        Guid memberId;
        await using (var dbContext = CreateDbContext())
        {
            var member = CreateMember(civilStatusId);
            memberId = member.Id;
            dbContext.Members.Add(member);
            dbContext.MemberEmployments.Add(
                new MemberEmployment(Guid.NewGuid(), memberId, $"BI-{Guid.NewGuid():N}"[..15], "Officer I", officeUnitId, null));
            await dbContext.SaveChangesAsync();
        }

        await using var conflictingContext = CreateDbContext();
        conflictingContext.MemberEmployments.Add(
            new MemberEmployment(Guid.NewGuid(), memberId, $"BI-{Guid.NewGuid():N}"[..15], "Officer II", officeUnitId, null));

        await Assert.ThrowsAsync<DbUpdateException>(() => conflictingContext.SaveChangesAsync());
    }

    [Fact]
    public async Task MemberAddress_UniquePerMemberAndType_IsEnforced()
    {
        if (!_isAvailable)
        {
            return;
        }

        var civilStatusId = await SeedCivilStatusAsync();

        Guid memberId;
        await using (var dbContext = CreateDbContext())
        {
            var member = CreateMember(civilStatusId);
            memberId = member.Id;
            dbContext.Members.Add(member);
            dbContext.MemberAddresses.Add(new MemberAddress(Guid.NewGuid(), memberId, MemberAddressType.Present, "123 Rizal St., Manila"));
            await dbContext.SaveChangesAsync();
        }

        await using var conflictingContext = CreateDbContext();
        conflictingContext.MemberAddresses.Add(
            new MemberAddress(Guid.NewGuid(), memberId, MemberAddressType.Present, "456 Bonifacio Ave., Manila"));

        await Assert.ThrowsAsync<DbUpdateException>(() => conflictingContext.SaveChangesAsync());
    }

    [Fact]
    public async Task CivilStatus_CannotBeDeleted_WhileReferencedByAMember()
    {
        if (!_isAvailable)
        {
            return;
        }

        var civilStatusId = await SeedCivilStatusAsync();

        await using (var dbContext = CreateDbContext())
        {
            dbContext.Members.Add(CreateMember(civilStatusId));
            await dbContext.SaveChangesAsync();
        }

        await using var deleteContext = CreateDbContext();
        var civilStatus = await deleteContext.CivilStatuses.SingleAsync(item => item.Id == civilStatusId);
        deleteContext.CivilStatuses.Remove(civilStatus);

        await Assert.ThrowsAsync<DbUpdateException>(() => deleteContext.SaveChangesAsync());
    }

    [Fact]
    public async Task DeletingMember_CascadesToStatusHistoryAndEmployment()
    {
        if (!_isAvailable)
        {
            return;
        }

        var civilStatusId = await SeedCivilStatusAsync();
        var officeUnitId = await SeedOfficeUnitAsync();

        Guid memberId;
        await using (var dbContext = CreateDbContext())
        {
            var member = CreateMember(civilStatusId);
            memberId = member.Id;
            dbContext.Members.Add(member);
            dbContext.MemberEmployments.Add(
                new MemberEmployment(Guid.NewGuid(), memberId, $"BI-{Guid.NewGuid():N}"[..15], "Officer I", officeUnitId, null));
            await dbContext.SaveChangesAsync();
        }

        await using (var deleteContext = CreateDbContext())
        {
            var member = await deleteContext.Members.SingleAsync(m => m.Id == memberId);
            deleteContext.Members.Remove(member);
            await deleteContext.SaveChangesAsync();
        }

        await using var readContext = CreateDbContext();
        Assert.False(await readContext.Members.AnyAsync(m => m.Id == memberId));
        Assert.False(await readContext.MemberStatusHistories.AnyAsync(h => h.MemberId == memberId));
        Assert.False(await readContext.MemberEmployments.AnyAsync(e => e.MemberId == memberId));
    }

    [Fact]
    public async Task MemberImportStaging_RowNumberUniquePerBatch_IsEnforced()
    {
        if (!_isAvailable)
        {
            return;
        }

        Guid batchId;
        await using (var dbContext = CreateDbContext())
        {
            var batch = new ImportBatch(Guid.NewGuid(), "legacy-members.xlsx", Guid.NewGuid(), DateTimeOffset.UtcNow);
            batchId = batch.Id;
            dbContext.ImportBatches.Add(batch);
            dbContext.MemberImportStagingRows.Add(
                new MemberImportStaging(Guid.NewGuid(), batchId, 1, new MemberImportStagingFields { LastName = "Dela Cruz" }));
            await dbContext.SaveChangesAsync();
        }

        await using var conflictingContext = CreateDbContext();
        conflictingContext.MemberImportStagingRows.Add(
            new MemberImportStaging(Guid.NewGuid(), batchId, 1, new MemberImportStagingFields { LastName = "Santos" }));

        await Assert.ThrowsAsync<DbUpdateException>(() => conflictingContext.SaveChangesAsync());
    }

    [Fact]
    public async Task MemberImportStaging_PromotedMemberUniqueConstraint_IsEnforced()
    {
        if (!_isAvailable)
        {
            return;
        }

        var civilStatusId = await SeedCivilStatusAsync();
        var batchId = Guid.NewGuid();
        Guid promotedMemberId;

        await using (var dbContext = CreateDbContext())
        {
            dbContext.ImportBatches.Add(new ImportBatch(batchId, "legacy-members.xlsx", Guid.NewGuid(), DateTimeOffset.UtcNow));

            var promotedMember = CreateMember(civilStatusId);
            promotedMemberId = promotedMember.Id;
            dbContext.Members.Add(promotedMember);

            var firstRow = new MemberImportStaging(Guid.NewGuid(), batchId, 1, new MemberImportStagingFields { LastName = "Dela Cruz" });
            firstRow.RecordValidation(isValid: true);
            firstRow.MarkPromoted(promotedMemberId);
            dbContext.MemberImportStagingRows.Add(firstRow);

            await dbContext.SaveChangesAsync();
        }

        await using var conflictingContext = CreateDbContext();
        var secondRow = new MemberImportStaging(Guid.NewGuid(), batchId, 2, new MemberImportStagingFields { LastName = "Dela Cruz" });
        secondRow.RecordValidation(isValid: true);
        secondRow.MarkPromoted(promotedMemberId);
        conflictingContext.MemberImportStagingRows.Add(secondRow);

        await Assert.ThrowsAsync<DbUpdateException>(() => conflictingContext.SaveChangesAsync());
    }

    [Fact]
    public async Task DeletingImportBatch_CascadesToStagingRowsAndValidationErrors()
    {
        if (!_isAvailable)
        {
            return;
        }

        Guid batchId;
        Guid rowId;
        await using (var dbContext = CreateDbContext())
        {
            var batch = new ImportBatch(Guid.NewGuid(), "legacy-members.xlsx", Guid.NewGuid(), DateTimeOffset.UtcNow);
            batchId = batch.Id;
            dbContext.ImportBatches.Add(batch);

            var row = new MemberImportStaging(Guid.NewGuid(), batchId, 1, new MemberImportStagingFields { LastName = "Dela Cruz" });
            rowId = row.Id;
            dbContext.MemberImportStagingRows.Add(row);

            dbContext.ImportValidationErrors.Add(new ImportValidationError(
                Guid.NewGuid(), batchId, rowId, "EmployeeNumber", ImportValidationSeverity.Error, "Employee number is required.", DateTimeOffset.UtcNow));

            await dbContext.SaveChangesAsync();
        }

        await using (var deleteContext = CreateDbContext())
        {
            var batch = await deleteContext.ImportBatches.SingleAsync(b => b.Id == batchId);
            deleteContext.ImportBatches.Remove(batch);
            await deleteContext.SaveChangesAsync();
        }

        await using var readContext = CreateDbContext();
        Assert.False(await readContext.ImportBatches.AnyAsync(b => b.Id == batchId));
        Assert.False(await readContext.MemberImportStagingRows.AnyAsync(r => r.ImportBatchId == batchId));
        Assert.False(await readContext.ImportValidationErrors.AnyAsync(e => e.ImportBatchId == batchId));
    }

    private async Task<Guid> SeedCivilStatusAsync()
    {
        await using var dbContext = CreateDbContext();
        var civilStatus = new CivilStatus(Guid.NewGuid(), $"CS-{Guid.NewGuid():N}"[..12], "Synthetic Civil Status");
        dbContext.CivilStatuses.Add(civilStatus);
        await dbContext.SaveChangesAsync();
        return civilStatus.Id;
    }

    private async Task<Guid> SeedOfficeUnitAsync()
    {
        await using var dbContext = CreateDbContext();
        var officeUnit = new OfficeUnit(Guid.NewGuid(), $"OU-{Guid.NewGuid():N}"[..12], "Synthetic Office Unit");
        dbContext.OfficeUnits.Add(officeUnit);
        await dbContext.SaveChangesAsync();
        return officeUnit.Id;
    }

    private static Member CreateMember(Guid civilStatusId)
    {
        return new Member(
            Guid.NewGuid(), "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
            civilStatusId, joiningReason: null, DateTimeOffset.UtcNow);
    }

    private BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseSqlServer(_connectionString, sql => sql.EnableRetryOnFailure());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
