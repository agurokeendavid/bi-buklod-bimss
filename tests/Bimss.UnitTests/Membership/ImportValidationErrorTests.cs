using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class ImportValidationErrorTests
{
    private static readonly DateTimeOffset DetectedAt = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_Succeeds_WithRowLevelFields()
    {
        var id = Guid.NewGuid();
        var importBatchId = Guid.NewGuid();
        var stagingId = Guid.NewGuid();

        var error = new ImportValidationError(
            id, importBatchId, stagingId, "EmployeeNumber", ImportValidationSeverity.Error, "Employee number is required.", DetectedAt);

        Assert.Equal(id, error.Id);
        Assert.Equal(importBatchId, error.ImportBatchId);
        Assert.Equal(stagingId, error.MemberImportStagingId);
        Assert.Equal("EmployeeNumber", error.FieldName);
        Assert.Equal(ImportValidationSeverity.Error, error.Severity);
        Assert.Equal("Employee number is required.", error.Message);
        Assert.Equal(DetectedAt, error.DetectedAtUtc);
    }

    [Fact]
    public void Constructor_Succeeds_WithBatchLevelFields()
    {
        var error = new ImportValidationError(
            Guid.NewGuid(),
            Guid.NewGuid(),
            memberImportStagingId: null,
            fieldName: null,
            ImportValidationSeverity.Error,
            "The file has no rows.",
            DetectedAt);

        Assert.Null(error.MemberImportStagingId);
        Assert.Null(error.FieldName);
    }

    [Fact]
    public void Constructor_Throws_WhenImportBatchIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new ImportValidationError(
            Guid.NewGuid(), Guid.Empty, null, null, ImportValidationSeverity.Error, "message", DetectedAt));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenMessageIsMissing(string? message)
    {
        Assert.ThrowsAny<ArgumentException>(() => new ImportValidationError(
            Guid.NewGuid(), Guid.NewGuid(), null, null, ImportValidationSeverity.Error, message!, DetectedAt));
    }
}
