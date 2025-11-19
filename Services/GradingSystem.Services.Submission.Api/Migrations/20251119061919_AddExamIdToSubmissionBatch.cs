using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradingSystem.Services.Submissions.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExamIdToSubmissionBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExamId",
                table: "SubmissionBatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionBatches_ExamId",
                table: "SubmissionBatches",
                column: "ExamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubmissionBatches_ExamId",
                table: "SubmissionBatches");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "SubmissionBatches");
        }
    }
}
