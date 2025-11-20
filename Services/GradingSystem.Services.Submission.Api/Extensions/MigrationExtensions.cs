using GradingSystem.Services.Submissions.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Submissions.Api.Extensions;

internal static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using SubmissionsDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<SubmissionsDbContext>();
        
        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SubmissionsDbContext>>();
            logger.LogError(ex, "Error applying migrations");
            throw;
        }
    }
}
