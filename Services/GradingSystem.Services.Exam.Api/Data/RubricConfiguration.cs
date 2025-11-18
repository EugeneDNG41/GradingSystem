using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Services.Exams.Api.Data;

internal sealed class RubricConfiguration : IEntityTypeConfiguration<Rubric>
{
    public void Configure(EntityTypeBuilder<Rubric> builder)
    {
        builder.ToTable("Rubrics");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Criteria)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.MaxScore)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(r => r.OrderIndex)
            .IsRequired();
    }
}
