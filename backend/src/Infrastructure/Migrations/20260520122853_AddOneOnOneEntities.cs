using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeckTalentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOneOnOneEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OneOnOneMeetings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConductedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    Format = table.Column<int>(type: "int", nullable: false),
                    WillTopics = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CanTopics = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MustTopics = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CurrentSituation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FreeText = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    ShareWithMdc = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneOnOneMeetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OneOnOneMeetings_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OneOnOneMeetings_Users_TalentId",
                        column: x => x.TalentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingActionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeetingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingActionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingActionItems_OneOnOneMeetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "OneOnOneMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingActionItems_MeetingId_Status",
                table: "MeetingActionItems",
                columns: new[] { "MeetingId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OneOnOneMeetings_ManagerId_TalentId",
                table: "OneOnOneMeetings",
                columns: new[] { "ManagerId", "TalentId" });

            migrationBuilder.CreateIndex(
                name: "IX_OneOnOneMeetings_TalentId_ConductedAt",
                table: "OneOnOneMeetings",
                columns: new[] { "TalentId", "ConductedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingActionItems");

            migrationBuilder.DropTable(
                name: "OneOnOneMeetings");
        }
    }
}
