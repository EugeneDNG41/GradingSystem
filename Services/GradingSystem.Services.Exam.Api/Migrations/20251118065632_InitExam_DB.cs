using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GradingSystem.Services.Exams.Api.Migrations;

/// <inheritdoc />
public partial class InitExam_DB : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "ExamId",
            table: "Rubrics",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "Semesters",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Semesters", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Exams",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                SemesterId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Exams", x => x.Id);
                table.ForeignKey(
                    name: "FK_Exams_Semesters_SemesterId",
                    column: x => x.SemesterId,
                    principalTable: "Semesters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ExamExaminers",
            columns: table => new
            {
                ExamId = table.Column<int>(type: "integer", nullable: false),
                UserId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamExaminers", x => new { x.ExamId, x.UserId });
                table.ForeignKey(
                    name: "FK_ExamExaminers_Exams_ExamId",
                    column: x => x.ExamId,
                    principalTable: "Exams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Rubrics_ExamId",
            table: "Rubrics",
            column: "ExamId");

        migrationBuilder.CreateIndex(
            name: "IX_Exams_SemesterId",
            table: "Exams",
            column: "SemesterId");

        migrationBuilder.AddForeignKey(
            name: "FK_Rubrics_Exams_ExamId",
            table: "Rubrics",
            column: "ExamId",
            principalTable: "Exams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Rubrics_Exams_ExamId",
            table: "Rubrics");

        migrationBuilder.DropTable(
            name: "ExamExaminers");

        migrationBuilder.DropTable(
            name: "Exams");

        migrationBuilder.DropTable(
            name: "Semesters");

        migrationBuilder.DropIndex(
            name: "IX_Rubrics_ExamId",
            table: "Rubrics");

        migrationBuilder.DropColumn(
            name: "ExamId",
            table: "Rubrics");
    }
}
