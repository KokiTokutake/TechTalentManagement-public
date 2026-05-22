using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeckTalentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTalentManagerComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TalentManagerComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentChallenges = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DevelopmentPolicy = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExpectedRole = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalentManagerComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalentManagerComments_EvaluationPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "EvaluationPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TalentManagerComments_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TalentManagerComments_Users_TalentId",
                        column: x => x.TalentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TalentManagerComments_ManagerId",
                table: "TalentManagerComments",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_TalentManagerComments_PeriodId",
                table: "TalentManagerComments",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_TalentManagerComments_TalentId_PeriodId",
                table: "TalentManagerComments",
                columns: new[] { "TalentId", "PeriodId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TalentManagerComments");
        }
    }
}
