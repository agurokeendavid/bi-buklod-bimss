using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bimss.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImportStagingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RowCount = table.Column<int>(type: "int", nullable: true),
                    StagedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ValidatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PromotedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberImportStaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    SubmittedAtRaw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmissionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirthRaw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CivilStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpouseFullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionDesignation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfficeUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PermanentAppointmentDateRaw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProofOfEmploymentNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HighestEducationalAttainment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegreeOrCourse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EligibilityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EligibilityDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PresentAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvincialAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Landline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChildrenRaw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherFullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherMaidenName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentsPresentAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeneficiariesRaw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JoiningReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrivacyConsentRaw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MatchedMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MatchStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PromotedMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberImportStaging", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberImportStaging_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberImportStaging_Members_MatchedMemberId",
                        column: x => x.MatchedMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberImportStaging_Members_PromotedMemberId",
                        column: x => x.PromotedMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportValidationErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberImportStagingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DetectedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportValidationErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportValidationErrors_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImportValidationErrors_MemberImportStaging_MemberImportStagingId",
                        column: x => x.MemberImportStagingId,
                        principalTable: "MemberImportStaging",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_UploadedByUserId",
                table: "ImportBatches",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportValidationErrors_ImportBatchId",
                table: "ImportValidationErrors",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportValidationErrors_MemberImportStagingId",
                table: "ImportValidationErrors",
                column: "MemberImportStagingId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberImportStaging_ImportBatchId_RowNumber",
                table: "MemberImportStaging",
                columns: new[] { "ImportBatchId", "RowNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberImportStaging_MatchedMemberId",
                table: "MemberImportStaging",
                column: "MatchedMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberImportStaging_PromotedMemberId",
                table: "MemberImportStaging",
                column: "PromotedMemberId",
                unique: true,
                filter: "[PromotedMemberId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportValidationErrors");

            migrationBuilder.DropTable(
                name: "MemberImportStaging");

            migrationBuilder.DropTable(
                name: "ImportBatches");
        }
    }
}
