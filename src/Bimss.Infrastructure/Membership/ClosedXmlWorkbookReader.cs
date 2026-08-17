using Bimss.Application.Membership;
using ClosedXML.Excel;

namespace Bimss.Infrastructure.Membership;

// ClosedXML (MIT) reads .xlsx only — this project's import source is a
// Google Forms export, so legacy .xls/CSV support is not needed.
public sealed class ClosedXmlWorkbookReader : IExcelWorkbookReader
{
    public IReadOnlyList<IReadOnlyDictionary<string, string?>> ReadRows(Stream content)
    {
        using var workbook = new XLWorkbook(content);
        var worksheet = workbook.Worksheets.First();
        var usedRows = worksheet.RowsUsed().ToList();

        if (usedRows.Count == 0)
        {
            return [];
        }

        var headers = usedRows[0].CellsUsed()
            .Select(cell => (Column: cell.Address.ColumnNumber, Header: cell.GetString().Trim()))
            .Where(header => !string.IsNullOrWhiteSpace(header.Header))
            .ToList();

        var rows = new List<IReadOnlyDictionary<string, string?>>();
        for (var rowIndex = 1; rowIndex < usedRows.Count; rowIndex++)
        {
            var row = usedRows[rowIndex];
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (var (column, header) in headers)
            {
                var cell = row.Cell(column);
                values[header] = cell.IsEmpty() ? null : cell.GetString();
            }

            rows.Add(values);
        }

        return rows;
    }
}
