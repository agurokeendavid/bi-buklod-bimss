using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberDocumentTests
{
    private static readonly DateTimeOffset UploadedAt = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_Succeeds_WithCoreFields()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var uploadedByUserId = Guid.NewGuid();

        var document = new MemberDocument(
            id, memberId, "ProofOfEmployment", "coe.pdf", "application/pdf", "a1b2c3", 102_400, UploadedAt, uploadedByUserId);

        Assert.Equal(id, document.Id);
        Assert.Equal(memberId, document.MemberId);
        Assert.Equal("ProofOfEmployment", document.DocumentType);
        Assert.Equal("coe.pdf", document.OriginalFileName);
        Assert.Equal("application/pdf", document.ContentType);
        Assert.Equal("a1b2c3", document.StorageKey);
        Assert.Equal(102_400, document.FileSizeBytes);
        Assert.Equal(UploadedAt, document.UploadedAtUtc);
        Assert.Equal(uploadedByUserId, document.UploadedByUserId);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    public void Constructor_Succeeds_WithAcceptedContentTypes(string contentType)
    {
        var document = CreateDocument(contentType: contentType);

        Assert.Equal(contentType, document.ContentType);
    }

    [Fact]
    public void Constructor_Throws_WhenContentTypeIsNotAccepted()
    {
        Assert.Throws<ArgumentException>(() => CreateDocument(contentType: "application/zip"));
    }

    [Fact]
    public void Constructor_Throws_WhenMemberIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => CreateDocument(memberId: Guid.Empty));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenDocumentTypeIsMissing(string? documentType)
    {
        Assert.ThrowsAny<ArgumentException>(() => CreateDocument(documentType: documentType!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenOriginalFileNameIsMissing(string? originalFileName)
    {
        Assert.ThrowsAny<ArgumentException>(() => CreateDocument(originalFileName: originalFileName!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenStorageKeyIsMissing(string? storageKey)
    {
        Assert.ThrowsAny<ArgumentException>(() => CreateDocument(storageKey: storageKey!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Throws_WhenFileSizeBytesIsNotPositive(long fileSizeBytes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateDocument(fileSizeBytes: fileSizeBytes));
    }

    [Fact]
    public void Constructor_Succeeds_WithNullUploadedByUserId()
    {
        var document = new MemberDocument(
            Guid.NewGuid(), Guid.NewGuid(), "ProofOfEmployment", "coe.pdf", "application/pdf", "a1b2c3", 102_400, UploadedAt, uploadedByUserId: null);

        Assert.Null(document.UploadedByUserId);
    }

    private static MemberDocument CreateDocument(
        Guid? memberId = null,
        string documentType = "ProofOfEmployment",
        string originalFileName = "coe.pdf",
        string contentType = "application/pdf",
        string storageKey = "a1b2c3",
        long fileSizeBytes = 102_400,
        Guid? uploadedByUserId = null)
    {
        return new MemberDocument(
            Guid.NewGuid(),
            memberId ?? Guid.NewGuid(),
            documentType,
            originalFileName,
            contentType,
            storageKey,
            fileSizeBytes,
            UploadedAt,
            uploadedByUserId ?? Guid.NewGuid());
    }
}
