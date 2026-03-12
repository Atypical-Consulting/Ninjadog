using Ninjadog.Engine.Core.ValueObjects;

namespace Ninjadog.Engine.Infrastructure.OutputProcessors;

/// <summary>
/// An output processor that writes generated content to the filesystem.
/// This class implements the <see cref="IOutputProcessor"/> interface and provides functionality to write
/// output content to disk, facilitating persistence and file-based operations.
/// </summary>
public class DiskOutputProcessor(INinjadogAppService ninjadogAppService)
    : IDiskOutputProcessor
{
    /// <summary>
    /// Processes the given content by writing it to the filesystem.
    /// </summary>
    /// <param name="templateManifest">The template manifest to be used by the engine.</param>
    /// <param name="ninjadogSettings">The ninjadog app settings to configure the engine.</param>
    /// <param name="contentFile">The content file to be generated and written to disk.</param>
    public void ProcessOutput(
        NinjadogTemplateManifest templateManifest,
        NinjadogSettings ninjadogSettings,
        NinjadogContentFile contentFile)
    {
        ninjadogAppService.AddFileToProjectAsync(contentFile).Wait();
    }
}
