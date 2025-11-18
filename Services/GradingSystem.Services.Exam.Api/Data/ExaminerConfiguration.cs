using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Services.Exams.Api.Data
{
    internal sealed class ExaminerConfiguration : IEntityTypeConfiguration<Examiner>
    {
        public void Configure(EntityTypeBuilder<Examiner> builder)
        {
            builder.ToTable("Examiners");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
