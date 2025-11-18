using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Services.Submissions.Api.Data;

internal sealed class SubmissionFileConfiguration : IEntityTypeConfiguration<SubmissionFile>
{
    public void Configure(EntityTypeBuilder<SubmissionFile> builder)
    {
        builder.ToTable("SubmissionFiles");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.BlobName).IsRequired().HasMaxLength(500);
        builder.Property(u => u.UploadDate).IsRequired();
    }
}
