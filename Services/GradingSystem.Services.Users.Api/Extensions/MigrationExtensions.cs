using GradingSystem.Services.Users.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Users.Api.Extensions;

internal static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using UsersDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        dbContext.Database.Migrate();
    }
}
