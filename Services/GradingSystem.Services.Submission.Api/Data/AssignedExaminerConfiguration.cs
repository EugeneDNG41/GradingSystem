using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Services.Submissions.Api.Data;

public class AssignedExaminerConfiguration : IEntityTypeConfiguration<AssignedExaminer>
{
    public void Configure(EntityTypeBuilder<AssignedExaminer> builder)
    {
        builder.ToTable("assigned_examiners");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.ExamId)
               .IsRequired();

        builder.Property(x => x.ExaminerId)
               .IsRequired();

        builder.Property(x => x.SubmissionBatchId)
               .IsRequired(false);

        builder.Property(x => x.AssignedAt)
               .IsRequired();

        // Foreign key relationship with SubmissionBatch
        builder.HasOne(x => x.SubmissionBatch)
               .WithMany()
               .HasForeignKey(x => x.SubmissionBatchId)
               .OnDelete(DeleteBehavior.SetNull);

        // UNIQUE (ExamId + ExaminerId) - removed as we now support multiple assignments per exam
        // Keep unique constraint only when SubmissionBatchId is null (exam-level assignment)
        // When SubmissionBatchId is set, allow multiple examiners per exam but unique per batch

        // Optional indexes
        builder.HasIndex(x => x.ExaminerId)
               .HasDatabaseName("IX_AssignedExaminer_ExaminerId");

        builder.HasIndex(x => x.ExamId)
               .HasDatabaseName("IX_AssignedExaminer_ExamId");

        builder.HasIndex(x => x.SubmissionBatchId)
               .HasDatabaseName("IX_AssignedExaminer_SubmissionBatchId");

        // Composite index for filtering (non-unique, partial unique indexes are created in migration)
        builder.HasIndex(x => new { x.SubmissionBatchId, x.ExaminerId })
               .HasDatabaseName("IX_AssignedExaminer_Batch_Examiner");

        // Note: Partial unique indexes are created manually in migration:
        // - UX_AssignedExaminer_Exam_Examiner (WHERE SubmissionBatchId IS NULL)
        // - UX_AssignedExaminer_Batch_Examiner (WHERE SubmissionBatchId IS NOT NULL)
    }
}
