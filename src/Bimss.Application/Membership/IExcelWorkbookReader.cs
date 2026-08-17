namespace Bimss.Application.Membership;

// Application-layer port over "read this file as a table of named columns" —
// keeps ImportBatchIngestionService decoupled from the concrete Excel
// library, same reasoning as IMemberDocumentStorage decoupling
// MemberDocumentUploadService from the concrete file storage mechanism.
public interface IExcelWorkbookReader
{
    // Returns one dictionary per data row (header text -> cell text), in row
    // order, using the workbook's first row as the header. Throws if content
    // cannot be read as a workbook; the caller (ImportBatchIngestionService)
    // translates that into a validation error.
    IReadOnlyList<IReadOnlyDictionary<string, string?>> ReadRows(Stream content);
}
