using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Services.Submissions.Api.Data
{
    public class CachedSemesterConfiguration : IEntityTypeConfiguration<CachedSemester>
    {
        public void Configure(EntityTypeBuilder<CachedSemester> builder)
        {
            builder.ToTable("CachedSemesters");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
        }
    }

}
