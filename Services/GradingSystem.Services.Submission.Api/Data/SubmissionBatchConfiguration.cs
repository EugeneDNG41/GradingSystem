using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Services.Submissions.Api.Data;

internal sealed class SubmissionBatchConfiguration : IEntityTypeConfiguration<SubmissionBatch>
{
    public void Configure(EntityTypeBuilder<SubmissionBatch> builder)
    {
        builder.ToTable("SubmissionBatches");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.SubmissionFileId).IsRequired();
        builder.Property(b => b.UploadedByUserId).IsRequired();
        builder.Property(b => b.Status).HasConversion<int>().IsRequired();
        builder.Property(b => b.UploadedAt).IsRequired();
        builder.Property(b => b.ProcessedAt);
        builder.Property(b => b.Notes).HasMaxLength(1000);

        builder.HasIndex(b => b.UploadedByUserId);
        builder.HasIndex(b => b.Status);

        builder.HasOne(b => b.SubmissionFile)
            .WithMany()
            .HasForeignKey(b => b.SubmissionFileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

