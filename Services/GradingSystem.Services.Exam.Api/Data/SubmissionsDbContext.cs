using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Exams.Api.Data;

public class SubmissionsDbContext(DbContextOptions<SubmissionsDbContext> options) : DbContext(options)
{
}