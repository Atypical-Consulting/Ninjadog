using Microsoft.Extensions.DependencyInjection;

namespace Ninjadog.Engine;

/// <summary>
/// Provides functionality to create and configure instances of the Ninjadog Engine.
/// This factory class abstracts the construction process of the Ninjadog Engine,
/// allowing for flexible and streamlined engine setup based on given configurations.
/// </summary>
public class NinjadogEngineFactory(IServiceProvider serviceProvider)
    : INinjadogEngineFactory
{
    /// <inheritdoc/>
    public INinjadogEngine CreateNinjadogEngine()
    {
        var templateManifest = serviceProvider.GetRequiredService<NinjadogTemplateManifest>();
        var ninjadogSettings = serviceProvider.GetRequiredService<NinjadogSettings>();

        var saveGeneratedFiles = ninjadogSettings.Config.SaveGeneratedFiles;
        NinjadogOutputProcessors outputProcessors = new(serviceProvider, disk: saveGeneratedFiles);
        NinjadogEngineConfiguration configuration = new(templateManifest, ninjadogSettings, outputProcessors);

        return new NinjadogEngineBuilder()
            .WithManifest(configuration.TemplateManifest)
            .WithSettings(configuration.NinjadogSettings)
            .WithOutputProcessors(configuration.OutputProcessors)
            .WithServiceProvider(serviceProvider)
            .Build();
    }
}
