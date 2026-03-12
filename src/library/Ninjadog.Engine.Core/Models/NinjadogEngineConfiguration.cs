namespace Ninjadog.Engine.Core.Models;

/// <summary>
/// Represents the configuration for creating a Ninjadog Engine instance,
/// including the template manifest, engine settings, and output processors.
/// This record is used to pass comprehensive configuration details to the Ninjadog Engine factory.
/// </summary>
/// <param name="TemplateManifest">The template manifest to be used by the Ninjadog Engine.</param>
/// <param name="NinjadogSettings">The settings for the Ninjadog Engine, including entity definitions and global configurations.</param>
/// <param name="OutputProcessors">A collection of output processors for handling the output of generated templates.</param>
public sealed record NinjadogEngineConfiguration(
    NinjadogTemplateManifest TemplateManifest,
    NinjadogSettings NinjadogSettings,
    NinjadogOutputProcessors OutputProcessors);
