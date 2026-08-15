using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bimss.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberPrivacyConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberPrivacyConsents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsentGiven = table.Column<bool>(type: "bit", nullable: false),
                    NoticeVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConsentedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberPrivacyConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberPrivacyConsents_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberPrivacyConsents_MemberId",
                table: "MemberPrivacyConsents",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberPrivacyConsents");
        }
    }
}
