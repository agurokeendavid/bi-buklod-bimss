using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class ImportBatchTests
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_Succeeds_WithCoreFields()
    {
        var id = Guid.NewGuid();
        var uploadedByUserId = Guid.NewGuid();

        var batch = new ImportBatch(id, "legacy-members.xlsx", uploadedByUserId, OccurredAt);

        Assert.Equal(id, batch.Id);
        Assert.Equal("legacy-members.xlsx", batch.FileName);
        Assert.Equal(uploadedByUserId, batch.UploadedByUserId);
        Assert.Equal(OccurredAt, batch.UploadedAtUtc);
        Assert.Equal(ImportBatchStatus.Created, batch.Status);
        Assert.Null(batch.RowCount);
        Assert.Null(batch.StagedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenFileNameIsMissing(string? fileName)
    {
        Assert.ThrowsAny<ArgumentException>(() => new ImportBatch(Guid.NewGuid(), fileName!, Guid.NewGuid(), OccurredAt));
    }

    [Fact]
    public void Constructor_Throws_WhenUploadedByUserIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new ImportBatch(Guid.NewGuid(), "file.xlsx", Guid.Empty, OccurredAt));
    }

    [Fact]
    public void MarkStaged_TransitionsToStaged_AndRecordsRowCount()
    {
        var batch = CreateBatch();

        batch.MarkStaged(42, OccurredAt);

        Assert.Equal(ImportBatchStatus.Staged, batch.Status);
        Assert.Equal(42, batch.RowCount);
        Assert.Equal(OccurredAt, batch.StagedAtUtc);
    }

    [Fact]
    public void MarkStaged_Throws_WhenNotCreated()
    {
        var batch = CreateBatch();
        batch.MarkStaged(10, OccurredAt);

        Assert.Throws<ConflictException>(() => batch.MarkStaged(10, OccurredAt));
    }

    [Fact]
    public void MarkStaged_Throws_WhenRowCountIsNegative()
    {
        var batch = CreateBatch();

        Assert.Throws<ArgumentOutOfRangeException>(() => batch.MarkStaged(-1, OccurredAt));
    }

    [Fact]
    public void MarkValidated_TransitionsToValidated()
    {
        var batch = CreateBatch();
        batch.MarkStaged(10, OccurredAt);

        batch.MarkValidated(OccurredAt);

        Assert.Equal(ImportBatchStatus.Validated, batch.Status);
        Assert.Equal(OccurredAt, batch.ValidatedAtUtc);
    }

    [Fact]
    public void MarkValidated_Throws_WhenNotStaged()
    {
        var batch = CreateBatch();

        Assert.Throws<ConflictException>(() => batch.MarkValidated(OccurredAt));
    }

    [Fact]
    public void MarkPromoted_TransitionsToPromoted()
    {
        var batch = CreateBatch();
        batch.MarkStaged(10, OccurredAt);
        batch.MarkValidated(OccurredAt);

        batch.MarkPromoted(OccurredAt);

        Assert.Equal(ImportBatchStatus.Promoted, batch.Status);
        Assert.Equal(OccurredAt, batch.PromotedAtUtc);
    }

    [Fact]
    public void MarkPromoted_Throws_WhenNotValidated()
    {
        var batch = CreateBatch();

        Assert.Throws<ConflictException>(() => batch.MarkPromoted(OccurredAt));
    }

    [Fact]
    public void Cancel_TransitionsToCancelled_AndRecordsRemarks()
    {
        var batch = CreateBatch();

        batch.Cancel(OccurredAt, "Wrong file uploaded");

        Assert.Equal(ImportBatchStatus.Cancelled, batch.Status);
        Assert.Equal(OccurredAt, batch.CancelledAtUtc);
        Assert.Equal("Wrong file uploaded", batch.Remarks);
    }

    [Fact]
    public void Cancel_Throws_WhenAlreadyPromoted()
    {
        var batch = CreateBatch();
        batch.MarkStaged(10, OccurredAt);
        batch.MarkValidated(OccurredAt);
        batch.MarkPromoted(OccurredAt);

        Assert.Throws<ConflictException>(() => batch.Cancel(OccurredAt, "Too late"));
    }

    [Fact]
    public void Cancel_Throws_WhenAlreadyCancelled()
    {
        var batch = CreateBatch();
        batch.Cancel(OccurredAt, "First cancel");

        Assert.Throws<ConflictException>(() => batch.Cancel(OccurredAt, "Second cancel"));
    }

    private static ImportBatch CreateBatch()
    {
        return new ImportBatch(Guid.NewGuid(), "legacy-members.xlsx", Guid.NewGuid(), OccurredAt);
    }
}
