using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Exams.Api.Data;

public class ExamsDbContext(DbContextOptions<ExamsDbContext> options) : DbContext(options)
{
}
