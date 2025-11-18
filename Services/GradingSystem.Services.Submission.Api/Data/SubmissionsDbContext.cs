using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Submissions.Api.Data;

public class SubmissionsDbContext(DbContextOptions<SubmissionsDbContext> options) : DbContext(options)
{
    public DbSet<SubmissionFile> SubmissionFiles => Set<SubmissionFile>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubmissionsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}