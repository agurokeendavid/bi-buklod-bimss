using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bimss.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberCoreAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SuffixId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CivilStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoiningReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Members_CivilStatuses_CivilStatusId",
                        column: x => x.CivilStatusId,
                        principalTable: "CivilStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Members_Suffixes_SuffixId",
                        column: x => x.SuffixId,
                        principalTable: "Suffixes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReasonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberStatusHistory_MemberStatusReasons_ReasonId",
                        column: x => x.ReasonId,
                        principalTable: "MemberStatusReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberStatusHistory_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Members_CivilStatusId",
                table: "Members",
                column: "CivilStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_SuffixId",
                table: "Members",
                column: "SuffixId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberStatusHistory_ActorUserId",
                table: "MemberStatusHistory",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberStatusHistory_MemberId",
                table: "MemberStatusHistory",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberStatusHistory_OccurredAtUtc",
                table: "MemberStatusHistory",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_MemberStatusHistory_ReasonId",
                table: "MemberStatusHistory",
                column: "ReasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberStatusHistory");

            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
