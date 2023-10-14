using BlobStorageIntegration.Api.Services;
using BlobStorageIntegration.Api.Settings;

namespace BlobStorageIntegration.Api;

public static class Bootstrapper
{
    public static IServiceCollection AddAzureBlobService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BlobSettings>(configuration.GetSection(nameof(BlobSettings)));
        services.AddScoped<AzureBlobService>();
        return services;
    }
}