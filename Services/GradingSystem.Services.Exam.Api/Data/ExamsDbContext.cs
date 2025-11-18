using JasperFx.CodeGeneration.Frames;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Exams.Api.Data;

public class ExamsDbContext(DbContextOptions<ExamsDbContext> options) : DbContext(options)
{
    public DbSet<Rubric> Rubrics => Set<Rubric>();
    public DbSet<Examiner> Examiners => Set<Examiner>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamExaminer> ExamExaminers => Set<ExamExaminer>();
    public DbSet<Semester> Semesters => Set<Semester>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExamsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

