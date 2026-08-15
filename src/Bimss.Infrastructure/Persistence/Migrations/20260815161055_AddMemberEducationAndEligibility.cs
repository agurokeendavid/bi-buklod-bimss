using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bimss.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberEducationAndEligibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberEducations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HighestAttainmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DegreeCourse = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberEducations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberEducations_EducationalAttainments_HighestAttainmentId",
                        column: x => x.HighestAttainmentId,
                        principalTable: "EducationalAttainments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberEducations_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberEligibilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EligibilityTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberEligibilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberEligibilities_EligibilityTypes_EligibilityTypeId",
                        column: x => x.EligibilityTypeId,
                        principalTable: "EligibilityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberEligibilities_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberEducations_HighestAttainmentId",
                table: "MemberEducations",
                column: "HighestAttainmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberEducations_MemberId",
                table: "MemberEducations",
                column: "MemberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberEligibilities_EligibilityTypeId",
                table: "MemberEligibilities",
                column: "EligibilityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberEligibilities_MemberId",
                table: "MemberEligibilities",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberEducations");

            migrationBuilder.DropTable(
                name: "MemberEligibilities");
        }
    }
}
