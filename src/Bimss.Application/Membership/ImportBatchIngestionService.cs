using System.Text.Json;
using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Application-layer orchestration for docs/DOMAIN_WORKFLOWS.md's first
// migration step: "Create Import Batch -> Load spreadsheet rows to
// staging." Deliberately does not validate row contents, match against
// existing members, or promote anything into real Member records — those
// are BIMSS-035/036/037. Rows are captured as raw, unvalidated
// MemberImportStagingFields, exactly as BIMSS-033 designed the staging
// schema to hold them.
public sealed class ImportBatchIngestionService(
    IExcelWorkbookReader workbookReader,
    IImportBatchRepository importBatchRepository,
    IAuditLogger auditLogger,
    TimeProvider timeProvider)
{
    public async Task<ImportBatchIngestionResult> IngestAsync(
        string fileName, Stream content, Guid uploadedByUserId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(content);

        IReadOnlyList<IReadOnlyDictionary<string, string?>> rawRows;
        try
        {
            rawRows = workbookReader.ReadRows(content);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Any failure to parse an arbitrary uploaded file degrades to a
            // single field-level validation error rather than a raw 500 —
            // the underlying Excel library's exception type isn't
            // meaningful to the officer reviewing the upload.
            throw new DomainValidationException(
                "The uploaded file could not be read.",
                new Dictionary<string, string[]>
                {
                    ["File"] = ["The uploaded file could not be read as an Excel workbook."],
                });
        }

        var occurredAtUtc = timeProvider.GetUtcNow();
        var batchId = Guid.NewGuid();
        var batch = new ImportBatch(batchId, fileName, uploadedByUserId, occurredAtUtc);

        var rows = new List<MemberImportStaging>(rawRows.Count);
        for (var index = 0; index < rawRows.Count; index++)
        {
            var fields = MapRow(rawRows[index]);
            rows.Add(new MemberImportStaging(Guid.NewGuid(), batchId, index + 1, fields));
        }

        batch.MarkStaged(rows.Count, occurredAtUtc);

        await importBatchRepository.AddBatchWithRowsAsync(batch, rows, cancellationToken);

        await auditLogger.LogAsync(
            new AuditEntry(uploadedByUserId, "ImportBatch.Ingest", "ImportBatch", batchId.ToString(), AuditResult.Success),
            cancellationToken);

        return new ImportBatchIngestionResult(batchId, rows.Count);
    }

    // Column headers match docs/DATA_DICTIONARY.md's Excel field mapping
    // table verbatim (its "Source field" column) — the Google Forms export
    // is expected to use that exact question text as its header row.
    private static MemberImportStagingFields MapRow(IReadOnlyDictionary<string, string?> row)
    {
        return new MemberImportStagingFields
        {
            SubmittedAtRaw = Get(row, "Timestamp"),
            FormEmail = Get(row, "Email Address"),
            SubmissionType = Get(row, "Type of Submission"),
            LastName = Get(row, "Last Name"),
            FirstName = Get(row, "First Name"),
            MiddleName = Get(row, "Middle Name"),
            Suffix = Get(row, "Suffix"),
            DateOfBirthRaw = Get(row, "Date of Birth"),
            PlaceOfBirth = Get(row, "Place of Birth"),
            CivilStatus = Get(row, "Civil Status"),
            SpouseFullName = Get(row, "Spouse's Full Name (If Married)"),
            EmployeeNumber = Get(row, "BI Employee Number"),
            PositionDesignation = Get(row, "Position/Designation"),
            OfficeUnit = Get(row, "Division/Section/Unit"),
            PermanentAppointmentDateRaw = Get(row, "Date of Permanent Appointment"),
            ProofOfEmploymentNote = Get(row, "Proof of Permanent Employment"),
            HighestEducationalAttainment = Get(row, "Highest Educational Attainment"),
            DegreeOrCourse = Get(row, "Degree or Course Completed"),
            EligibilityType = Get(row, "Civil Service Eligibility/PRC Eligibility"),
            EligibilityDetails = Get(row, "Eligibility or Professional License Details"),
            PresentAddress = Get(row, "Present Residential Address"),
            ProvincialAddress = Get(row, "Provincial/Permanent Address"),
            Landline = Get(row, "Landline Number"),
            MobileNumber = Get(row, "Cellphone Number"),
            Email = Get(row, "Current Email Address"),
            ChildrenRaw = Get(row, "Names of Children"),
            FatherFullName = Get(row, "Father's Full Name"),
            MotherMaidenName = Get(row, "Mother's Full Maiden Name"),
            ParentsPresentAddress = Get(row, "Parents' Present Address"),
            BeneficiariesRaw = BuildBeneficiariesRaw(row),
            JoiningReason = Get(row, "Reason for Joining Buklod"),
            PrivacyConsentRaw = Get(row, "Privacy Notice consent"),
        };
    }

    // Beneficiaries 1-4 are distinct, unambiguous name/relationship column
    // pairs, so they are losslessly captured here as structured JSON.
    // "Additional Beneficiaries (Beneficiary 5 and above)" is free text with
    // no agreed delimiter (docs/DATA_DICTIONARY.md: "do not auto-parse until
    // delimiter/format is agreed"), so it is carried through verbatim rather
    // than guessed at — splitting it into individual beneficiary rows is
    // BIMSS-036/037's job once that format is confirmed with Buklod.
    private static string? BuildBeneficiariesRaw(IReadOnlyDictionary<string, string?> row)
    {
        var beneficiaries = new List<RawBeneficiaryEntry>();
        for (var i = 1; i <= 4; i++)
        {
            var name = Get(row, $"Beneficiary {i} — Complete Name");
            var relationship = Get(row, $"Beneficiary {i} — Relationship to Member");
            if (name is not null || relationship is not null)
            {
                beneficiaries.Add(new RawBeneficiaryEntry(name, relationship));
            }
        }

        var additionalRaw = Get(row, "Additional Beneficiaries (Beneficiary 5 and above)");

        if (beneficiaries.Count == 0 && additionalRaw is null)
        {
            return null;
        }

        return JsonSerializer.Serialize(new RawBeneficiariesEnvelope(beneficiaries, additionalRaw));
    }

    private static string? Get(IReadOnlyDictionary<string, string?> row, string header)
    {
        return row.TryGetValue(header, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;
    }

    private sealed record RawBeneficiaryEntry(string? Name, string? Relationship);

    private sealed record RawBeneficiariesEnvelope(IReadOnlyList<RawBeneficiaryEntry> Beneficiaries, string? AdditionalRaw);
}
