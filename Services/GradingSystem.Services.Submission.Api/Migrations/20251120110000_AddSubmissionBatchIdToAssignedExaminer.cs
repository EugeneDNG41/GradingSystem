using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradingSystem.Services.Submissions.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionBatchIdToAssignedExaminer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_AssignedExaminer_Exam_Examiner",
                table: "assigned_examiners");

            migrationBuilder.AddColumn<int>(
                name: "SubmissionBatchId",
                table: "assigned_examiners",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignedExaminer_Batch_Examiner",
                table: "assigned_examiners",
                columns: new[] { "SubmissionBatchId", "ExaminerId" });

            migrationBuilder.CreateIndex(
                name: "IX_AssignedExaminer_SubmissionBatchId",
                table: "assigned_examiners",
                column: "SubmissionBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_assigned_examiners_SubmissionBatches_SubmissionBatchId",
                table: "assigned_examiners",
                column: "SubmissionBatchId",
                principalTable: "SubmissionBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Create partial unique index for ExamId + ExaminerId when SubmissionBatchId is NULL
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""UX_AssignedExaminer_Exam_Examiner""
                ON ""assigned_examiners"" (""ExamId"", ""ExaminerId"")
                WHERE ""SubmissionBatchId"" IS NULL;
            ");

            // Create partial unique index for SubmissionBatchId + ExaminerId when SubmissionBatchId is NOT NULL
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""UX_AssignedExaminer_Batch_Examiner""
                ON ""assigned_examiners"" (""SubmissionBatchId"", ""ExaminerId"")
                WHERE ""SubmissionBatchId"" IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assigned_examiners_SubmissionBatches_SubmissionBatchId",
                table: "assigned_examiners");

            migrationBuilder.DropIndex(
                name: "IX_AssignedExaminer_Batch_Examiner",
                table: "assigned_examiners");

            migrationBuilder.DropIndex(
                name: "IX_AssignedExaminer_SubmissionBatchId",
                table: "assigned_examiners");

            migrationBuilder.DropColumn(
                name: "SubmissionBatchId",
                table: "assigned_examiners");

            // Drop partial unique indexes
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""UX_AssignedExaminer_Batch_Examiner"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""UX_AssignedExaminer_Exam_Examiner"";");

            // Restore the old unique constraint
            migrationBuilder.CreateIndex(
                name: "UX_AssignedExaminer_Exam_Examiner",
                table: "assigned_examiners",
                columns: new[] { "ExamId", "ExaminerId" },
                unique: true);
        }
    }
}

