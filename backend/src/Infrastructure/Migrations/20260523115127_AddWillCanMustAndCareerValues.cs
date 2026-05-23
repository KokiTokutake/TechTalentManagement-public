using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechTalentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWillCanMustAndCareerValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CareerValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportantThings = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MotivationSituations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AvoidSituations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareerValues_EvaluationPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "EvaluationPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CareerValues_Users_TalentId",
                        column: x => x.TalentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WillCanMustEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    TimeHorizon = table.Column<int>(type: "int", nullable: true),
                    CanType = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    LinkedSkillItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WillCanMustEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WillCanMustEntries_EvaluationPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "EvaluationPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WillCanMustEntries_SkillItems_LinkedSkillItemId",
                        column: x => x.LinkedSkillItemId,
                        principalTable: "SkillItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WillCanMustEntries_Users_TalentId",
                        column: x => x.TalentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareerValues_PeriodId",
                table: "CareerValues",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerValues_TalentId_PeriodId",
                table: "CareerValues",
                columns: new[] { "TalentId", "PeriodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WillCanMustEntries_LinkedSkillItemId",
                table: "WillCanMustEntries",
                column: "LinkedSkillItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WillCanMustEntries_PeriodId",
                table: "WillCanMustEntries",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_WillCanMustEntries_TalentId_PeriodId_Category",
                table: "WillCanMustEntries",
                columns: new[] { "TalentId", "PeriodId", "Category" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CareerValues");

            migrationBuilder.DropTable(
                name: "WillCanMustEntries");
        }
    }
}
