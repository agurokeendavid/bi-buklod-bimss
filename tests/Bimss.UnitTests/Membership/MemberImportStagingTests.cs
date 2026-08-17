using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberImportStagingTests
{
    [Fact]
    public void Constructor_Succeeds_WithRawFields()
    {
        var id = Guid.NewGuid();
        var importBatchId = Guid.NewGuid();
        var fields = new MemberImportStagingFields { LastName = "Dela Cruz", FirstName = "Juan", EmployeeNumber = "BI-00123" };

        var row = new MemberImportStaging(id, importBatchId, 1, fields);

        Assert.Equal(id, row.Id);
        Assert.Equal(importBatchId, row.ImportBatchId);
        Assert.Equal(1, row.RowNumber);
        Assert.Equal("Dela Cruz", row.LastName);
        Assert.Equal("Juan", row.FirstName);
        Assert.Equal("BI-00123", row.EmployeeNumber);
        Assert.Equal(ImportRowValidationStatus.NotValidated, row.ValidationStatus);
        Assert.Equal(ImportRowMatchStatus.NotEvaluated, row.MatchStatus);
        Assert.Null(row.MatchedMemberId);
        Assert.Null(row.PromotedMemberId);
    }

    [Fact]
    public void Constructor_Throws_WhenImportBatchIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new MemberImportStaging(Guid.NewGuid(), Guid.Empty, 1, new MemberImportStagingFields()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Throws_WhenRowNumberIsLessThanOne(int rowNumber)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new MemberImportStaging(Guid.NewGuid(), Guid.NewGuid(), rowNumber, new MemberImportStagingFields()));
    }

    [Fact]
    public void Constructor_Throws_WhenFieldsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MemberImportStaging(Guid.NewGuid(), Guid.NewGuid(), 1, null!));
    }

    [Fact]
    public void RecordValidation_SetsValid()
    {
        var row = CreateRow();

        row.RecordValidation(isValid: true);

        Assert.Equal(ImportRowValidationStatus.Valid, row.ValidationStatus);
    }

    [Fact]
    public void RecordValidation_SetsInvalid()
    {
        var row = CreateRow();

        row.RecordValidation(isValid: false);

        Assert.Equal(ImportRowValidationStatus.Invalid, row.ValidationStatus);
    }

    [Fact]
    public void RecordValidation_Throws_WhenAlreadyPromoted()
    {
        var row = CreateRow();
        row.RecordValidation(isValid: true);
        row.MarkPromoted(Guid.NewGuid());

        Assert.Throws<ConflictException>(() => row.RecordValidation(isValid: false));
    }

    [Fact]
    public void RecordMatch_SetsMatchedMemberAndStatus()
    {
        var row = CreateRow();
        var matchedMemberId = Guid.NewGuid();

        row.RecordMatch(matchedMemberId, ImportRowMatchStatus.PossibleDuplicate);

        Assert.Equal(matchedMemberId, row.MatchedMemberId);
        Assert.Equal(ImportRowMatchStatus.PossibleDuplicate, row.MatchStatus);
    }

    [Fact]
    public void RecordMatch_Throws_WhenStatusIsNotEvaluated()
    {
        var row = CreateRow();

        Assert.Throws<ArgumentException>(() => row.RecordMatch(null, ImportRowMatchStatus.NotEvaluated));
    }

    [Fact]
    public void RecordMatch_Throws_WhenAlreadyPromoted()
    {
        var row = CreateRow();
        row.RecordValidation(isValid: true);
        row.MarkPromoted(Guid.NewGuid());

        Assert.Throws<ConflictException>(() => row.RecordMatch(Guid.NewGuid(), ImportRowMatchStatus.NoMatch));
    }

    [Fact]
    public void MarkPromoted_SetsPromotedMemberId()
    {
        var row = CreateRow();
        row.RecordValidation(isValid: true);
        var memberId = Guid.NewGuid();

        row.MarkPromoted(memberId);

        Assert.Equal(memberId, row.PromotedMemberId);
    }

    [Fact]
    public void MarkPromoted_Throws_WhenNotValidated()
    {
        var row = CreateRow();

        Assert.Throws<ConflictException>(() => row.MarkPromoted(Guid.NewGuid()));
    }

    [Fact]
    public void MarkPromoted_Throws_WhenValidationFailed()
    {
        var row = CreateRow();
        row.RecordValidation(isValid: false);

        Assert.Throws<ConflictException>(() => row.MarkPromoted(Guid.NewGuid()));
    }

    [Fact]
    public void MarkPromoted_Throws_WhenAlreadyPromoted()
    {
        var row = CreateRow();
        row.RecordValidation(isValid: true);
        row.MarkPromoted(Guid.NewGuid());

        Assert.Throws<ConflictException>(() => row.MarkPromoted(Guid.NewGuid()));
    }

    [Fact]
    public void MarkPromoted_Throws_WhenMemberIdIsEmpty()
    {
        var row = CreateRow();
        row.RecordValidation(isValid: true);

        Assert.Throws<ArgumentException>(() => row.MarkPromoted(Guid.Empty));
    }

    private static MemberImportStaging CreateRow()
    {
        return new MemberImportStaging(Guid.NewGuid(), Guid.NewGuid(), 1, new MemberImportStagingFields { LastName = "Dela Cruz" });
    }
}
