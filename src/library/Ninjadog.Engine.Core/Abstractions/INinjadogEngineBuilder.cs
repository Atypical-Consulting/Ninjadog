namespace Ninjadog.Engine.Core.Abstractions;

/// <summary>
/// Defines the interface for a builder responsible for constructing and configuring instances of the Ninjadog Engine.
/// This interface allows for a fluent, step-by-step configuration of the engine before its creation.
/// </summary>
public interface INinjadogEngineBuilder
{
    /// <summary>
    /// Specifies the template manifest to use with the Ninjadog Engine.
    /// </summary>
    /// <param name="templateManifest">The template manifest to be used by the engine.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    INinjadogEngineBuilder WithManifest(NinjadogTemplateManifest templateManifest);

    /// <summary>
    /// Specifies the settings to use with the Ninjadog Engine.
    /// </summary>
    /// <param name="ninjadogSettings">The settings to configure the engine.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    INinjadogEngineBuilder WithSettings(NinjadogSettings ninjadogSettings);

    /// <summary>
    /// Specifies the output processors to use with the Ninjadog Engine.
    /// </summary>
    /// <param name="outputProcessors">The output processors to be used by the engine.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    INinjadogEngineBuilder WithOutputProcessors(NinjadogOutputProcessors outputProcessors);

    /// <summary>
    /// Specifies the service provider to use with the Ninjadog Engine.
    /// </summary>
    /// <param name="serviceProvider">The service provider to be used by the engine.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    INinjadogEngineBuilder WithServiceProvider(IServiceProvider serviceProvider);

    /// <summary>
    /// Builds and returns a configured instance of the Ninjadog Engine.
    /// </summary>
    /// <returns>A fully configured instance of the Ninjadog Engine.</returns>
    INinjadogEngine Build();
}
