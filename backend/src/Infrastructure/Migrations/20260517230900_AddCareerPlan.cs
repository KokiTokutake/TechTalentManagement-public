using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechTalentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCareerPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CareerPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodYear = table.Column<int>(type: "int", nullable: false),
                    LongTermGoal = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MediumTermGoal = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CurrentFocus = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareerPlans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CareerPlanComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CareerPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerPlanComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareerPlanComments_CareerPlans_CareerPlanId",
                        column: x => x.CareerPlanId,
                        principalTable: "CareerPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CareerPlanComments_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareerPlanComments_AuthorId",
                table: "CareerPlanComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerPlanComments_CareerPlanId",
                table: "CareerPlanComments",
                column: "CareerPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerPlans_UserId_PeriodYear",
                table: "CareerPlans",
                columns: new[] { "UserId", "PeriodYear" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CareerPlanComments");

            migrationBuilder.DropTable(
                name: "CareerPlans");
        }
    }
}
