namespace Ninjadog.Engine.Core.DomainEvents.Run;

/// <summary>
/// Handles events that occur after the engine has finished processing.
/// </summary>
public class AfterEngineRunProcessor
    : IDomainEventProcessor<AfterEngineRunEvent>
{
    /// <summary>
    /// Handles the logic to be executed when the engine has finished processing.
    /// </summary>
    /// <param name="domainEvent">The event containing details about the engine settings.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(AfterEngineRunEvent domainEvent)
    {
        // Logic for handling the event before the engine starts processing
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
