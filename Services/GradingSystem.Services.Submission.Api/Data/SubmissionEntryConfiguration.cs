using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Services.Submissions.Api.Data;

internal sealed class SubmissionEntryConfiguration : IEntityTypeConfiguration<SubmissionEntry>
{
    public void Configure(EntityTypeBuilder<SubmissionEntry> builder)
    {
        builder.ToTable("SubmissionEntries");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.StudentCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Metadata)
            .HasColumnType("jsonb");

        builder.Property(e => e.TextHash)
            .HasMaxLength(128);

        builder.Property(e => e.TemporaryScore)
            .HasPrecision(5, 2);

        builder.Property(e => e.ExtractedAt)
            .IsRequired();

        builder.HasIndex(e => e.StudentCode);
        builder.HasIndex(e => e.TextHash).IsUnique();

        builder.HasOne(e => e.SubmissionBatch)
            .WithMany(b => b.Entries)
            .HasForeignKey(e => e.SubmissionBatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

