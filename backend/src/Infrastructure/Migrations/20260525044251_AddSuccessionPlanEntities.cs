using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechTalentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSuccessionPlanEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CriticalPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CurrentHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Importance = table.Column<int>(type: "int", nullable: false),
                    RequiredSkills = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriticalPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriticalPositions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CriticalPositions_Users_CurrentHolderId",
                        column: x => x.CurrentHolderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SuccessionCandidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Readiness = table.Column<int>(type: "int", nullable: false),
                    DevelopmentGap = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DevelopmentActions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NominatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NominatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuccessionCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuccessionCandidates_CriticalPositions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "CriticalPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SuccessionCandidates_Users_NominatedBy",
                        column: x => x.NominatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuccessionCandidates_Users_TalentId",
                        column: x => x.TalentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SuccessionDevelopmentActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuccessionDevelopmentActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuccessionDevelopmentActions_SuccessionCandidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "SuccessionCandidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CriticalPositions_CreatedBy",
                table: "CriticalPositions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CriticalPositions_CurrentHolderId",
                table: "CriticalPositions",
                column: "CurrentHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_CriticalPositions_Importance_IsActive",
                table: "CriticalPositions",
                columns: new[] { "Importance", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SuccessionCandidates_NominatedBy",
                table: "SuccessionCandidates",
                column: "NominatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SuccessionCandidates_PositionId_Readiness",
                table: "SuccessionCandidates",
                columns: new[] { "PositionId", "Readiness" });

            migrationBuilder.CreateIndex(
                name: "IX_SuccessionCandidates_PositionId_TalentId",
                table: "SuccessionCandidates",
                columns: new[] { "PositionId", "TalentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuccessionCandidates_TalentId",
                table: "SuccessionCandidates",
                column: "TalentId");

            migrationBuilder.CreateIndex(
                name: "IX_SuccessionDevelopmentActions_CandidateId_Status",
                table: "SuccessionDevelopmentActions",
                columns: new[] { "CandidateId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuccessionDevelopmentActions");

            migrationBuilder.DropTable(
                name: "SuccessionCandidates");

            migrationBuilder.DropTable(
                name: "CriticalPositions");
        }
    }
}
