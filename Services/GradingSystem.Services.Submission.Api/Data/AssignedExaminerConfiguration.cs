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

        builder.Property(x => x.AssignedAt)
               .IsRequired();

        // UNIQUE (ExamId + ExaminerId)
        builder.HasIndex(x => new { x.ExamId, x.ExaminerId })
               .IsUnique()
               .HasDatabaseName("UX_AssignedExaminer_Exam_Examiner");

        // Optional indexes
        builder.HasIndex(x => x.ExaminerId)
               .HasDatabaseName("IX_AssignedExaminer_ExaminerId");

        builder.HasIndex(x => x.ExamId)
               .HasDatabaseName("IX_AssignedExaminer_ExamId");
    }
}
