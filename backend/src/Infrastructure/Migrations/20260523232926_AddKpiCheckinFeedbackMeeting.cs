using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechTalentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKpiCheckinFeedbackMeeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedbackMeetings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelfEvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConductedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    OverallComment = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    NextPeriodExpectation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TalentReactionNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDisclosed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DisclosedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackMeetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackMeetings_SelfEvaluations_SelfEvaluationId",
                        column: x => x.SelfEvaluationId,
                        principalTable: "SelfEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedbackMeetings_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedbackMeetings_Users_TalentId",
                        column: x => x.TalentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoalKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MeasurementMethod = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalKpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalKpis_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoalCheckins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KpiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ProgressPercent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CheckedInAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalCheckins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalCheckins_GoalKpis_KpiId",
                        column: x => x.KpiId,
                        principalTable: "GoalKpis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GoalCheckins_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoalCheckins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackMeetings_ManagerId",
                table: "FeedbackMeetings",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackMeetings_SelfEvaluationId",
                table: "FeedbackMeetings",
                column: "SelfEvaluationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackMeetings_TalentId_IsDisclosed",
                table: "FeedbackMeetings",
                columns: new[] { "TalentId", "IsDisclosed" });

            migrationBuilder.CreateIndex(
                name: "IX_GoalCheckins_GoalId_CheckedInAt",
                table: "GoalCheckins",
                columns: new[] { "GoalId", "CheckedInAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GoalCheckins_KpiId",
                table: "GoalCheckins",
                column: "KpiId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalCheckins_UserId",
                table: "GoalCheckins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalKpis_GoalId",
                table: "GoalKpis",
                column: "GoalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackMeetings");

            migrationBuilder.DropTable(
                name: "GoalCheckins");

            migrationBuilder.DropTable(
                name: "GoalKpis");
        }
    }
}
