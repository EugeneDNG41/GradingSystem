using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Services.Submissions.Api.Data;

public class CachedExamConfiguration : IEntityTypeConfiguration<CachedExam>
{
    public void Configure(EntityTypeBuilder<CachedExam> builder)
    {
        builder.ToTable("CachedExams");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.DueDate).IsRequired();
        builder.Property(e => e.SemesterId).IsRequired();
        builder.Property(e => e.CachedAt).IsRequired();
    }
}


