using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Services.Submissions.Api.Data;

internal sealed class SubmissionViolationConfiguration : IEntityTypeConfiguration<SubmissionViolation>
{
    public void Configure(EntityTypeBuilder<SubmissionViolation> builder)
    {
        builder.ToTable("SubmissionViolations");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(v => v.Severity)
            .IsRequired();

        builder.Property(v => v.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(v => v.AssetLink)
            .HasMaxLength(500);

        builder.HasOne(v => v.SubmissionEntry)
            .WithMany(e => e.Violations)
            .HasForeignKey(v => v.SubmissionEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.SubmissionAsset)
            .WithMany(a => a.LinkedViolations)
            .HasForeignKey(v => v.SubmissionAssetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

