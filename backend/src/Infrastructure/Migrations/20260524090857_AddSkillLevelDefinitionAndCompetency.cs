using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechTalentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillLevelDefinitionAndCompetency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompetencyMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetencyMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SkillLevelDefinitions",
                columns: table => new
                {
                    SkillItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Examples = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillLevelDefinitions", x => new { x.SkillItemId, x.Level });
                    table.ForeignKey(
                        name: "FK_SkillLevelDefinitions_SkillItems_SkillItemId",
                        column: x => x.SkillItemId,
                        principalTable: "SkillItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillLevelMigrationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExecutedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExecutedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AffectedCount = table.Column<int>(type: "int", nullable: false),
                    ConversionRuleJson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillLevelMigrationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillLevelMigrationLogs_Users_ExecutedBy",
                        column: x => x.ExecutedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompetencyEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EvaluatorType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetencyEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetencyEvaluations_CompetencyMasters_CompetencyId",
                        column: x => x.CompetencyId,
                        principalTable: "CompetencyMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetencyEvaluations_EvaluationPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "EvaluationPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetencyEvaluations_Users_EvaluatorId",
                        column: x => x.EvaluatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetencyEvaluations_Users_TalentId",
                        column: x => x.TalentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetencyEvaluations_CompetencyId",
                table: "CompetencyEvaluations",
                column: "CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetencyEvaluations_EvaluatorId",
                table: "CompetencyEvaluations",
                column: "EvaluatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetencyEvaluations_PeriodId",
                table: "CompetencyEvaluations",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetencyEvaluations_TalentId_EvaluatorId_CompetencyId_PeriodId_EvaluatorType",
                table: "CompetencyEvaluations",
                columns: new[] { "TalentId", "EvaluatorId", "CompetencyId", "PeriodId", "EvaluatorType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetencyMasters_Category_Name",
                table: "CompetencyMasters",
                columns: new[] { "Category", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillLevelMigrationLogs_ExecutedBy",
                table: "SkillLevelMigrationLogs",
                column: "ExecutedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetencyEvaluations");

            migrationBuilder.DropTable(
                name: "SkillLevelDefinitions");

            migrationBuilder.DropTable(
                name: "SkillLevelMigrationLogs");

            migrationBuilder.DropTable(
                name: "CompetencyMasters");
        }
    }
}
