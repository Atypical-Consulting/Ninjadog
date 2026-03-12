namespace Ninjadog.Engine.Core.Abstractions;

/// <summary>
/// Defines the interface for the Ninjadog Engine, which handles the execution of template generation processes.
/// This interface encapsulates the core functionality of running the templating engine to produce output based on specified templates.
/// </summary>
public interface INinjadogEngine
{
    /// <summary>
    /// Gets the collection of domain events that have been triggered during the execution of the engine.
    /// </summary>
    ICollection<IDomainEvent> Events { get; }

    /// <summary>
    /// Gets the context of the Ninjadog Engine.
    /// </summary>
    NinjadogEngineContext Context { get; }

    /// <summary>
    /// Runs the Ninjadog Engine to process and generate templates.
    /// This method triggers the execution of the template generation logic encapsulated by the engine.
    /// </summary>
    void Run();
}
