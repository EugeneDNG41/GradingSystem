using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Submissions.Api.Data;

public class SubmissionsDbContext(DbContextOptions<SubmissionsDbContext> options) : DbContext(options)
{
}