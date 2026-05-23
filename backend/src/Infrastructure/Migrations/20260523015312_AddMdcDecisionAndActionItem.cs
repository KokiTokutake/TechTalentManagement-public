using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechTalentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMdcDecisionAndActionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MdcDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionType = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DecidedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MdcDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MdcDecisions_CommitteeSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "CommitteeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MdcDecisions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MdcActionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AssigneeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MdcActionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MdcActionItems_CommitteeSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "CommitteeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MdcActionItems_MdcDecisions_DecisionId",
                        column: x => x.DecisionId,
                        principalTable: "MdcDecisions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MdcActionItems_Users_AssigneeId",
                        column: x => x.AssigneeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MdcDecisionTalents",
                columns: table => new
                {
                    DecisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MdcDecisionTalents", x => new { x.DecisionId, x.TalentId });
                    table.ForeignKey(
                        name: "FK_MdcDecisionTalents_MdcDecisions_DecisionId",
                        column: x => x.DecisionId,
                        principalTable: "MdcDecisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MdcDecisionTalents_Users_TalentId",
                        column: x => x.TalentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MdcActionItems_AssigneeId_Status",
                table: "MdcActionItems",
                columns: new[] { "AssigneeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MdcActionItems_DecisionId",
                table: "MdcActionItems",
                column: "DecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_MdcActionItems_SessionId",
                table: "MdcActionItems",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MdcDecisions_CreatedBy",
                table: "MdcDecisions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MdcDecisions_SessionId",
                table: "MdcDecisions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MdcDecisionTalents_TalentId",
                table: "MdcDecisionTalents",
                column: "TalentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MdcActionItems");

            migrationBuilder.DropTable(
                name: "MdcDecisionTalents");

            migrationBuilder.DropTable(
                name: "MdcDecisions");
        }
    }
}
