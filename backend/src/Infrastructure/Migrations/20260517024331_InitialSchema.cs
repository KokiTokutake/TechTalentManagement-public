using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeckTalentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvaluationPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GoalStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GoalEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EvaluationStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EvaluationEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ApprovalDeadline = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntraObjectId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Goals_EvaluationPeriods_EvaluationPeriodId",
                        column: x => x.EvaluationPeriodId,
                        principalTable: "EvaluationPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Goals_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SelfEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SelfEvaluations_EvaluationPeriods_EvaluationPeriodId",
                        column: x => x.EvaluationPeriodId,
                        principalTable: "EvaluationPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SelfEvaluations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManagerEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelfEvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    FeedbackComment = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagerEvaluations_SelfEvaluations_SelfEvaluationId",
                        column: x => x.SelfEvaluationId,
                        principalTable: "SelfEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManagerEvaluations_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SelfEvaluationItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelfEvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AchievementScore = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfEvaluationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SelfEvaluationItems_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SelfEvaluationItems_SelfEvaluations_SelfEvaluationId",
                        column: x => x.SelfEvaluationId,
                        principalTable: "SelfEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationPeriods_Year",
                table: "EvaluationPeriods",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_EvaluationPeriodId",
                table: "Goals",
                column: "EvaluationPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_UserId_EvaluationPeriodId_Category",
                table: "Goals",
                columns: new[] { "UserId", "EvaluationPeriodId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_ManagerEvaluations_ManagerId",
                table: "ManagerEvaluations",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerEvaluations_SelfEvaluationId",
                table: "ManagerEvaluations",
                column: "SelfEvaluationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SelfEvaluationItems_GoalId",
                table: "SelfEvaluationItems",
                column: "GoalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SelfEvaluationItems_SelfEvaluationId_GoalId",
                table: "SelfEvaluationItems",
                columns: new[] { "SelfEvaluationId", "GoalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SelfEvaluations_EvaluationPeriodId",
                table: "SelfEvaluations",
                column: "EvaluationPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_SelfEvaluations_UserId_EvaluationPeriodId",
                table: "SelfEvaluations",
                columns: new[] { "UserId", "EvaluationPeriodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EntraObjectId",
                table: "Users",
                column: "EntraObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ManagerId",
                table: "Users",
                column: "ManagerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagerEvaluations");

            migrationBuilder.DropTable(
                name: "SelfEvaluationItems");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "SelfEvaluations");

            migrationBuilder.DropTable(
                name: "EvaluationPeriods");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
