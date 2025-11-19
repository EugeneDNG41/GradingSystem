using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Submissions.Api.Data;

public class SubmissionsDbContext(DbContextOptions<SubmissionsDbContext> options) : DbContext(options)
{
    public DbSet<SubmissionFile> SubmissionFiles => Set<SubmissionFile>();
    public DbSet<SubmissionBatch> SubmissionBatches => Set<SubmissionBatch>();
    public DbSet<SubmissionEntry> SubmissionEntries => Set<SubmissionEntry>();
    public DbSet<SubmissionAsset> SubmissionAssets => Set<SubmissionAsset>();
    public DbSet<SubmissionViolation> SubmissionViolations => Set<SubmissionViolation>();
    public DbSet<CachedExam> CachedExams => Set<CachedExam>();
    public DbSet<CachedSemester> CachedSemesters => Set<CachedSemester>();
    public DbSet<CachedExaminer> CachedExaminers => Set<CachedExaminer>();
    public DbSet<GradeEntry> GradeEntries => Set<GradeEntry>();
    public DbSet<AssignedExaminer> AssignedExaminers => Set<AssignedExaminer>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubmissionsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}