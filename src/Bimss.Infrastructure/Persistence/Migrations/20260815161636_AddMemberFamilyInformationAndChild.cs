using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bimss.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberFamilyInformationAndChild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberChildren",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberChildren", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberChildren_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberFamilyInformation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpouseFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FatherFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MotherMaidenName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ParentsPresentAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberFamilyInformation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberFamilyInformation_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberChildren_MemberId",
                table: "MemberChildren",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberFamilyInformation_MemberId",
                table: "MemberFamilyInformation",
                column: "MemberId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberChildren");

            migrationBuilder.DropTable(
                name: "MemberFamilyInformation");
        }
    }
}
