using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Services.Submissions.Api.Data;

public class GradeEntryConfiguration : IEntityTypeConfiguration<GradeEntry>
{
    public void Configure(EntityTypeBuilder<GradeEntry> builder)
    {
        builder.ToTable("grade_entries");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
               .ValueGeneratedOnAdd();

        builder.Property(g => g.SubmissionEntryId)
               .IsRequired();

        builder.HasOne(g => g.SubmissionEntry)
               .WithMany() 
               .HasForeignKey(g => g.SubmissionEntryId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(g => g.ExaminerId)
               .IsRequired();

        builder.Property(g => g.Score)
               .IsRequired()
               .HasPrecision(5, 2);

        builder.Property(g => g.Notes)
               .HasMaxLength(4000)
               .IsRequired(false);

        builder.Property(g => g.GradedAt)
               .IsRequired();

        builder.HasIndex(g => g.SubmissionEntryId)
               .HasDatabaseName("IX_GradeEntries_SubmissionEntryId");

        builder.HasIndex(g => g.ExaminerId)
               .HasDatabaseName("IX_GradeEntries_ExaminerId");
    }
}
