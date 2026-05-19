using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeckTalentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMdcEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommitteeSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeldOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Minutes = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitteeSessions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KeyPersons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DesignatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DesignatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeyPersons_Users_DesignatedBy",
                        column: x => x.DesignatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KeyPersons_Users_TalentId",
                        column: x => x.TalentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeSessionParticipants",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeSessionParticipants", x => new { x.SessionId, x.UserId });
                    table.ForeignKey(
                        name: "FK_CommitteeSessionParticipants_CommitteeSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "CommitteeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommitteeSessionParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeSessionKeyPersons",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeyPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeSessionKeyPersons", x => new { x.SessionId, x.KeyPersonId });
                    table.ForeignKey(
                        name: "FK_CommitteeSessionKeyPersons_CommitteeSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "CommitteeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommitteeSessionKeyPersons_KeyPersons_KeyPersonId",
                        column: x => x.KeyPersonId,
                        principalTable: "KeyPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DevelopmentPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeyPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Challenge = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Goal = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SupportAction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevelopmentPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlans_KeyPersons_KeyPersonId",
                        column: x => x.KeyPersonId,
                        principalTable: "KeyPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlans_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeSessionKeyPersons_KeyPersonId",
                table: "CommitteeSessionKeyPersons",
                column: "KeyPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeSessionParticipants_UserId",
                table: "CommitteeSessionParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeSessions_CreatedBy",
                table: "CommitteeSessions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlans_AuthorId",
                table: "DevelopmentPlans",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlans_KeyPersonId",
                table: "DevelopmentPlans",
                column: "KeyPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_KeyPersons_DesignatedBy",
                table: "KeyPersons",
                column: "DesignatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_KeyPersons_TalentId",
                table: "KeyPersons",
                column: "TalentId",
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitteeSessionKeyPersons");

            migrationBuilder.DropTable(
                name: "CommitteeSessionParticipants");

            migrationBuilder.DropTable(
                name: "DevelopmentPlans");

            migrationBuilder.DropTable(
                name: "CommitteeSessions");

            migrationBuilder.DropTable(
                name: "KeyPersons");
        }
    }
}
