using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechTalentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ManagerComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ManagerCommentedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerCommentedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectAssignments_Users_ManagerCommentedById",
                        column: x => x.ManagerCommentedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProjectAssignments_Users_TalentId",
                        column: x => x.TalentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectAssignmentSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAssignmentSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectAssignmentSkills_ProjectAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "ProjectAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectAssignmentSkills_SkillItems_SkillItemId",
                        column: x => x.SkillItemId,
                        principalTable: "SkillItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignments_ManagerCommentedById",
                table: "ProjectAssignments",
                column: "ManagerCommentedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignments_TalentId_EndDate",
                table: "ProjectAssignments",
                columns: new[] { "TalentId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignments_TalentId_StartDate",
                table: "ProjectAssignments",
                columns: new[] { "TalentId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignmentSkills_AssignmentId_SkillItemId",
                table: "ProjectAssignmentSkills",
                columns: new[] { "AssignmentId", "SkillItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignmentSkills_SkillItemId",
                table: "ProjectAssignmentSkills",
                column: "SkillItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectAssignmentSkills");

            migrationBuilder.DropTable(
                name: "ProjectAssignments");
        }
    }
}
