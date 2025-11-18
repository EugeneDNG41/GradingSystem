using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Services.Submissions.Api.Data;

internal sealed class SubmissionAssetConfiguration : IEntityTypeConfiguration<SubmissionAsset>
{
    public void Configure(EntityTypeBuilder<SubmissionAsset> builder)
    {
        builder.ToTable("SubmissionAssets");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.MimeType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.Content)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.HasOne(a => a.SubmissionEntry)
            .WithMany(e => e.Assets)
            .HasForeignKey(a => a.SubmissionEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

