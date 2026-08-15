using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bimss.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberEmployment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberEmployments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PositionDesignation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OfficeUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermanentAppointmentDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberEmployments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberEmployments_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberEmployments_OfficeUnits_OfficeUnitId",
                        column: x => x.OfficeUnitId,
                        principalTable: "OfficeUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberEmployments_EmployeeNumber",
                table: "MemberEmployments",
                column: "EmployeeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberEmployments_MemberId",
                table: "MemberEmployments",
                column: "MemberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberEmployments_OfficeUnitId",
                table: "MemberEmployments",
                column: "OfficeUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberEmployments");
        }
    }
}
