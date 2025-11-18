using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GradingSystem.Services.Exams.Api.Migrations
{
    /// <inheritdoc />
    public partial class ExaminerEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Examiners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Examiners", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamExaminers_UserId",
                table: "ExamExaminers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamExaminers_Examiners_UserId",
                table: "ExamExaminers",
                column: "UserId",
                principalTable: "Examiners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamExaminers_Examiners_UserId",
                table: "ExamExaminers");

            migrationBuilder.DropTable(
                name: "Examiners");

            migrationBuilder.DropIndex(
                name: "IX_ExamExaminers_UserId",
                table: "ExamExaminers");
        }
    }
}
