using Ninjadog.Engine.Core.ValueObjects;

namespace Ninjadog.Engine.Infrastructure.OutputProcessors;

/// <summary>
/// An output processor that stores generated content in memory.
/// This class implements the IOutputProcessor interface and provides a mechanism for storing
/// output content within an internal memory structure.
/// </summary>
public class InMemoryOutputProcessor : IInMemoryOutputProcessor
{
    /// <summary>
    /// Gets the memory storage that holds the generated content.
    /// </summary>
    public Dictionary<string, string> MemoryStorage { get; } = [];

    /// <summary>
    /// Processes the given content by storing it in memory.
    /// </summary>
    /// <param name="templateManifest">The template manifest to be used by the engine.</param>
    /// <param name="ninjadogSettings">The ninjadog app settings to configure the engine.</param>
    /// <param name="contentFile">The content file to be generated and stored.</param>
    public void ProcessOutput(
        NinjadogTemplateManifest templateManifest,
        NinjadogSettings ninjadogSettings,
        NinjadogContentFile contentFile)
    {
        // raise an exception if the output path is already present in the memory storage
        if (MemoryStorage.ContainsKey(contentFile.Key))
        {
            throw new InvalidOperationException(
                $"The key {contentFile.Key} is already present in the memory storage.");
        }

        MemoryStorage[contentFile.Key] = contentFile.Content;
    }
}
