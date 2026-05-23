using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechTalentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNineBoxEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NineBoxAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceLevel = table.Column<int>(type: "int", nullable: false),
                    PotentialLevel = table.Column<int>(type: "int", nullable: false),
                    PerformanceRationale = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PotentialRationale = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AssessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NineBoxAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NineBoxAssessments_EvaluationPeriods_EvaluationPeriodId",
                        column: x => x.EvaluationPeriodId,
                        principalTable: "EvaluationPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NineBoxAssessments_Users_AssessorId",
                        column: x => x.AssessorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NineBoxAssessments_Users_TalentId",
                        column: x => x.TalentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NineBoxCellDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceLevel = table.Column<int>(type: "int", nullable: false),
                    PotentialLevel = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NineBoxCellDefinitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NineBoxAssessments_AssessorId",
                table: "NineBoxAssessments",
                column: "AssessorId");

            migrationBuilder.CreateIndex(
                name: "IX_NineBoxAssessments_EvaluationPeriodId_PerformanceLevel_PotentialLevel",
                table: "NineBoxAssessments",
                columns: new[] { "EvaluationPeriodId", "PerformanceLevel", "PotentialLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_NineBoxAssessments_TalentId_EvaluationPeriodId",
                table: "NineBoxAssessments",
                columns: new[] { "TalentId", "EvaluationPeriodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NineBoxCellDefinitions_PerformanceLevel_PotentialLevel",
                table: "NineBoxCellDefinitions",
                columns: new[] { "PerformanceLevel", "PotentialLevel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NineBoxAssessments");

            migrationBuilder.DropTable(
                name: "NineBoxCellDefinitions");
        }
    }
}
