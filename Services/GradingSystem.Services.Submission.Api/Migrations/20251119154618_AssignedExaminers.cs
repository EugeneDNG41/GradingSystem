using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GradingSystem.Services.Submissions.Api.Migrations
{
    /// <inheritdoc />
    public partial class AssignedExaminers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assigned_examiners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExamId = table.Column<int>(type: "integer", nullable: false),
                    ExaminerId = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assigned_examiners", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignedExaminer_ExamId",
                table: "assigned_examiners",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedExaminer_ExaminerId",
                table: "assigned_examiners",
                column: "ExaminerId");

            migrationBuilder.CreateIndex(
                name: "UX_AssignedExaminer_Exam_Examiner",
                table: "assigned_examiners",
                columns: new[] { "ExamId", "ExaminerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assigned_examiners");
        }
    }
}
