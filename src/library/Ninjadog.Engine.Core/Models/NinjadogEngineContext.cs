using System.Diagnostics;

namespace Ninjadog.Engine.Core.Models;

/// <summary>
/// Represents the context of a Ninjadog Engine run, encapsulating metrics and state information.
/// </summary>
public sealed class NinjadogEngineContext
{
    private readonly Stopwatch _stopwatch;

    /// <summary>
    /// Initializes a new instance of the <see cref="NinjadogEngineContext"/> class.
    /// </summary>
    public NinjadogEngineContext()
    {
        _stopwatch = new Stopwatch();
    }

    /// <summary>
    /// Gets the total number of files generated during the engine run.
    /// </summary>
    public int TotalFilesGenerated { get; private set; }

    /// <summary>
    /// Gets the total number of characters generated across all files during the engine run.
    /// </summary>
    public int TotalCharactersGenerated { get; private set; }

    /// <summary>
    /// Gets the total time elapsed during the engine run.
    /// </summary>
    public TimeSpan TotalTimeElapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Updates the metrics to account for a newly generated file.
    /// </summary>
    /// <param name="charactersGenerated">The number of characters in the generated file.</param>
    public void FileGenerated(NinjadogContentFile charactersGenerated)
    {
        TotalFilesGenerated++;
        TotalCharactersGenerated += charactersGenerated.Length;
    }

    /// <summary>
    /// Updates the metrics to account for an error that occurred during the engine run.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
#pragma warning disable CA1822 // Member does not access instance data
    public void ErrorOccurred(Exception exception)
#pragma warning restore CA1822
    {
        // no-op
    }

    /// <summary>
    /// Resets the metrics and stopwatch to their initial state.
    /// </summary>
    public void Reset()
    {
        TotalFilesGenerated = 0;
        TotalCharactersGenerated = 0;
        _stopwatch.Reset();
    }

    /// <summary>
    /// Starts the stopwatch.
    /// </summary>
    public void StartCollectMetrics()
    {
        _stopwatch.Start();
    }

    /// <summary>
    /// Stops the stopwatch.
    /// </summary>
    public void StopCollectMetrics()
    {
        _stopwatch.Stop();
    }

    /// <summary>
    /// Creates a snapshot of the current context.
    /// </summary>
    /// <returns>A snapshot of the current context.</returns>
    public NinjadogEngineContextSnapshot GetSnapshot()
    {
        return new NinjadogEngineContextSnapshot(
            TotalFilesGenerated,
            TotalCharactersGenerated,
            TotalTimeElapsed);
    }
}
