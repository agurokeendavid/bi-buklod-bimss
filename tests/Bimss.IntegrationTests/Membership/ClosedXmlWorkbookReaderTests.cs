using Bimss.Infrastructure.Membership;
using Bimss.IntegrationTests.Support;

namespace Bimss.IntegrationTests.Membership;

public class ClosedXmlWorkbookReaderTests
{
    [Fact]
    public void ReadRows_MapsCellsToTheirColumnHeader()
    {
        using var content = new MemoryStream(ExcelFixtures.BuildWorkbook(
            headers: ["Last Name", "First Name", "BI Employee Number"],
            rows:
            [
                ["Dela Cruz", "Juan", "BI-00123"],
                ["Santos", "Ana", "BI-00456"],
            ]));
        var reader = new ClosedXmlWorkbookReader();

        var rows = reader.ReadRows(content);

        Assert.Equal(2, rows.Count);
        Assert.Equal("Dela Cruz", rows[0]["Last Name"]);
        Assert.Equal("Juan", rows[0]["First Name"]);
        Assert.Equal("BI-00123", rows[0]["BI Employee Number"]);
        Assert.Equal("Santos", rows[1]["Last Name"]);
    }

    [Fact]
    public void ReadRows_ReturnsEmptyValue_ForBlankCells()
    {
        using var content = new MemoryStream(ExcelFixtures.BuildWorkbook(
            headers: ["Last Name", "Middle Name"],
            rows: [["Dela Cruz", ""]]));
        var reader = new ClosedXmlWorkbookReader();

        var rows = reader.ReadRows(content);

        Assert.Null(rows[0]["Middle Name"]);
    }

    [Fact]
    public void ReadRows_ReturnsEmptyList_ForAWorkbookWithOnlyAHeaderRow()
    {
        using var content = new MemoryStream(ExcelFixtures.BuildWorkbook(headers: ["Last Name", "First Name"], rows: []));
        var reader = new ClosedXmlWorkbookReader();

        var rows = reader.ReadRows(content);

        Assert.Empty(rows);
    }

    [Fact]
    public void ReadRows_Throws_ForAStreamThatIsNotAWorkbook()
    {
        using var content = new MemoryStream([1, 2, 3, 4, 5]);
        var reader = new ClosedXmlWorkbookReader();

        Assert.ThrowsAny<Exception>(() => reader.ReadRows(content));
    }
}
