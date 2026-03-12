namespace Ninjadog.Engine.Core.DomainEvents.Run;

/// <summary>
/// Handles events that occur before the engine starts processing.
/// </summary>
public class BeforeEngineRunProcessor : IDomainEventProcessor<BeforeEngineRunEvent>
{
    /// <summary>
    /// Handles the logic to be executed when the engine is about to start processing.
    /// </summary>
    /// <param name="domainEvent">The event containing details about the engine settings.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(BeforeEngineRunEvent domainEvent)
    {
        // Logic for handling the event before the engine starts processing
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
