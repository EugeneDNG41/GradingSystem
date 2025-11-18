using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Services.Exams.Api.Data
{
    public class ExamExaminerConfiguration : IEntityTypeConfiguration<ExamExaminer>
    {
        public void Configure(EntityTypeBuilder<ExamExaminer> builder)
        {
            builder.ToTable("ExamExaminers");

            builder.HasKey(x => new { x.ExamId, x.UserId });

            builder.HasOne<Exam>()            
                   .WithMany()
                   .HasForeignKey(x => x.ExamId);

            builder.HasOne<Examiner>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId);
        }
    }

}
