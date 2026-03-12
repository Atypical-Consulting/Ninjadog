namespace Ninjadog.Engine.Core.Abstractions;

/// <summary>
/// Provides functionality to create and configure instances of the Ninjadog Engine.
/// </summary>
public interface INinjadogEngineFactory
{
    /// <summary>
    /// Creates and configures a new instance of the Ninjadog Engine using the specified configuration.
    /// This method initializes a Ninjadog Engine with the provided template manifest, settings,
    /// and output processors. It facilitates the creation of a tailored engine instance
    /// suitable for specific templating tasks.
    /// </summary>
    /// <returns>A configured instance of the Ninjadog Engine.</returns>
    INinjadogEngine CreateNinjadogEngine();
}
