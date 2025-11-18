using JasperFx.CodeGeneration.Frames;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Exams.Api.Data;

public class ExamsDbContext(DbContextOptions<ExamsDbContext> options) : DbContext(options)
{
    public DbSet<Rubric> Rubrics => Set<Rubric>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExamsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

