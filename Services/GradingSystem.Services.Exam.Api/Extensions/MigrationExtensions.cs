using GradingSystem.Services.Exams.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Exams.Api.Extensions;

internal static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using ExamsDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ExamsDbContext>();
        dbContext.Database.Migrate();
    }
}
