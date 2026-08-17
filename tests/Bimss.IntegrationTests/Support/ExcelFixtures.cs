using ClosedXML.Excel;

namespace Bimss.IntegrationTests.Support;

// Builds a minimal in-memory .xlsx for tests that exercise real Excel
// parsing (ClosedXmlWorkbookReaderTests, ImportBatchesControllerTests) —
// avoids checking in binary fixture files.
public static class ExcelFixtures
{
    public static byte[] BuildWorkbook(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
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

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
