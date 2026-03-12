using Microsoft.Extensions.DependencyInjection;
using Ninjadog.Engine.Infrastructure.OutputProcessors;
using Ninjadog.Engine.Infrastructure.Services;

namespace Ninjadog.Engine.Infrastructure;

/// <summary>
/// Provides extension methods for configuring the Ninjadog Engine infrastructure.
/// </summary>
public static class InfrastructureExtensions
{
    /// <summary>
    /// Adds the Ninjadog Engine infrastructure services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ICliDotnetService, CliDotnetService>();
        services.AddScoped<IDiskOutputProcessor, DiskOutputProcessor>();
        services.AddScoped<IInMemoryOutputProcessor, InMemoryOutputProcessor>();
        services.AddScoped<INinjadogAppService, NinjadogAppService>();

        return services;
    }
}
