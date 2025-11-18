using Microsoft.Extensions.DependencyInjection;

namespace GradingSystem.Shared.Services.BlobStorage;
public static class BlobServiceExtensions
{
    public static IServiceCollection AddBlobService(
        this IServiceCollection services)
    {
        services.AddScoped<IBlobService, BlobService>();
        return services;
    }
}
