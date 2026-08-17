using Bimss.Infrastructure.Membership;
using ClosedXML.Excel;

namespace Bimss.IntegrationTests.Membership;

public class ClosedXmlWorkbookReaderTests
{
    [Fact]
    public void ReadRows_MapsCellsToTheirColumnHeader()
    {
        using var content = BuildWorkbook(
            headers: ["Last Name", "First Name", "BI Employee Number"],
            rows:
            [
                ["Dela Cruz", "Juan", "BI-00123"],
                ["Santos", "Ana", "BI-00456"],
            ]);
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
        using var content = BuildWorkbook(
            headers: ["Last Name", "Middle Name"],
            rows: [["Dela Cruz", ""]]);
        var reader = new ClosedXmlWorkbookReader();

        var rows = reader.ReadRows(content);

        Assert.Null(rows[0]["Middle Name"]);
    }

    [Fact]
    public void ReadRows_ReturnsEmptyList_ForAWorkbookWithOnlyAHeaderRow()
    {
        using var content = BuildWorkbook(headers: ["Last Name", "First Name"], rows: []);
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

    private static MemoryStream BuildWorkbook(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Form Responses 1");

        for (var column = 0; column < headers.Count; column++)
        {
            worksheet.Cell(1, column + 1).Value = headers[column];
        }

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            for (var column = 0; column < rows[rowIndex].Count; column++)
            {
                var value = rows[rowIndex][column];
                if (!string.IsNullOrEmpty(value))
                {
                    worksheet.Cell(rowIndex + 2, column + 1).Value = value;
                }
            }
        }

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
